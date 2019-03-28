using System.Collections.Generic;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using System.IO.Abstractions.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis;
using Roslyn.Testing.CodeFix;
using Xunit;

namespace System.IO.Abstractions.Analyzers.Tests.CodeFixes
{
	public class PathCodeFixTests :
		CSharpCodeFixProviderTest<PathAnalyzer, PathCodeFix>
	{
		[Theory]
		[InlineData("BeforeFix.txt", "AfterFix.txt")]
		public void CodeFix(string sourceBefore, string sourceAfter)
		{
			var sourceBeforeFix = ReadFile(sourceBefore);
			var sourceAfterFix = ReadFile(sourceAfter);
			VerifyFix(sourceBeforeFix, sourceAfterFix, 0, true);
		}

		protected override IEnumerable<MetadataReference> GetAdditionalReferences() => new[]
		{
			MetadataReference.CreateFromFile(typeof(IFileSystem).Assembly.Location)
		};
	}
}