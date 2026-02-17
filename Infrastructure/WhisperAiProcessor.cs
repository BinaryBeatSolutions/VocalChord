using BinaryBeat.Core;
using System.Runtime.CompilerServices;
using Whisper.net;
using Whisper.net.Ggml;
using System.Linq;

namespace BinaryBeat.Infrastructure;

/// <summary>
/// 
/// </summary>
public class WhisperAiProcessor : IAiProcessor, IDisposable
{
    private WhisperFactory? _factory;
    private WhisperProcessor? _processor;
    private readonly string _modelPath;

    // Default path om ingen anges
    private static readonly string DefaultModelPath = "ggml-tiny.bin";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customModelPath"></param>
    public WhisperAiProcessor(string? customModelPath = null)
    {

        _modelPath = customModelPath ?? DefaultModelPath;
        Console.WriteLine(_modelPath);
    }

    /// <summary>
    /// Initierar motorn och laddar ner modellen om den saknas.
    /// Bör anropas innan första körning.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!File.Exists(_modelPath))
        {
            var directory = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            Console.WriteLine($"[BinaryBeat] Downloading model to: {_modelPath}...");

            // Vi använder Whisper.net:s inbyggda downloader
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileStream = File.OpenWrite(_modelPath);
            await modelStream.CopyToAsync(fileStream);

            Console.WriteLine("[BinaryBeat] Download complete.");
        }

        _factory = WhisperFactory.FromPath(_modelPath);
        _processor = _factory.CreateBuilder()
            .WithLanguage("en") // Eller AutoDetectLanguage() för att låta modellen gissa språket. Det finns lite olika strategier för att styra språket.
            .WithThreads(Environment.ProcessorCount)
            .WithEntropyThreshold(0.1f)
            .WithBeamSearchSamplingStrategy()
            .ParentBuilder.Build();

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
        if (_processor == null) throw new InvalidOperationException("Processor not initialized.");

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
    /// Dispose components to free up resources. WhisperFactory och WhisperProcessor kan använda GPU/CPU-resurser, så det är viktigt att städa upp ordentligt.
    /// </summary>
    public void Dispose()
    {
        if (_processor != null) 
            _processor.Dispose();
        
        if(_factory!=null)
            _factory.Dispose();
    }
}