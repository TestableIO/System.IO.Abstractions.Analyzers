using System.Linq;
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
/// Code action to replace a <see cref="DirectoryInfo"/> with a IFileSystem.DirectoryInfo.
/// </summary>
public class DirectoryInfoCodeAction(
	string title,
	Document document,
	ObjectCreationExpressionSyntax creationExpressionSyntax,
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

		if (creationExpressionSyntax.ArgumentList is null)
		{
			return document;
		}

		var arguments = creationExpressionSyntax.ArgumentList.Arguments.Select(x => x.ToFullString());

		editor.ReplaceNode(creationExpressionSyntax,
			SF.ParseExpression($"{field.Declaration.Variables.ToFullString()}.DirectoryInfo.New({string.Join(",", arguments)})"));

		return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}