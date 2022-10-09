using System.Composition;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider for <see cref="PathAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PathCodeFix))]
public class PathCodeFix : BaseInvokeCodeFix
{
	[ImportingConstructor]
	[Obsolete("This exported object must be obtained through the MEF export provider.", true)]
	public PathCodeFix()
	{
	}

	/// <inheritdoc />
	protected override string DiagnosticId => Constants.Io0006;

	/// <inheritdoc />
	protected override string Title => "Use IFileSystem.Path for improved testability";
}