using System.Collections.Immutable;
using System.Composition;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileServiceInterfaceInjectionCodeFix))]
	public class FileServiceInterfaceInjectionCodeFix : CodeFixProvider
	{
		private const string Title = "Inject IFileSystem and using System.IO.Abstractions";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(Constants.Io0002,
				Constants.Io0003,
				Constants.Io0004,
				Constants.Io0005,
				Constants.Io0006,
				Constants.Io0007);

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclarationSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			var constructor = RoslynClassFileSystem.GetConstructor(classDeclarationSyntax);

			if (!RoslynClassFileSystem.HasFileSystemField(classDeclarationSyntax)
				|| constructor != null && !RoslynClassFileSystem.ConstructorHasFileSystemParameter(constructor))
			{
				context.RegisterCodeFix(new FileServiceInterfaceInjectionCodeAction(Title,
						context.Document,
						classDeclarationSyntax),
					context.Diagnostics);
			}
		}
	}
}