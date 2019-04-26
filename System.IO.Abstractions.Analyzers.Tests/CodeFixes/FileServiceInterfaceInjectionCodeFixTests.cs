using System.Collections.Generic;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using System.IO.Abstractions.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis;
using Roslyn.Testing.CodeFix;
using Xunit;

namespace System.IO.Abstractions.Analyzers.Tests.CodeFixes
{
	public class FileServiceInterfaceInjectionCodeFixTests :
		CSharpCodeFixProviderTest<PathAnalyzer, FileServiceInterfaceInjectionCodeFix>
	{
		[Theory]
		[InlineData("BeforeFix.txt", "AfterFix.txt")]
		[InlineData("BeforeFixWithoutConstructor.txt", "AfterFix.txt")]
		[InlineData("BeforeFixExistsAbstractionsUsing.txt", "AfterFix.txt")]
		[InlineData("BeforeFixExistingConstructorParameter.txt", "AfterFix.txt")]
		[InlineData("BeforeFixExistingConstructorParameterUniqName.txt", "AfterFixConstructorParameterUniqName.txt")]
		[InlineData("BeforeFixContainsAssignment.txt", "AfterFixContainsAssignment.txt")]
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