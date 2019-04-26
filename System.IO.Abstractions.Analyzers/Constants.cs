using System.IO.Abstractions.Analyzers.Analyzers.FileSystemTypeAnalyzers;

namespace System.IO.Abstractions.Analyzers
{
	public static class Constants
	{
		internal const string FileSystem = "System.IO.Abstractions.IFileSystem";

		internal const string FileSystemInterfaceName = "I" + FileSystemClassName;

		internal const string FileSystemClassName = "FileSystem";

		internal const string FileSystemNameSpace = "System.IO.Abstractions";

		internal const string ParameterFileSystemName = "fileSystem";

		internal const string FieldFileSystemName = "_fileSystem";

		public const string Io0001 = "IO0001";

		/// <summary>
		/// <see cref="FileAnalyzer" />
		/// </summary>
		public const string Io0002 = "IO0002";

		/// <summary>
		/// <see cref="DirectoryAnalyzer" />
		/// </summary>
		public const string Io0003 = "IO0003";

		/// <summary>
		/// <see cref="FileInfoAnalyzer" />
		/// </summary>
		public const string Io0004 = "IO0004";

		/// <summary>
		/// <see cref="FileStreamAnalyzer" />
		/// </summary>
		public const string Io0005 = "IO0005";

		/// <summary>
		/// <see cref="PathAnalyzer" />
		/// </summary>
		public const string Io0006 = "IO0006";

		/// <summary>
		/// <see cref="DirectoryInfoAnalyzer" />
		/// </summary>
		public const string Io0007 = "IO0007";
	}
}