using System.Collections.Immutable;
using System.Composition;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider for <see cref="FileInfoAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileInfoCodeFix))]
public class FileInfoCodeFix : CodeFixProvider
{
	private const string Title = "Use IFileInfoFactory instead creation FileInfo for improved testability";

	[ImportingConstructor]
	[Obsolete("This exported object must be obtained through the MEF export provider.", true)]
	public FileInfoCodeFix()
	{
	}

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Constants.Io0004);

	/// <inheritdoc />
	public override sealed FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
			.ConfigureAwait(false);

		var classDeclarationSyntax = root?.FindNode(context.Span)
			.FirstAncestorOrSelf<ClassDeclarationSyntax>();

		if (classDeclarationSyntax is null || !RoslynClassFileSystem.HasFileSystemField(classDeclarationSyntax))
		{
			return;
		}

		var creationExpressionSyntax = root.FindNode(context.Span)
			.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

		var fieldDeclarationSyntax = classDeclarationSyntax
			.Members
			.OfType<FieldDeclarationSyntax>()
			.FirstOrDefault(x =>
				x.Declaration.Type.NormalizeWhitespace()
					.ToFullString()
				== RoslynClassFileSystem.GetFileSystemType()
					.ToFullString());

		context.RegisterCodeFix(new FileInfoCodeAction(Title,
				context.Document,
				creationExpressionSyntax,
				fieldDeclarationSyntax),
			context.Diagnostics);
	}
}