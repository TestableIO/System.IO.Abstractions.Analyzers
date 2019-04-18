using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.RoslynToken
{
	public static class RoslynClassFileSystem
	{
		public static bool HasConstructor(SyntaxNode classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>().Any();
		}

		public static ConstructorDeclarationSyntax GetConstructor(SyntaxNode classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
		}

		public static MethodDeclarationSyntax GetMethod(ClassDeclarationSyntax classDeclaration)
		{
			return classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
		}

		public static bool HasFileSystemField(TypeDeclarationSyntax classDeclaration)
		{
			return classDeclaration.Members.OfType<FieldDeclarationSyntax>()
				.Any(x => x.Declaration.Type.NormalizeWhitespace().ToFullString() == GetFileSystemType().ToFullString());
		}

		public static TypeSyntax GetFileSystemType()
		{
			return SF.ParseTypeName(Constants.FileSystemInterfaceName);
		}

		public static UsingDirectiveSyntax GetFileSystemUsing()
		{
			return SF.UsingDirective(SF.ParseName(Constants.FileSystemNameSpace));
		}

		public static UsingDirectiveSyntax GetSystemIoUsing(CompilationUnitSyntax unit)
		{
			return GetUsing(unit, typeof(Path).Namespace);
		}

		public static UsingDirectiveSyntax GetUsing(CompilationUnitSyntax unit, string usingName)
		{
			return unit.Usings.FirstOrDefault(x =>
				x.Name.NormalizeWhitespace().ToFullString().Equals(usingName, StringComparison.InvariantCultureIgnoreCase));
		}

		public static FieldDeclarationSyntax CreateFileSystemFieldDeclaration()
		{
			return SF.FieldDeclaration(SF.VariableDeclaration(GetFileSystemType())
					.WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier(Constants.FieldFileSystemName)))))
				.WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword),
					SF.Token(SyntaxKind.ReadOnlyKeyword)));
		}

		public static bool ConstructorHasFileSystemParameter(BaseMethodDeclarationSyntax constructor)
		{
			return constructor.ParameterList.Parameters
				.Any(x => x.Identifier.Text == Constants.ParameterFileSystemName && x.Type == GetFileSystemType());
		}

		public static ParameterSyntax CreateFileSystemParameterDeclaration()
		{
			return SF.Parameter(SF.Identifier(Constants.ParameterFileSystemName))
				.WithType(GetFileSystemType())
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.SpecialTypeAnnotation)
				.NormalizeWhitespace();
		}

		public static CompilationUnitSyntax GetCompilationUnit(SyntaxNode node)
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

		public static bool ConstructorHasAssignmentExpression(BaseMethodDeclarationSyntax constructor)
		{
			if (constructor.Body == null)
			{
				return false;
			}

			return constructor.Body.Statements.OfType<ExpressionStatementSyntax>()
				.Any(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression)
						&& x.Expression.Contains(SF.IdentifierName(Constants.FieldFileSystemName))
						&& x.Expression.Contains(SF.IdentifierName(Constants.ParameterFileSystemName)));
		}
	}
}