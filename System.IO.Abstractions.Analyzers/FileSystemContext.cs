using Microsoft.CodeAnalysis;

namespace System.IO.Abstractions.Analyzers
{
	public class FileSystemContext
	{
		public FileSystemContext(Compilation compilation)
		{
			FileSystemType = compilation.GetTypeByMetadataName(Constants.FileSystem);
		}

		public bool HasReference => FileSystemType != null;

		public INamedTypeSymbol FileSystemType { get; }
	}
}