using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions
{
	/// <inheritdoc />
	public class FileServiceInterfaceInjectionCodeAction : CodeAction
	{
		private readonly ConstructorDeclarationSyntax _constructor;

		private readonly Document _document;

		public FileServiceInterfaceInjectionCodeAction(string title, Document document, ConstructorDeclarationSyntax constructor)
		{
			_constructor = constructor;
			_document = document;
			Title = title;
		}

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);

			var parameter = SF.Parameter(SF.Identifier("fileSystem"))
				.WithType(SF.ParseTypeName(nameof(IFileSystem)))
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			if (!(_constructor.Parent is ClassDeclarationSyntax classDeclarationSyntax))
			{
				editor.AddParameter(_constructor, parameter);

				return editor.GetChangedDocument();
			}

			var fileSystem = classDeclarationSyntax.Members
				.OfType<FieldDeclarationSyntax>()
				.FirstOrDefault(x => x.NormalizeWhitespace().ToFullString().Equals(nameof(IFileSystem)));

			if (fileSystem != null)
			{
				return editor.GetChangedDocument();
			}

			var fileSystemPropertyDeclaration = SF.FieldDeclaration(SF.VariableDeclaration(SF.IdentifierName(nameof(IFileSystem)))
					.WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier("_fileSystem")))))
				.WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword),
					SF.Token(SyntaxKind.ReadOnlyKeyword)));

			editor.InsertMembers(classDeclarationSyntax,
				0,
				new SyntaxNode[]
					{ fileSystemPropertyDeclaration });

			editor.AddParameter(_constructor, parameter);

			return editor.GetChangedDocument();
		}
	}
}