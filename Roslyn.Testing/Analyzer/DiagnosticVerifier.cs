using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Testing.Model;

namespace Roslyn.Testing.Analyzer
{
	internal static class DiagnosticAnalyzerTestExtensions
	{
		private static readonly MetadataReference CorlibReference =
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

		private static readonly MetadataReference SystemCoreReference =
			MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

		private static readonly MetadataReference CSharpSymbolsReference =
			MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);

		private static readonly MetadataReference CodeAnalysisReference =
			MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

		private static readonly MetadataReference SystemDiagReference =
			MetadataReference.CreateFromFile(typeof(Process).Assembly.Location);

		internal static string DefaultFilePathPrefix = "Test";

		internal static string CSharpDefaultFileExt = "cs";

		internal static string VisualBasicDefaultExt = "vb";

		internal static string TestProjectName = "TestProject";

	#region  [Get Diagnostics]

		/// <summary>
		/// Given classes in the form of strings, their language, and an
		/// IDiagnosticAnlayzer to apply to it, return the diagnostics found in the string
		/// after converting it to a document.
		/// </summary>
		/// <param name="sources"> Classes in the form of strings </param>
		/// <param name="language"> The language the source classes are in </param>
		/// <param name="analyzer"> The analyzer to be run on the sources </param>
		/// <returns>
		/// An IEnumerable of Diagnostics that surfaced in the source code, sorted by
		/// Location
		/// </returns>
		public static Diagnostic[] GetSortedDiagnostics(this DiagnosticAnalyzer analyzer,
														string[] sources,
														string language, IEnumerable<MetadataReference> references = null)
		{
			return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources, language, references));
		}

		/// <summary>
		/// Given an analyzer and a document to apply it to, run the analyzer and gather an
		/// array of diagnostics found in it.
		/// The returned diagnostics are then ordered by location in the source document.
		/// </summary>
		/// <param name="analyzer"> The analyzer to run on the documents </param>
		/// <param name="documents"> The Documents that the analyzer will be run on </param>
		/// <returns>
		/// An IEnumerable of Diagnostics that surfaced in the source code, sorted by
		/// Location
		/// </returns>
		public static Diagnostic[] GetSortedDiagnosticsFromDocuments(this DiagnosticAnalyzer analyzer,
																	Document[] documents)
		{
			var projects = new HashSet<Project>();

			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var diagnostics = new List<Diagnostic>();

			foreach (var project in projects)
			{
				var compilationWithAnalyzers =
					project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));

				var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;

				foreach (var diag in diags)
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						diagnostics.Add(diag);
					} else
					{
						foreach (var document in documents)
						{
							var tree = document.GetSyntaxTreeAsync().Result;

							if (tree == diag.Location.SourceTree)
							{
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			var results = SortDiagnostics(diagnostics);
			diagnostics.Clear();

			return results;
		}

		/// <summary>
		/// Sort diagnostics by location in source document
		/// </summary>
		/// <param name="diagnostics"> The list of Diagnostics to be sorted </param>
		/// <returns> An IEnumerable containing the Diagnostics in order of Location </returns>
		private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
		{
			return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
		}

	#endregion

	#region [Set up compilation and documents]

		/// <summary>
		/// Given an array of strings as sources and a language, turn them into a project
		/// and return the documents and spans of it.
		/// </summary>
		/// <param name="sources"> Classes in the form of strings </param>
		/// <param name="language"> The language the source code is in </param>
		/// <returns>
		/// A Tuple containing the Documents produced from the sources and their TextSpans
		/// if relevant
		/// </returns>
		private static Document[] GetDocuments(string[] sources, string language, IEnumerable<MetadataReference> references = null)
		{
			if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
			{
				throw new ArgumentException("Unsupported Language");
			}

			var project = CreateProject(sources, language, references);
			var documents = project.Documents.ToArray();

			if (sources.Length != documents.Length)
			{
				throw new SystemException("Amount of sources did not match amount of Documents created");
			}

			return documents;
		}

		/// <summary>
		/// Create a Document from a string through creating a project that contains it.
		/// </summary>
		/// <param name="source"> Classes in the form of a string </param>
		/// <param name="language"> The language the source code is in </param>
		/// <returns> A Document created from the source string </returns>
		public static Document CreateDocument(string source, string language, IEnumerable<MetadataReference> references = null)
		{
			return CreateProject(new[] { source }, language, references).Documents.First();
		}

		/// <summary>
		/// Create a project using the inputted strings as sources.
		/// </summary>
		/// <param name="sources"> Classes in the form of strings </param>
		/// <param name="language"> The language the source code is in </param>
		/// <param name="references">Дополнительные сборки</param>
		/// <returns>
		/// A Project created out of the Documents created from the source
		/// strings
		/// </returns>
		private static Project CreateProject(string[] sources, string language, IEnumerable<MetadataReference> references = null)
		{
			var fileNamePrefix = DefaultFilePathPrefix;
			var fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

			var projectId = ProjectId.CreateNewId(TestProjectName);

			var solution = new AdhocWorkspace()
				.CurrentSolution
				.AddProject(projectId, TestProjectName, TestProjectName, language)
				.AddMetadataReference(projectId, CorlibReference)
				.AddMetadataReference(projectId, SystemCoreReference)
				.AddMetadataReference(projectId, CSharpSymbolsReference)
				.AddMetadataReference(projectId, CodeAnalysisReference)
				.AddMetadataReference(projectId, SystemDiagReference);

			if (references != null)
			{
				solution = solution.AddMetadataReferences(projectId, references);
			}

			var count = 0;

			foreach (var source in sources)
			{
				var newFileName = fileNamePrefix + count + "." + fileExt;
				var documentId = DocumentId.CreateNewId(projectId, newFileName);
				solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
				count++;
			}

			return solution.GetProject(projectId);
		}

	#endregion

	#region [Actual comparisons and verifications]

		/// <summary>
		/// Checks each of the actual Diagnostics found and compares them with the
		/// corresponding DiagnosticResult in the array of expected results.
		/// Diagnostics are considered equal only if the DiagnosticResultLocation, Id,
		/// Severity, and Message of the DiagnosticResult match the actual diagnostic.
		/// </summary>
		/// <param name="actualResults">
		/// The Diagnostics found by the compiler after running the analyzer on the source
		/// code
		/// </param>
		/// <param name="analyzer"> The analyzer that was being run on the sources </param>
		/// <param name="expectedResults">
		/// Diagnostic Results that should have appeared in
		/// the code
		/// </param>
		public static VerifyDiagnosticAnalyzerResult VerifyDiagnosticResults(this DiagnosticAnalyzer analyzer,
																			IEnumerable<Diagnostic> actualResults,
																			DiagnosticResult[] expectedResults)
		{
			var expectedCount = expectedResults.Length;
			var actualCount = actualResults.Count();

			if (expectedCount != actualCount)
			{
				var diagnosticsOutput = actualResults.Any()
					? FormatDiagnostics(analyzer, actualResults.ToArray())
					: "    NONE.";

				var msg = GetMismatchNumberOfDiagnosticsMessage(expectedCount, actualCount, diagnosticsOutput);

				return VerifyDiagnosticAnalyzerResult.Fail(msg);
			}

			for (var i = 0; i < expectedResults.Length; i++)
			{
				var actual = actualResults.ElementAt(i);
				var expected = expectedResults[i];

				if (expected.Line == -1 && expected.Column == -1)
				{
					if (actual.Location != Location.None)
					{
						var msg = GetExpectedDiagnosticWithNoLocation(analyzer, actual);

						return VerifyDiagnosticAnalyzerResult.Fail(msg);
					}
				} else
				{
					var locationResult =
						VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());

					if (!locationResult.Success)
					{
						return locationResult;
					}

					var additionalLocations = actual.AdditionalLocations.ToArray();

					if (additionalLocations.Length != expected.Locations.Length - 1)
					{
						var msg = GetNotExpectedLocation(analyzer, actual, expected, additionalLocations);

						return VerifyDiagnosticAnalyzerResult.Fail(msg);
					}

					for (var j = 0; j < additionalLocations.Length; ++j)
					{
						locationResult = VerifyDiagnosticLocation(analyzer,
							actual,
							additionalLocations[j],
							expected.Locations[j + 1]);

						if (!locationResult.Success)
						{
							return locationResult;
						}
					}
				}

				if (actual.Id != expected.Id)
				{
					var msg = GetNoExpectedDiagnosticId(analyzer, actual, expected);

					return VerifyDiagnosticAnalyzerResult.Fail(msg);
				}

				if (actual.Severity != expected.Severity)
				{
					var msg = GetNotExpectedSeverityMessage(analyzer, actual, expected);

					return VerifyDiagnosticAnalyzerResult.Fail(msg);
				}

				if (actual.GetMessage() != expected.Message)
				{
					var msg = GetNotExcpectedMessage(analyzer, actual, expected);

					return VerifyDiagnosticAnalyzerResult.Fail(msg);
				}
			}

			return VerifyDiagnosticAnalyzerResult.Ok();
		}

		/// <summary>
		/// Helper method to VerifyDiagnosticResult that checks the location of a
		/// diagnostic and compares it with the location in the expected DiagnosticResult.
		/// </summary>
		/// <param name="analyzer"> The analyzer that was being run on the sources </param>
		/// <param name="diagnostic"> The diagnostic that was found in the code </param>
		/// <param name="actual"> The Location of the Diagnostic found in the code </param>
		/// <param name="expected">
		/// The DiagnosticResultLocation that should have been
		/// found
		/// </param>
		private static VerifyDiagnosticAnalyzerResult VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer,
																				Diagnostic diagnostic,
																				Location actual,
																				DiagnosticResultLocation expected)
		{
			var actualSpan = actual.GetLineSpan();

			var isInExpectedFile = actualSpan.Path == expected.Path
									|| actualSpan.Path != null
									&& actualSpan.Path.Contains("Test0.")
									&& expected.Path.Contains("Test.");

			if (!isInExpectedFile)
			{
				var msg = GetNotInExpectedFileMessage(analyzer, diagnostic, expected, actualSpan);

				return VerifyDiagnosticAnalyzerResult.Fail(msg);
			}

			var actualLinePosition = actualSpan.StartLinePosition;

			// Only check line position if there is an actual line in the real diagnostic
			if (actualLinePosition.Line > 0)
			{
				if (actualLinePosition.Line + 1 != expected.Line)
				{
					var msg = GetNotInExpectedLineMessage(analyzer, diagnostic, expected, actualLinePosition);

					return VerifyDiagnosticAnalyzerResult.Fail(msg);
				}
			}

			// Only check column position if there is an actual column position in the real diagnostic
			if (actualLinePosition.Character > 0)
			{
				if (actualLinePosition.Character + 1 != expected.Column)
				{
					var msg = GetNotInExpectedColumn(analyzer, diagnostic, expected, actualLinePosition);

					return VerifyDiagnosticAnalyzerResult.Fail(msg);
				}
			}

			return VerifyDiagnosticAnalyzerResult.Ok();
		}

		private static string GetMismatchNumberOfDiagnosticsMessage(int expectedCount, int actualCount,
																	string diagnosticsOutput)
		{
			return
				$"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" actual \"{actualCount}\"{Environment.NewLine}{Environment.NewLine}Diagnostics:{Environment.NewLine}{diagnosticsOutput}{Environment.NewLine}";
		}

		private static string GetExpectedDiagnosticWithNoLocation(DiagnosticAnalyzer analyzer, Diagnostic actual)
		{
			return
				$"Expected:\nA project diagnostic with No location\nActual:\n{FormatDiagnostics(analyzer, new[] { actual })}";
		}

		private static string GetNotExpectedLocation(DiagnosticAnalyzer analyzer,
													Diagnostic actual,
													DiagnosticResult expected,
													Location[] additionalLocations)
		{
			return
				$"Expected {expected.Locations.Length - 1} additional locations but got {additionalLocations.Length} for Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { actual })}{Environment.NewLine}";
		}

		private static string GetNoExpectedDiagnosticId(DiagnosticAnalyzer analyzer,
														Diagnostic actual,
														DiagnosticResult expected)
		{
			return
				$"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { actual })}{Environment.NewLine}";
		}

		private static string GetNotExpectedSeverityMessage(DiagnosticAnalyzer analyzer,
															Diagnostic actual,
															DiagnosticResult expected)
		{
			return
				$"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { actual })}{Environment.NewLine}";
		}

		private static string GetNotExcpectedMessage(DiagnosticAnalyzer analyzer,
													Diagnostic actual,
													DiagnosticResult expected)
		{
			return
				$"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage()}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { actual })}{Environment.NewLine}";
		}

		private static string GetNotInExpectedColumn(DiagnosticAnalyzer analyzer,
													Diagnostic diagnostic,
													DiagnosticResultLocation expected,
													LinePosition actualLinePosition)
		{
			return
				$"Expected diagnostic to start at column \"{expected.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { diagnostic })}{Environment.NewLine}";
		}

		private static string GetNotInExpectedLineMessage(DiagnosticAnalyzer analyzer,
														Diagnostic diagnostic,
														DiagnosticResultLocation expected,
														LinePosition actualLinePosition)
		{
			return
				$"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { diagnostic })}{Environment.NewLine}";
		}

		private static string GetNotInExpectedFileMessage(DiagnosticAnalyzer analyzer,
														Diagnostic diagnostic,
														DiagnosticResultLocation expected,
														FileLinePositionSpan actualSpan)
		{
			return
				$"Expected diagnostic to be in file \"{expected.Path}\" was actually in file \"{actualSpan.Path}\"{Environment.NewLine}{Environment.NewLine}Diagnostic:{Environment.NewLine}    {FormatDiagnostics(analyzer, new[] { diagnostic })}{Environment.NewLine}";
		}

		/// <summary>
		/// Helper method to format a Diagnostic into an easily readable string
		/// </summary>
		/// <param name="analyzer"> The analyzer that this verifier tests </param>
		/// <param name="diagnostics"> The Diagnostics to be formatted </param>
		/// <returns> The Diagnostics formatted as a string </returns>
		private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, Diagnostic[] diagnostics)
		{
			var builder = new StringBuilder();

			for (var i = 0; i < diagnostics.Length; ++i)
			{
				builder.AppendLine("// " + diagnostics[i]);

				var analyzerType = analyzer.GetType();
				var rules = analyzer.SupportedDiagnostics;

				foreach (var rule in rules)
				{
					if (rule != null && rule.Id == diagnostics[i].Id)
					{
						var location = diagnostics[i].Location;

						if (location == Location.None)
						{
							builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
						} else
						{
							if (!location.IsInSource)
							{
								var msg =
									$"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}{Environment.NewLine}";

								throw new Exception(msg);
							}

							var resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs")
								? "GetCSharpResultAt"
								: "GetBasicResultAt";

							var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

							builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
								resultMethodName,
								linePosition.Line + 1,
								linePosition.Character + 1,
								analyzerType.Name,
								rule.Id);
						}

						if (i != diagnostics.Length - 1)
						{
							builder.Append(',');
						}

						builder.AppendLine();

						break;
					}
				}
			}

			return builder.ToString();
		}

	#endregion
	}
}