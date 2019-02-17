using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Testing.Model;
using Shouldly;

namespace Roslyn.Testing.Analyzer
{
	public abstract class CSharpDiagnosticAnalyzerTest<T> : FileReaderTest
		where T : DiagnosticAnalyzer, new()
	{
		private readonly DiagnosticAnalyzer _diagnosticAnalyzer;

		protected CSharpDiagnosticAnalyzerTest()
		{
			_diagnosticAnalyzer = new T();
		}

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted
		/// string as a source
		/// Note: input a DiagnosticResult for each Diagnostic expected
		/// </summary>
		/// <param name="source"> A class in the form of a string to run the analyzer on </param>
		/// <param name="expected">
		/// DiagnosticResults that should appear after the analyzer
		/// is run on the source
		/// </param>
		protected void VerifyDiagnostic(string source, DiagnosticResult[] expected)
		{
			VerifyDiagnostic(new[] { source }, expected);
		}

		protected void VerifyDiagnostic(string source, DiagnosticResult expected)
		{
			VerifyDiagnostic(new[] { source }, new[] { expected });
		}

		protected void VerifyNoDiagnosticTriggered(string source)
		{
			VerifyDiagnostic(new[] { source }, new DiagnosticResult[0]);
		}

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as
		/// a source
		/// Note: input a DiagnosticResult for each Diagnostic expected
		/// </summary>
		/// <param name="sources">
		/// An array of strings to create source documents from to
		/// run the analyzers on
		/// </param>
		/// <param name="expected">
		/// DiagnosticResults that should appear after the analyzer
		/// is run on the sources
		/// </param>
		protected void VerifyDiagnostic(string[] sources, DiagnosticResult[] expected)
		{
			var actual = _diagnosticAnalyzer.GetSortedDiagnostics(sources, LanguageNames.CSharp);
			var result = _diagnosticAnalyzer.VerifyDiagnosticResults(actual, expected);
			result.Success.ShouldBe(true, result.ErrorMessage);
		}
	}
}