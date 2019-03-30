using System.Collections.Immutable;
using System.Composition;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DirectoryInfoCodeFix))]
	public class DirectoryInfoCodeFix : CodeFixProvider
	{
		private const string Title = "Use IDirectoryInfoFactory instead creation DirectoryInfo for improved testability";

		public override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(Constants.Io0007);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclarationSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

			if (RoslynClassFileSystem.HasFileSystemField(classDeclarationSyntax))
			{
				var creationExpressionSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

				var fieldDeclarationSyntax = classDeclarationSyntax
					.Members
					.OfType<FieldDeclarationSyntax>()
					.FirstOrDefault(x =>
						x.Declaration.Type.NormalizeWhitespace().ToFullString()
						== RoslynClassFileSystem.GetFileSystemType().ToFullString());

				context.RegisterCodeFix(new DirectoryInfoCodeAction(Title,
						context.Document,
						creationExpressionSyntax,
						fieldDeclarationSyntax),
					context.Diagnostics);
			}
		}
	}
}