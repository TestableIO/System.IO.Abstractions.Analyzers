using System.Collections.Immutable;
using System.Composition;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileCodeFix))]
	public class FileCodeFix : CodeFixProvider
	{
		private const string Title = "Use IFileSystem.File for improved testablity";

		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Constants.Io0002);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();

			// TODO: Add check contains private property with Type IFileSystem
			context.RegisterCodeFix(new FileSystemInvokeCodeAction(Title,
					context.Document,
					invocation),
				context.Diagnostics);
		}
	}
}