namespace TiCodeX.SQLSchemaCompare.CLI;

/// <summary>
/// The helper class
/// </summary>
public static class Helper
{
    /// <summary>
    /// If the path starts with the user's profile (home) directory, the replaces the part with a tilde (~).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The shortened path.</returns>
    public static string ShortenHomePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var homePath = Path.TrimEndingDirectorySeparator(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        if (!path.StartsWith(homePath, StringComparison.InvariantCultureIgnoreCase))
        {
            return path;
        }

        return $"~{path[homePath.Length..]}";
    }
}
