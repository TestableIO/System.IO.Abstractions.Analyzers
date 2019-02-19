using System.Collections.Immutable;
using System.Composition;
using System.IO.Abstractions.Analyzers.Analyzers;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class FileServiceInterfaceInjectionCodeFix : CodeFixProvider
	{
		private const string Title = "Use System.IO.Abstractions";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(FileServiceInterfaceInjectionAnalyzer.DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var constructorDeclarationSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

			context.RegisterCodeFix(new FileServiceInterfaceInjectionCodeAction(Title,
					context.Document,
					constructorDeclarationSyntax),
				context.Diagnostics);
		}
	}
}