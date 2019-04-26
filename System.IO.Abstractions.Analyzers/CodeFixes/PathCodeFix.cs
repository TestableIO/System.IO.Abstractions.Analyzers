using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PathCodeFix))]
	public class PathCodeFix : BaseInvokeCodeFix
	{
		protected override string DiagnosticId => Constants.Io0006;

		protected override string Title => "Use IFileSystem.Path for improved testability";
	}
}