using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;

/// <summary>
/// Search StreamReader in Code
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StreamReaderAnalyzer : BaseFileSystemNodeAnalyzer
{
	/// <summary>
	/// Diagnostic Identifier
	/// </summary>
	[UsedImplicitly]
	public const string DiagnosticId = "IO0011";

	/// <summary>
	/// Diagnostic Title
	/// </summary>
	private const string Title = "Replace StreamReader string constructor overload with stream based overload"
								+ " using a stream from IFileSystem.FileStream for improved testability";

	/// <summary>
	/// Diagnostic Message Format
	/// </summary>
	public const string MessageFormat = Title;

	/// <summary>
	/// Diagnostic Description
	/// </summary>
	private const string Description = Title;

	/// <summary>
	/// Diagnostic rule
	/// </summary>
	private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Warning,
		true,
		Description);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	/// <inheritdoc />
	protected override void Analyze(OperationAnalysisContext context, IOperation operation)
	{
		if (IsFirstConstructorParameterOfType<string>(context, operation))
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation()));
		}
	}

	/// <inheritdoc />
	protected override Type GetFileSystemType() => typeof(StreamReader);
}