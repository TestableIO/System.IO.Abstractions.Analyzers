using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.IO.Abstractions.Analyzers.CodeFixes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DirectoryCodeFix))]
	public class DirectoryCodeFix : BaseInvokeCodeFix
	{
		protected override string DiagnosticId => Constants.Io0003;

		protected override string Title => "Use IFileSystem.Directory for improved testablity";
	}
}