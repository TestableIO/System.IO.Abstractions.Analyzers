using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.RoslynToken;

public static class RoslynClassFileSystem
{
	public static bool HasConstructor(SyntaxNode classDeclaration) => classDeclaration.ChildNodes()
		.OfType<ConstructorDeclarationSyntax>()
		.Any();

	public static ConstructorDeclarationSyntax GetConstructor(SyntaxNode classDeclaration) => classDeclaration.ChildNodes()
		.OfType<ConstructorDeclarationSyntax>()
		.FirstOrDefault();

	public static MethodDeclarationSyntax GetMethod(ClassDeclarationSyntax classDeclaration) => classDeclaration.ChildNodes()
		.OfType<MethodDeclarationSyntax>()
		.FirstOrDefault();

	public static bool HasFileSystemField(TypeDeclarationSyntax classDeclaration) => classDeclaration.Members
		.OfType<FieldDeclarationSyntax>()
		.Any(x => x.Declaration.Type.NormalizeWhitespace()
					.ToFullString()
				== GetFileSystemType()
					.ToFullString());

	public static TypeSyntax GetFileSystemType() => SF.ParseTypeName(Constants.FileSystemInterfaceName);

	public static UsingDirectiveSyntax GetFileSystemUsing() => SF.UsingDirective(SF.ParseName(Constants.FileSystemNameSpace));

	public static UsingDirectiveSyntax GetSystemIoUsing(CompilationUnitSyntax unit) => GetUsing(unit, typeof(Path).Namespace);

	public static UsingDirectiveSyntax GetUsing(CompilationUnitSyntax unit, string usingName) => unit.Usings.FirstOrDefault(x =>
		x.Name.NormalizeWhitespace()
			.ToFullString()
			.Equals(usingName, StringComparison.Ordinal));

	public static FieldDeclarationSyntax CreateFileSystemFieldDeclaration() => SF.FieldDeclaration(SF
			.VariableDeclaration(GetFileSystemType())
			.WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier(Constants.FieldFileSystemName)))))
		.WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword),
			SF.Token(SyntaxKind.ReadOnlyKeyword)));

	public static bool ConstructorHasFileSystemParameter(BaseMethodDeclarationSyntax constructor) => constructor.ParameterList.Parameters
		.Any(x => x.Type != null
				&& x.Type.NormalizeWhitespace()
					.ToFullString()
				== GetFileSystemType()
					.NormalizeWhitespace()
					.ToFullString());

	public static ParameterSyntax GetFileSystemParameterFromConstructor(ConstructorDeclarationSyntax constructor) => constructor
		.ParameterList.Parameters
		.FirstOrDefault(x =>
			x.Type != null
			&& x.Type.NormalizeWhitespace()
				.ToFullString()
			== GetFileSystemType()
				.NormalizeWhitespace()
				.ToFullString());

	public static FieldDeclarationSyntax GetFileSystemFieldFromClass(ClassDeclarationSyntax @class) => @class.Members
		.OfType<FieldDeclarationSyntax>()
		.FirstOrDefault(x =>
			x.Declaration.Type.NormalizeWhitespace()
				.ToFullString()
			== GetFileSystemType()
				.ToFullString());

	public static ParameterSyntax CreateFileSystemParameterDeclaration() => SF.Parameter(SF.Identifier(Constants.ParameterFileSystemName))
		.WithType(GetFileSystemType())
		.NormalizeWhitespace();

	public static CompilationUnitSyntax GetCompilationUnit(SyntaxNode node) => node switch
	{
		null => null,
		CompilationUnitSyntax compilationUnitSyntax => compilationUnitSyntax,
		var _ => GetCompilationUnit(node.Parent)
	};

	public static bool ConstructorHasAssignmentExpression(BaseMethodDeclarationSyntax constructor,
														string field = Constants.FieldFileSystemName)
	{
		if (constructor.Body == null)
		{
			return false;
		}

		return constructor.Body.Statements.OfType<ExpressionStatementSyntax>()
			.Any(x => x.Expression.IsKind(SyntaxKind.SimpleAssignmentExpression)
					&& x.Expression.ChildNodes()
						.OfType<IdentifierNameSyntax>()
						.Any(a => a.Identifier.Text == field));
	}
}