using System.IO;
using JetBrains.Annotations;

namespace Roslyn.Testing.Model;

public abstract class FileReaderTest
{
	public abstract string Filepath { get; }

	[UsedImplicitly]
	public abstract string PathToTestData { get; }

	public string ReadFile(string filename)
	{
		var pathToFile = Path.Combine(PathToTestData, Filepath, filename);

		return File.ReadAllText(pathToFile);
	}
}