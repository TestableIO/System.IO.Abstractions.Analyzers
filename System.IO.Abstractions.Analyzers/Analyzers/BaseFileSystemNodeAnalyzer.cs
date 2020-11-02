using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers
{
	public abstract class BaseFileSystemNodeAnalyzer : BaseFileSystemAnalyzer
	{
		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
													FileSystemContext fileSystemContext)
		{
			if (IsNotUsedSystemIo())
			{
				return;
			}

			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var invocation = (InvocationExpressionSyntax) syntaxContext.Node;

					if (IsStaticInvocationStartWith(invocation) && !IsInvocationFromAbstractions(syntaxContext, invocation))
					{
						Analyze(syntaxContext, invocation);
					}
				},
				SyntaxKind.InvocationExpression);

			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var creationExpressionSyntax = (ObjectCreationExpressionSyntax) syntaxContext.Node;
					var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(creationExpressionSyntax);

					if (IsTypesEquals(typeInfo))
					{
						Analyze(syntaxContext, creationExpressionSyntax);
					}
				},
				SyntaxKind.ObjectCreationExpression);
		}

		protected abstract void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax invocation);

		protected abstract Type GetFileSystemType();

		private bool IsNotUsedSystemIo()
		{
			return typeof(Path).Namespace != GetFileSystemType().Namespace;
		}

		private bool IsTypesEquals(TypeInfo typeInfo)
		{
			var fileSystemType = GetFileSystemType();
			var namespaceSymbol = typeInfo.Type.ContainingNamespace;

			return typeInfo.Type.Name == fileSystemType.Name
					&& (namespaceSymbol.IsGlobalNamespace ||
										namespaceSymbol.ToString() == fileSystemType.Namespace);
		}

		private bool IsStaticInvocationStartWith(InvocationExpressionSyntax invocation)
		{
			return invocation.IsKind(SyntaxKind.InvocationExpression)
					&& invocation.Expression.NormalizeWhitespace()
						.ToFullString()
						.StartsWith(GetFileSystemType().Name + ".", StringComparison.Ordinal);
		}

		private static bool IsInvocationFromAbstractions(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
		{
			return (invocation?.Expression as MemberAccessExpressionSyntax)?.Expression is ExpressionSyntax invokedMember
					&& context.SemanticModel.GetSymbolInfo(invokedMember).Symbol is ISymbol symbol
					&& ((symbol as IPropertySymbol)?.Type ?? (symbol as IFieldSymbol)?.Type) is ITypeSymbol type
					&& !type.ContainingNamespace.IsGlobalNamespace
					&& type.ContainingNamespace.ToString().StartsWith(Constants.FileSystemNameSpace, StringComparison.Ordinal);
		}

		protected static bool IsFirstConstructorParameterOfType<T>(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
		{
			return (syntax as ObjectCreationExpressionSyntax)?.ArgumentList?.Arguments.FirstOrDefault() is ArgumentSyntax firstArgument
				&& (context.SemanticModel.GetSymbolInfo(firstArgument.Expression).Symbol as ILocalSymbol)?.Type is ITypeSymbol argumentType
				&& argumentType.ContainingNamespace.Name == typeof(T).Namespace
				&& argumentType.Name == typeof(T).Name;
		}
	}
}