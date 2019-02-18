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
			if (typeof(Path).Namespace != GetFileSystemType().Namespace)
			{
				return;
			}

			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var invocation = (InvocationExpressionSyntax) syntaxContext.Node;

					if (invocation.NormalizeWhitespace().ToFullString().StartsWith(GetFileSystemType().Name))
					{
						Analyze(syntaxContext, invocation);
					}
				},
				SyntaxKind.InvocationExpression);

			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				{
					var invocation = (ObjectCreationExpressionSyntax) syntaxContext.Node;

					if (invocation.Type.NormalizeWhitespace().ToFullString().StartsWith(GetFileSystemType().Name))
					{
						Analyze(syntaxContext, invocation);
					}
				},
				SyntaxKind.ObjectCreationExpression);
		}

		protected abstract void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax invocation);

		protected abstract Type GetFileSystemType();
	}
}