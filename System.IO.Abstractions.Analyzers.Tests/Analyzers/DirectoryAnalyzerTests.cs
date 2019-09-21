using System.Collections.Generic;
using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;
using Microsoft.CodeAnalysis;
using Roslyn.Testing.Analyzer;
using Roslyn.Testing.Model;
using Xunit;

namespace System.IO.Abstractions.Analyzers.Tests.Analyzers
{
	public class DirectoryAnalyzerTests: CSharpDiagnosticAnalyzerTest<DirectoryAnalyzer>
	{
		[Theory]
		[InlineData("Valid.txt")]
		[InlineData("FalsePositive.txt")]
		[InlineData("FalsePositive2.txt")]
		public void Analyzer_is_not_triggered(string filename)
		{
			var source = ReadFile(filename);
			VerifyNoDiagnosticTriggered(source);
		}

		[Theory]
		[InlineData("WithOutFileSystem.txt", 15, 4)]
		public void Analyzer_is_triggered(string filename, int diagnosticLine, int diagnosticColumn)
		{
			var source = ReadFile(filename);

			var expectedDiagnostic = new DiagnosticResult
			{
				Id = DirectoryAnalyzer.DiagnosticId,
				Message = DirectoryAnalyzer.MessageFormat,
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", diagnosticLine, diagnosticColumn) }
			};

			VerifyDiagnostic(source, expectedDiagnostic);
		}

		[Fact]
		public void Empty_source_code_does_not_trigger_analyzer()
		{
			var source = string.Empty;
			VerifyNoDiagnosticTriggered(source);
		}

		protected override IEnumerable<MetadataReference> GetAdditionalReferences() => new[]
		{
			MetadataReference.CreateFromFile(typeof(IFileSystem).Assembly.Location)
		};
	}
}