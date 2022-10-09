using System.Composition;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes;

/// <summary>
/// Code fix provider for <see cref="DirectoryAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DirectoryCodeFix))]
public class DirectoryCodeFix : BaseInvokeCodeFix
{
	[ImportingConstructor]
	[Obsolete("This exported object must be obtained through the MEF export provider.", true)]
	public DirectoryCodeFix()
	{
	}

	/// <inheritdoc />
	protected override string DiagnosticId => Constants.Io0003;

	/// <inheritdoc />
	protected override string Title => "Use IFileSystem.Directory for improved testability";
}