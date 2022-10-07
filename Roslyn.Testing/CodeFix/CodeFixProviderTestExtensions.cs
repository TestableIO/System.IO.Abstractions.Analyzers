using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Roslyn.Testing.Analyzer;

namespace Roslyn.Testing.CodeFix;

internal static class CodeFixProviderTestExtensions
{
	/// <summary>
	/// Apply the inputted CodeAction to the inputted document.
	/// Meant to be used to apply codefixes.
	/// </summary>
	/// <param name="document"> The Document to apply the fix on </param>
	/// <param name="codeAction"> A CodeAction that will be applied to the Document. </param>
	/// <returns> A Document with the changes from the CodeAction </returns>
	public static Document ApplyFix(this Document document, CodeAction codeAction)
	{
		var operations = codeAction.GetOperationsAsync(CancellationToken.None)
			.GetAwaiter()
			.GetResult();

		var solution = operations.OfType<ApplyChangesOperation>()
			.Single()
			.ChangedSolution;

		return solution.GetDocument(document.Id);
	}

	#region [Actual comparisons and verifications]

	/// <summary>
	/// General verifier for codefixes.
	/// Creates a Document from the source string, then gets diagnostics on it and
	/// applies the relevant codefixes.
	/// Then gets the string after the codefix is applied and compares it with the
	/// expected result.
	/// Note: If any codefix causes new diagnostics to show up, the test fails unless
	/// allowNewCompilerDiagnostics is set to true.
	/// </summary>
	/// <param name="language"> The language the source code is in </param>
	/// <param name="analyzer"> The analyzer to be applied to the source code </param>
	/// <param name="codeFixProvider">
	/// The codefix to be applied to the code wherever
	/// the relevant Diagnostic is found
	/// </param>
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
	/// <param name="additionalReferences"></param>
	public static VerifyCodeFixProviderResult VerifyFix(this CodeFixProvider codeFixProvider,
														string language,
														DiagnosticAnalyzer analyzer,
														string oldSource,
														string newSource,
														int? codeFixIndex,
														bool allowNewCompilerDiagnostics,
														IEnumerable<MetadataReference> additionalReferences = null)
	{
		var document = DiagnosticAnalyzerTestExtensions.CreateDocument(oldSource, language, additionalReferences);

		var analyzerDiagnostics = analyzer.GetSortedDiagnosticsFromDocuments(new[]
		{
			document
		});

		var compilerDiagnostics = document.GetCompilerDiagnostics();
		var attempts = analyzerDiagnostics.Length;

		for (var i = 0; i < attempts; ++i)
		{
			var actions = new List<CodeAction>();
			var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);

			codeFixProvider.RegisterCodeFixesAsync(context)
				.GetAwaiter()
				.GetResult();

			if (!actions.Any())
			{
				break;
			}

			if (codeFixIndex != null)
			{
				document = document.ApplyFix(actions.ElementAt((int) codeFixIndex));

				break;
			}

			document = document.ApplyFix(actions.ElementAt(0));

			analyzerDiagnostics = analyzer.GetSortedDiagnosticsFromDocuments(new[]
			{
				document
			});

			var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetCompilerDiagnostics());

			//check if applying the code fix introduced any new compiler diagnostics
			if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
			{
				// Format and get the compiler diagnostics again so that the locations make sense in the output
				document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync()
						.GetAwaiter()
						.GetResult(),
					Formatter.Annotation,
					document.Project.Solution.Workspace));

				newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetCompilerDiagnostics());
				var msg = GetNewCompilerDiagnosticsIntroducedMessage(document, newCompilerDiagnostics);

				return VerifyCodeFixProviderResult.Fail(msg);
			}

			//check if there are analyzer diagnostics left after the code fix
			if (!analyzerDiagnostics.Any())
			{
				break;
			}
		}

		//after applying all of the code fixes, compare the resulting string to the inputted one
		var actual = document.GetStringFromDocument();

		return newSource.Equals(actual)
			? VerifyCodeFixProviderResult.Ok()
			: VerifyCodeFixProviderResult.Fail(newSource, actual);
	}

	private static string GetNewCompilerDiagnosticsIntroducedMessage(Document document, IEnumerable<Diagnostic> newCompilerDiagnostics) =>
		$"Fix introduced new compiler diagnostics:{Environment.NewLine}{string.Join("{Environment.NewLine}", newCompilerDiagnostics.Select(d => d.ToString()))}{Environment.NewLine}{Environment.NewLine}New document:{Environment.NewLine}{document.GetSyntaxRootAsync().GetAwaiter().GetResult().ToFullString()}{Environment.NewLine}";

	/// <summary>
	/// Get the existing compiler diagnostics on the inputted document.
	/// </summary>
	/// <param name="document">
	/// The Document to run the compiler diagnostic analyzers
	/// on
	/// </param>
	/// <returns> The compiler diagnostics that were found in the code </returns>
	private static IEnumerable<Diagnostic> GetCompilerDiagnostics(this Document document) => document.GetSemanticModelAsync()
		.GetAwaiter()
		.GetResult()
		.GetDiagnostics();

	/// <summary>
	/// Given a document, turn it into a string based on the syntax root
	/// </summary>
	/// <param name="document"> The Document to be converted to a string </param>
	/// <returns> A string containing the syntax of the Document after formatting </returns>
	public static string GetStringFromDocument(this Document document)
	{
		var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation)
			.GetAwaiter()
			.GetResult();

		var root = simplifiedDoc.GetSyntaxRootAsync()
			.GetAwaiter()
			.GetResult();

		root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);

		return root.GetText()
			.ToString();
	}

	/// <summary>
	/// Compare two collections of Diagnostics,and return a list of any new diagnostics
	/// that appear only in the second collection.
	/// Note: Considers Diagnostics to be the same if they have the same Ids.  In the
	/// case of multiple diagnostics with the same Id in a row,
	/// this method may not necessarily return the new one.
	/// </summary>
	/// <param name="diagnostics">
	/// The Diagnostics that existed in the code before the
	/// CodeFix was applied
	/// </param>
	/// <param name="newDiagnostics">
	/// The Diagnostics that exist in the code after the
	/// CodeFix was applied
	/// </param>
	/// <returns>
	/// A list of Diagnostics that only surfaced in the code after the CodeFix was
	/// applied
	/// </returns>
	private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics,
															IEnumerable<Diagnostic> newDiagnostics)
	{
		var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start)
			.ToArray();

		var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start)
			.ToArray();

		var oldIndex = 0;
		var newIndex = 0;

		while (newIndex < newArray.Length)
		{
			if (oldIndex < oldArray.Length
				&& oldArray[oldIndex]
					.Id
				== newArray[newIndex]
					.Id)
			{
				++oldIndex;
				++newIndex;
			} else
			{
				yield return newArray[newIndex++];
			}
		}
	}

	#endregion
}