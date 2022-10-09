using System.Composition;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider for <see cref="FileAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileCodeFix))]
public class FileCodeFix : BaseInvokeCodeFix
{
	[ImportingConstructor]
	[Obsolete("This exported object must be obtained through the MEF export provider.", true)]
	public FileCodeFix()
	{
	}

	/// <inheritdoc />
	protected override string DiagnosticId => Constants.Io0002;

	/// <inheritdoc />
	protected override string Title => "Use IFileSystem.File for improved testability";
}