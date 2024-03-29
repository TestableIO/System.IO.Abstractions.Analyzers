using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;

/// <summary>
/// Search method invocation from FileStream class in code
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FileStreamAnalyzer : BaseFileSystemNodeAnalyzer
{
	/// <summary>
	/// Diagnostic Identifier
	/// </summary>
	[UsedImplicitly]
	public const string DiagnosticId = Constants.Io0005;

	/// <summary>
	/// Diagnostic Title
	/// </summary>
	private const string Title = "Replace FileStream class with IFileSystem.FileStream for improved testability";

	/// <summary>
	/// Diagnostic Message Format
	/// </summary>
	public const string MessageFormat = Title;

	/// <summary>
	/// Diagnostic Description
	/// </summary>
	private const string Description = Title;

	private const string Link = "http://bit.ly/2Oz5MaM";

	/// <summary>
	/// Diagnostic rule
	/// </summary>
	private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Warning,
		true,
		Description,
		Link);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	/// <inheritdoc />
	protected override void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax) =>
		context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation()));

	/// <inheritdoc />
	protected override Type GetFileSystemType() => typeof(FileStream);
}