using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

namespace BinaryBeat.Infrastructure;


internal static class Configuration
{
    internal static string APP_NAME = "--- BinaryBeat Solutions: VocalChord LIVE ---";

    internal static string APP_START_DEV_MESSAGE = "[DEBUG] NAudio recording started...";

    internal static string APP_VERSION = "1.0";

    internal static readonly string APP_DEFAULT_MODEL = "ggml-tiny.bin";

    internal static readonly string APP_MODEL_EN = "ggml-tiny.en.bin";

    /// <summary>
    /// Microphone Threshold. 
    /// 0.01 is around -40dB. Talk into the mic and see what value is the trigger.
    /// Adjustable so you dont have to shout into the mic.
    /// </summary>
    internal static double NOISE_GATE_TRESHOLD = 0.001;
}

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
        /// <returns></returns>
        public static string GetModelPath()
        {
            // AppContext.BaseDirectory is more secure for Native AOT and VST-plugins
            string baseDir = AppContext.BaseDirectory;

            // Debug (Visual Studio), back up to projectroot to keep everything simple and close.
#if DEBUG
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\")); //Microsoft!!! Still after all these years.
            string path = Path.Combine(projectRoot, "Models", Configuration.APP_MODEL_EN);
#else
                    string path = Path.Combine(baseDir, "Models", modelName);
#endif

            return path;
        }
    }

    public static class AudioUtils
    {
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


        /// <summary>
        /// Noise Gate, check for treshold. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesRecorded"></param>
        /// <returns></returns>
        private static bool NoiseGate(byte[] buffer, int bytesRecorded)
        {
            if (bytesRecorded == 0) return true;

            double sum = 0;
            // Vi hoppar 2 bytes åt gången eftersom vi kör 16-bitars PCM
            for (int i = 0; i < bytesRecorded; i += 2)
            {
                // Konvertera bytes till en 16-bitars short (sample)
                short sample = BitConverter.ToInt16(buffer, i);

                // Normalisera till -1.0 till 1.0 och kvadrera för RMS-beräkning
                double sample32 = sample / 32768.0;
                sum += sample32 * sample32;
            }

            double rms = Math.Sqrt(sum / (bytesRecorded / 2));

            return rms < Configuration.NOISE_GATE_TRESHOLD;
        }

        public static bool IsSilence(byte[] buffer, int bytesRecorded, double noiseThreshold = 0.0)
        {
            if (noiseThreshold > 0.0)
                Configuration.NOISE_GATE_TRESHOLD = noiseThreshold;

            return NoiseGate(buffer, bytesRecorded);
        }
    }
}