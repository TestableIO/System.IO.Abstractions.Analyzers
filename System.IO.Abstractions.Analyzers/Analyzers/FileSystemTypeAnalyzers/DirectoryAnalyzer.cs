using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DirectoryAnalyzer : BaseFileSystemNodeAnalyzer
{
	/// <summary>
	/// Diagnostic Identifier
	/// </summary>
	[UsedImplicitly]
	public const string DiagnosticId = Constants.Io0003;

	/// <summary>
	/// Diagnostic Title
	/// </summary>
	private const string Title = "Replace Directory class with IFileSystem.Directory for improved testability";

	/// <summary>
	/// Diagnostic Message Format
	/// </summary>
	public const string MessageFormat = Title;

	/// <summary>
	/// Diagnostic Description
	/// </summary>
	private const string Description = Title;

	private const string Link = "http://bit.ly/2Oz9iSx";

	/// <summary>
	/// Правило
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
	protected override void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax invocation) =>
		context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));

	/// <inheritdoc />
	protected override Type GetFileSystemType() => typeof(Directory);
}