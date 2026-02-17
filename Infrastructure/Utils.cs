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
    /// Noise GATE. Voice Activity Detection to remove background 
    /// noise when below -40db to free up heavy use on the CPU
    /// </summary>
    /// <param name="buffer">Byte chunk array</param>
    /// <returns>The RMS value, for eg 44000</returns>
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