using Microsoft.CodeAnalysis;

namespace System.IO.Abstractions.Analyzers;

/// <summary>
/// Context for a IFileSystem method invocation.
/// </summary>
public class FileSystemContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FileSystemContext"/> class.
	/// </summary>
	/// <param name="compilation"></param>
	public FileSystemContext(Compilation compilation) => FileSystemType = compilation.GetTypeByMetadataName(Constants.FileSystem);

	/// <summary>
	/// Project has reference to System.IO.Abstractions.
	/// </summary>
	public bool HasReference => FileSystemType is not null;

	private INamedTypeSymbol FileSystemType { get; }
}