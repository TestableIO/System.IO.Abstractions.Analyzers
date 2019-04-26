using System.IO.Abstractions.Analyzers.RoslynToken;
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
	public class FileServiceConstructorInitialCodeAction : CodeAction
	{
		private readonly ClassDeclarationSyntax _class;

		private readonly Document _document;

		public FileServiceConstructorInitialCodeAction(string title, Document document, ClassDeclarationSyntax @class)
		{
			_class = @class;
			_document = document;
			Title = title;
		}

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);

			if (!RoslynClassFileSystem.HasFileSystemField(_class))
			{
				editor.InsertMembers(_class,
					0,
					new SyntaxNode[]
					{
						RoslynClassFileSystem.CreateFileSystemFieldDeclaration()
					});
			}

			ConstructorAddParameter(_class, editor);

			var compilationUnitSyntax = RoslynClassFileSystem.GetCompilationUnit(_class);

			if (!compilationUnitSyntax.Usings.Any())
			{
				return editor.GetChangedDocument();
			}

			var fileSystem = RoslynClassFileSystem.GetUsing(compilationUnitSyntax, Constants.FileSystemNameSpace);

			if (fileSystem != default(UsingDirectiveSyntax))
			{
				return editor.GetChangedDocument();
			}

			var systemIo = RoslynClassFileSystem.GetSystemIoUsing(compilationUnitSyntax);

			if (systemIo == default(UsingDirectiveSyntax))
			{
				editor.InsertBefore(compilationUnitSyntax.Usings.FirstOrDefault(),
					RoslynClassFileSystem.GetFileSystemUsing());
			} else
			{
				editor.InsertAfter(systemIo, RoslynClassFileSystem.GetFileSystemUsing());
			}

			return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		private static ExpressionStatementSyntax CreateAssignmentExpression(string field = Constants.FieldFileSystemName)
		{
			return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
				SF.IdentifierName(field),
				SF.ObjectCreationExpression(SF.IdentifierName(Constants.FileSystemClassName))
					.WithArgumentList(SF.ArgumentList())));
		}

		private static void ConstructorAddParameter(ClassDeclarationSyntax classDeclaration, SyntaxEditor editor)
		{
			var constructor = RoslynClassFileSystem.HasConstructor(classDeclaration)
				? RoslynClassFileSystem.GetConstructor(classDeclaration)
				: SF.ConstructorDeclaration(classDeclaration.Identifier)
					.WithModifiers(SyntaxTokenList.Create(SF.Token(SyntaxKind.PublicKeyword)));

			var newConstructor = constructor.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			if (!RoslynClassFileSystem.ConstructorHasAssignmentExpression(newConstructor))
			{
				var statementSyntax = CreateAssignmentExpression();

				if (RoslynClassFileSystem.HasFileSystemField(classDeclaration))
				{
					var fileSystem = RoslynClassFileSystem.GetFileSystemFieldFromClass(classDeclaration);
					statementSyntax = CreateAssignmentExpression(fileSystem.Declaration.Variables.ToFullString());
				}

				newConstructor =
					newConstructor.AddBodyStatements(statementSyntax);
			}

			if (RoslynClassFileSystem.HasConstructor(classDeclaration))
			{
				editor.ReplaceNode(constructor, newConstructor);
			} else
			{
				editor.InsertBefore(RoslynClassFileSystem.GetMethod(classDeclaration), newConstructor);
			}
		}
	}
}