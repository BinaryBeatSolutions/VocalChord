using Whisper.net;
using Whisper.net.Ggml;

namespace BinaryBeat.Infrastructure;

/// <summary>
/// Default speech recognition model
/// </summary>
public class WhisperAiProcessor : IAProcessor, IDisposable
{
    private WhisperFactory _factory;
    private WhisperProcessor _processor;
    private readonly string _modelPath;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="customModelPath"></param>
    public WhisperAiProcessor(string? customModelPath = null)
    {
        _modelPath = customModelPath ?? Utils.PathResolver.GetModelPath();
    }

    /// <summary>
    /// Initialize the engine and downloading the model if no model exist.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!File.Exists(_modelPath))
        {
            var directory = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

#if DEBUG
            Console.WriteLine($"[BinaryBeat] Downloading model to: {_modelPath}..."); //Needs to be shown in VST that we are downloading their first model.
            Console.WriteLine($"[BinaryBeat] This is only done the first time if the model doesn't exists.");
#endif
            // Using Whisper.net:s builtin downloader
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileStream = File.OpenWrite(_modelPath);
            await modelStream.CopyToAsync(fileStream);
#if DEBUG
            Console.WriteLine("[BinaryBeat] Download complete.");
#endif
        }

        _factory = WhisperFactory.FromPath(_modelPath);
        _processor = _factory.CreateBuilder()
        .WithLanguage("en") // Eller AutoDetectLanguage() för att låta modellen gissa språket. Det finns lite olika strategier för att styra språket.
            .WithThreads(Environment.ProcessorCount)
            .WithEntropyThreshold(0.1f)
            .WithBeamSearchSamplingStrategy()
            .ParentBuilder.Build();
#if DEBUG
        Console.WriteLine($"[MODEL] {Path.GetFileName(_modelPath)}");
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async IAsyncEnumerable<SpeechResult> ProcessAudioAsync(byte[] pcmData, [EnumeratorCancellation] CancellationToken ct)
    {
        if (_processor == null) throw new InvalidOperationException("[DEBUG] Processor not initialized.");

        // Konvertera PCM (short) till float samples
        var samples = new float[pcmData.Length / 2];
        for (int n = 0; n < samples.Length; n++)
        {
            short sample = BitConverter.ToInt16(pcmData, n * 2);
            samples[n] = sample / 32768.0f;
        }

        await foreach (var segment in _processor.ProcessAsync(samples, ct))
        {
            yield return new SpeechResult(segment.Text.Trim(), segment.Probability);
        }
    }

    /// <summary>
    /// Dispose components to free up resources. WhisperFactory och WhisperProcessor kan använda mycket GPU/CPU-resurser, så det är viktigt att städa upp ordentligt.
    /// </summary>
    public void Dispose()
    {
        if (_processor != null) 
            _processor.Dispose();
        
        if(_factory!=null)
            _factory.Dispose();

        GC.SuppressFinalize(this);
    }
}