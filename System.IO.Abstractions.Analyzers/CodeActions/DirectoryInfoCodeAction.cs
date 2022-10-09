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
public class DirectoryInfoCodeAction : CodeAction
{
	private readonly ObjectCreationExpressionSyntax _creationExpressionSyntax;

	private readonly Document _document;

	private readonly FieldDeclarationSyntax _field;

	public DirectoryInfoCodeAction(string title, Document document, ObjectCreationExpressionSyntax creationExpressionSyntax,
									FieldDeclarationSyntax field)
	{
		Title = title;
		_document = document;
		_creationExpressionSyntax = creationExpressionSyntax;
		_field = field;
	}

	/// <inheritdoc />
	public override string Title { get; }

	/// <inheritdoc />
	public override string EquivalenceKey => Title;

	/// <inheritdoc />
	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(_document, cancellationToken)
			.ConfigureAwait(false);

		var arguments = _creationExpressionSyntax.ArgumentList.Arguments.Select(x => x.ToFullString());

		editor.ReplaceNode(_creationExpressionSyntax,
			SF.ParseExpression(
				$"{_field.Declaration.Variables.ToFullString()}.DirectoryInfo.FromDirectoryName({string.Join(",", arguments)})"));

		return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}