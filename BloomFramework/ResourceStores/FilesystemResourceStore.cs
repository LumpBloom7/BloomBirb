namespace BloomFramework.ResourceStores;

public class FilesystemResourceStore : IResourceStore
{

    private static readonly char[] path_separators = new[]{
        '.',
        '\\',
        '/'
    };

    private readonly string root;

    // Always from working directory
    public FilesystemResourceStore(string directory)
    {
        root = directory;

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(root);

        Console.WriteLine("Files available:");
        Console.WriteLine("-------------------");

        foreach (string file in Directory.EnumerateFiles(directory, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
            Console.WriteLine(file);
    }

    public Stream? Get(string filename)
    {
        string? correctPath = findActualPath($"{root}{Path.DirectorySeparatorChar}", new ReadOnlySpan<string>(filename.Split(path_separators)));

        if (correctPath is null)
            return null;

        return new FileStream(correctPath, FileMode.Open);
    }

    private string? findActualPath(string current, ReadOnlySpan<string> splitted)
    {
        if (splitted.Length == 1)
        {
            string final = current + splitted[0];
            if (!File.Exists(final))
                return null;

            return final;
        }

        return
        findActualPath($"{current}{splitted[0]}{Path.DirectorySeparatorChar}", splitted.Slice(1))
        ?? findActualPath($"{current}{splitted[0]}.", splitted.Slice(1));
    }
}
