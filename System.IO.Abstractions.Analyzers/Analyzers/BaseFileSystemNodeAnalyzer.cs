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

					if (IsStaticInvocationStartWith(invocation))
					{
						Analyze(syntaxContext, invocation);
					}
				},
				SyntaxKind.InvocationExpression);

			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var creationExpressionSyntax = (ObjectCreationExpressionSyntax) syntaxContext.Node;

					if (IsTypesEquals(creationExpressionSyntax.Type))
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

		private bool IsTypesEquals(TypeSyntax type)
		{
			return type.NormalizeWhitespace().ToFullString() == GetFileSystemType().Name;
		}

		private bool IsStaticInvocationStartWith(InvocationExpressionSyntax invocation)
		{
			return invocation.IsKind(SyntaxKind.InvocationExpression)
					&& invocation.Expression.NormalizeWhitespace()
						.ToFullString()
						.StartsWith(GetFileSystemType().Name + ".", StringComparison.CurrentCultureIgnoreCase);
		}
	}
}