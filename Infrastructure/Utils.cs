namespace BinaryBeat.Infrastructure;

/// <summary>
/// Utility class
/// </summary>
public static class Utils
{
    /// <summary>
    /// Find model helper
    /// </summary>
    public static class PathResolver
    {
        /// <summary>
        /// TODO: Be able to change model for more complex data
        /// Get the path to the Model
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static string GetModelPath(string modelName = "ggml-tiny.bin")
        {
            // AppContext.BaseDirectory is more secure for Native AOT and VST-plugins
            string baseDir = AppContext.BaseDirectory;

            // Debug (Visual Studio), back up to projectroot to keep everything simple and close.
            #if DEBUG
                    string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\")); //Microsoft!!! Still after all these years.
                    string path = Path.Combine(projectRoot, "Models", modelName);
            #else
                    string path = Path.Combine(baseDir, "Models", modelName);
            #endif

            return path;
        }
    }


    /// <summary>
    /// Calculate RMS value
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static float CalculateRMS(byte[] buffer)
    {
        float sum = 0;
        for (int i = 0; i < buffer.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(buffer, i);
            float f = sample / 32768f;
            sum += f * f;
        }
        return MathF.Sqrt(sum / (buffer.Length / 2));
    }
}