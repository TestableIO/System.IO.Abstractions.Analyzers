using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileCodeFix))]
	public class FileCodeFix : BaseInvokeCodeFix
	{
		protected override string DiagnosticId => Constants.Io0002;

		protected override string Title => "Use IFileSystem.File for improved testability";
	}
}