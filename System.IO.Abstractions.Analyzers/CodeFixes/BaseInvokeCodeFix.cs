using System.Collections.Immutable;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	public abstract class BaseInvokeCodeFix : CodeFixProvider
	{
		protected abstract string DiagnosticId { get; }

		protected abstract string Title { get; }

		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root.FindNode(context.Span).Ancestors().OfType<ClassDeclarationSyntax>().Any(RoslynClassFileSystem.HasFileSystemField))
			{
				var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();

				var @class = root.FindNode(context.Span)
					.FirstAncestorOrSelf<ClassDeclarationSyntax>();

				var field = RoslynClassFileSystem.GetFileSystemFieldFromClass(@class);

				context.RegisterCodeFix(new FileSystemInvokeCodeAction(Title,
						context.Document,
						invocation,
						field),
					context.Diagnostics);
			}
		}
	}
}