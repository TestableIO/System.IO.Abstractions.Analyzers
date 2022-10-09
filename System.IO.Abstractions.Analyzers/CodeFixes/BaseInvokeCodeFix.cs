using System.Collections.Immutable;
using System.IO.Abstractions.Analyzers.CodeActions;
using System.IO.Abstractions.Analyzers.RoslynToken;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.IO.Abstractions.Analyzers.CodeFixes;

/// <summary>
/// Base class for code fixes that replace a <see cref="MemberAccessExpressionSyntax"/> with a <see cref="InvocationExpressionSyntax"/>.
/// </summary>
public abstract class BaseInvokeCodeFix : CodeFixProvider
{
	/// <summary>
	/// Diagnostic ID of the analyzer that this code fix provider can fix.
	/// </summary>
	protected abstract string DiagnosticId { get; }

	/// <summary>
	/// Title of the code fix.
	/// </summary>
	protected abstract string Title { get; }

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

	/// <inheritdoc />
	public override sealed FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public override async sealed Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
			.ConfigureAwait(false);

		if (root == null)
		{
			return;
		}

		var classDeclaration = root.FindNode(context.Span)
			.FirstAncestorOrSelf<ClassDeclarationSyntax>();

		if (RoslynClassFileSystem.HasFileSystemField(classDeclaration))
		{
			var invocation = root.FindNode(context.Span)
				.DescendantNodesAndSelf()
				.OfType<InvocationExpressionSyntax>()
				.FirstOrDefault();

			var field = RoslynClassFileSystem.GetFileSystemFieldFromClass(classDeclaration);

			context.RegisterCodeFix(new FileSystemInvokeCodeAction(Title,
					context.Document,
					invocation,
					field),
				context.Diagnostics);
		}
	}
}