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
				editor.InsertMembers(_class,
					0,
					new SyntaxNode[]
					{
						CreateFileSystemPropertyDeclaration()
					});
			}

			ConstructorAddParameter(_class, editor);

			var compilationUnitSyntax = GetCompilationUnit(_class);
			var fileSystemUsing = GetFileSystemUsing();

			if (compilationUnitSyntax.Usings.Any())
			{
				editor.ReplaceNode(GetSystemIoUsing(compilationUnitSyntax),
					fileSystemUsing);
			}

			return editor.GetChangedDocument();
		}

		private static UsingDirectiveSyntax GetFileSystemUsing()
		{
			return SF.UsingDirective(SF.ParseName(Constants.FileSystemNameSpace));
		}

		private static UsingDirectiveSyntax GetSystemIoUsing(CompilationUnitSyntax unit)
		{
			return unit.Usings.FirstOrDefault(x =>
				x.Name.NormalizeWhitespace().ToFullString().Equals(typeof(Path).Namespace));
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
			if (constructor.Body == null)
			{
				return false;
			}

			return constructor.Body.Statements.OfType<ExpressionStatementSyntax>()
				.Any(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression)
						&& x.Expression.Contains(SF.IdentifierName(FieldFileSystemName))
						&& x.Expression.Contains(SF.IdentifierName(ParameterFileSystemName)));
		}

		private static bool HasConstructor(SyntaxNode classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>().Any();
		}

		private static void ConstructorAddParameter(ClassDeclarationSyntax classDeclaration, SyntaxEditor editor)
		{
			var constructor = HasConstructor(classDeclaration)
				? GetConstructor(classDeclaration)
				: SF.ConstructorDeclaration(classDeclaration.Identifier)
					.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

			var newConstructor = constructor.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				.NormalizeWhitespace();

			if (!ConstructorHasAssignmentExpression(newConstructor))
			{
				newConstructor = newConstructor.AddBodyStatements(CreateAssignmentExpression());
			}

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
				editor.InsertBefore(GetMethod(classDeclaration), newConstructor);
			}
		}

		private static MethodDeclarationSyntax GetMethod(ClassDeclarationSyntax classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
		}
	}
}