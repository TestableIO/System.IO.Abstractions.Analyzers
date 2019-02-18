using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileSystemWatcherAnalyzer: BaseFileSystemNodeObjectCreationAnalyzer
	{
		/// <summary>
		/// Diagnostic Identifier
		/// </summary>
		[UsedImplicitly]
		public const string DiagnosticId = "IO0009";

		/// <summary>
		/// Diagnostic Title
		/// </summary>
		private const string Title = "Invocation DriveInfo class shold be replaced with IFileSystem.DriveInfo";

		/// <summary>
		/// Diagnostic Message Format
		/// </summary>
		public const string MessageFormat = Title;

		/// <summary>
		/// Diagnostic Description
		/// </summary>
		private const string Description = Title;

		/// <summary>
		/// Правило
		/// </summary>
		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			true,
			Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		protected override void Analyze(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax syntax)
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation()));
		}

		protected override Type GetFileSystemType()
		{
			return typeof(FileSystemWatcher);
		}
	}
}