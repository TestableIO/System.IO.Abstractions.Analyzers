using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace System.IO.Abstractions.Analyzers.CodeActions;

/// <summary>
/// Code action to replace with a IFileSystem call.
/// </summary>
public class FileSystemInvokeCodeAction(
	string title,
	Document document,
	InvocationExpressionSyntax invocation,
	FieldDeclarationSyntax field)
	: CodeAction
{
	/// <inheritdoc />
	public override string Title { get; } = title;

	/// <inheritdoc />
	public override string EquivalenceKey => Title;

	/// <inheritdoc />
	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
			.ConfigureAwait(false);

		if (field.Declaration.Variables.Any())
		{
			editor.ReplaceNode(invocation,
				SF.ParseExpression(
					$"{field.Declaration.Variables.FirstOrDefault()?.Identifier.Text}.{invocation.NormalizeWhitespace().ToFullString()}"));
		}

		return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}