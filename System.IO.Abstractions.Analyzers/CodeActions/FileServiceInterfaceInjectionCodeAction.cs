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

			var parameter = CreateFileSystemParameterDeclaration();

			if (!(_constructor.Parent is ClassDeclarationSyntax classDeclarationSyntax))
			{
				editor.AddParameter(_constructor, parameter);

				return editor.GetChangedDocument();
			}

			var fileSystem = classDeclarationSyntax.Members
				.OfType<FieldDeclarationSyntax>()
				.FirstOrDefault(x => x.NormalizeWhitespace().ToFullString().Equals(Constants.FileSystemName));

			if (fileSystem != null)
			{
				return editor.GetChangedDocument();
			}

			var fileSystemPropertyDeclaration = CreateFileSystemPropertyDeclaration();

			editor.InsertMembers(classDeclarationSyntax,
				0,
				new SyntaxNode[]
				{
					fileSystemPropertyDeclaration
				});

			var newConstructor = _constructor.WithBody(_constructor.Body.AddStatements(CreateAssignmentExpression()))
				.AddParameterListParameters(parameter)
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			editor.ReplaceNode(_constructor, newConstructor);
			var compilationUnitSyntax = GetCompilationUnit(_constructor);

			editor.ReplaceNode(compilationUnitSyntax.Usings.FirstOrDefault(),
				SF.UsingDirective(SF.ParseName(Constants.FileSystemNameSpace)));

			return editor.GetChangedDocument();
		}

		private static FieldDeclarationSyntax CreateFileSystemPropertyDeclaration()
		{
			return SF.FieldDeclaration(SF.VariableDeclaration(CreateFileSystemType())
					.WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier("_fileSystem")))))
				.WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword),
					SF.Token(SyntaxKind.ReadOnlyKeyword)));
		}

		private static ParameterSyntax CreateFileSystemParameterDeclaration()
		{
			return SF.Parameter(SF.Identifier("fileSystem"))
				.WithType(CreateFileSystemType())
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.SpecialTypeAnnotation)
				.NormalizeWhitespace();
		}

		private static TypeSyntax CreateFileSystemType()
		{
			return SF.ParseTypeName(Constants.FileSystemName);
		}

		private static ExpressionStatementSyntax CreateAssignmentExpression()
		{
			return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
				SF.IdentifierName("_fileSystem"),
				SF.IdentifierName("fileSystem")));
		}

		private CompilationUnitSyntax GetCompilationUnit(SyntaxNode node)
		{
			switch (node)
			{
				case null:

					return null;
				case CompilationUnitSyntax compilationUnitSyntax:

					return compilationUnitSyntax;
				default:

					return GetCompilationUnit(node.Parent);
			}
		}
	}
}