namespace BinaryBeat.Infrastructure;

public static class PathResolver
{
    public static string GetModelPath(string modelName = "ggml-base.bin")
    {
        // AppContext.BaseDirectory är säkrast för Native AOT och VST-plugins
        string baseDir = AppContext.BaseDirectory;

        // Om vi kör i Debug (Visual Studio), backar vi upp till projektroten
#if DEBUG
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\"));
        string path = Path.Combine(projectRoot, "Models", modelName);
#else
        string path = Path.Combine(baseDir, "Models", modelName);
#endif

        return path;
    }

    public static void EnsureDirectoryExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}