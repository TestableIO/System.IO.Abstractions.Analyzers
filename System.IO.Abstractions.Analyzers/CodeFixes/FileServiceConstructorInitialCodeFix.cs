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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileServiceConstructorInitialCodeFix))]
	public class FileServiceConstructorInitialCodeFix : CodeFixProvider
	{
		private const string Title = "Create FileSystem in constructor and using System.IO.Abstractions";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(Constants.Io0001);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclarationSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

			context.RegisterCodeFix(new FileServiceConstructorInitialCodeAction(Title,
					context.Document,
					classDeclarationSyntax),
				context.Diagnostics);
		}
	}
}