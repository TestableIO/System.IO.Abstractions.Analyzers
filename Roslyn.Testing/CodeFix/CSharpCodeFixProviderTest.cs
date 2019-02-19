using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Testing.Model;
using Shouldly;

namespace Roslyn.Testing.CodeFix
{
	public abstract class CSharpCodeFixProviderTest<TDiagnosticAnalyzer, TCodeFixProvider> : FileReaderTest
		where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFixProvider : CodeFixProvider, new()
	{
	#region To be implemented by Test classes

		/// <inheritdoc />
		public override string Filepath => _codeFixProvider.GetType().Name;

		/// <inheritdoc />
		public override string PathToTestData => "./TestData/CodeFix/";

		protected virtual IEnumerable<MetadataReference> GetAdditionalReferences()
		{
			return Enumerable.Empty<MetadataReference>();
		}

	#endregion

		private readonly TCodeFixProvider _codeFixProvider;

		private readonly TDiagnosticAnalyzer _diagnosticAnalyzer;

		protected CSharpCodeFixProviderTest()
		{
			_diagnosticAnalyzer = new TDiagnosticAnalyzer();
			_codeFixProvider = new TCodeFixProvider();
		}

		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">
		/// A class in the form of a string before the CodeFix was
		/// applied to it
		/// </param>
		/// <param name="newSource">
		/// A class in the form of a string after the CodeFix was
		/// applied to it
		/// </param>
		/// <param name="codeFixIndex">
		/// Index determining which codefix to apply if there
		/// are multiple
		/// </param>
		/// <param name="allowNewCompilerDiagnostics">
		/// A bool controlling whether or not the test will fail if the CodeFix introduces
		/// other warnings after being applied
		/// </param>
		protected void VerifyFix(string oldSource,
								string newSource,
								int? codeFixIndex = null,
								bool allowNewCompilerDiagnostics = false)
		{
			var result = _codeFixProvider.VerifyFix(LanguageNames.CSharp,
				_diagnosticAnalyzer,
				oldSource,
				newSource,
				codeFixIndex,
				allowNewCompilerDiagnostics,
				GetAdditionalReferences());

			if (result.Success)
			{
				return;
			}

			if (string.IsNullOrEmpty(result.ErrorMessage))
			{
				result.NewSource.ShouldBe(result.ActualSource);
			} else
			{
				result.Success.ShouldBeTrue(result.ErrorMessage);
			}
		}
	}
}