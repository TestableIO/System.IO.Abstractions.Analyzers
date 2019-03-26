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
		private const string FieldFileSystemName = "_fileSystem";

		private const string ParameterFileSystemName = "fileSystem";

		private readonly ClassDeclarationSyntax _class;

		private readonly Document _document;

		public FileServiceInterfaceInjectionCodeAction(string title, Document document, ClassDeclarationSyntax @class)
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

			if (!HasFileSystemProperty(_class))
			{
				var fileSystemPropertyDeclaration = CreateFileSystemPropertyDeclaration();

				editor.InsertMembers(_class,
					0,
					new SyntaxNode[]
					{
						fileSystemPropertyDeclaration
					});
			}

			ConstructorAddParameter(_class, editor);

			var compilationUnitSyntax = GetCompilationUnit(_class);

			editor.ReplaceNode(compilationUnitSyntax.Usings.FirstOrDefault(),
				SF.UsingDirective(SF.ParseName(Constants.FileSystemNameSpace)));

			return editor.GetChangedDocument();
		}

		private static FieldDeclarationSyntax CreateFileSystemPropertyDeclaration()
		{
			return SF.FieldDeclaration(SF.VariableDeclaration(GetFileSystemType())
					.WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier(FieldFileSystemName)))))
				.WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword),
					SF.Token(SyntaxKind.ReadOnlyKeyword)));
		}

		private static ParameterSyntax CreateFileSystemParameterDeclaration()
		{
			return SF.Parameter(SF.Identifier(ParameterFileSystemName))
				.WithType(GetFileSystemType())
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.SpecialTypeAnnotation)
				.NormalizeWhitespace();
		}

		private static TypeSyntax GetFileSystemType()
		{
			return SF.ParseTypeName(Constants.FileSystemName);
		}

		private static ExpressionStatementSyntax CreateAssignmentExpression()
		{
			return SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
				SF.IdentifierName(FieldFileSystemName),
				SF.IdentifierName(ParameterFileSystemName)));
		}

		private static CompilationUnitSyntax GetCompilationUnit(SyntaxNode node)
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

		private static bool HasFileSystemProperty(TypeDeclarationSyntax classDeclaration)
		{
			return classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
				.Any(x => x.Identifier.Text == FieldFileSystemName && x.Type == GetFileSystemType());
		}

		private static ConstructorDeclarationSyntax GetConstructor(SyntaxNode classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
		}

		private static bool ConstructorHasFileSystemParameter(BaseMethodDeclarationSyntax constructor)
		{
			return constructor.ParameterList.Parameters
				.Any(x => x.Identifier.Text == ParameterFileSystemName && x.Type == GetFileSystemType());
		}

		private static bool ConstructorHasAssignmentExpression(BaseMethodDeclarationSyntax constructor)
		{
			return constructor.Body.Statements.OfType<ExpressionStatementSyntax>()
				.Any(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression)
						&& x.Expression.Contains(SF.IdentifierName(FieldFileSystemName))
						&& x.Expression.Contains(SF.IdentifierName(ParameterFileSystemName)));
		}

		private static bool HasConstructor(SyntaxNode classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>().Any();
		}

		private static void ConstructorAddParameter(ClassDeclarationSyntax classDeclaration, DocumentEditor editor)
		{
			var constructor = HasConstructor(classDeclaration)
				? GetConstructor(classDeclaration)
				: SF.ConstructorDeclaration(classDeclaration.Identifier);

			var newConstructor = constructor.WithBody(constructor.Body?.AddStatements(CreateAssignmentExpression()))
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			if (!ConstructorHasFileSystemParameter(newConstructor))
			{
				var parameter = CreateFileSystemParameterDeclaration();
				newConstructor = newConstructor.AddParameterListParameters(parameter);
			}

			if (HasConstructor(classDeclaration))
			{
				editor.ReplaceNode(constructor, newConstructor);
			} else
			{
				editor.InsertMembers(classDeclaration,
					0,
					new SyntaxNode[]
					{
						newConstructor
					});
			}
		}
	}
}