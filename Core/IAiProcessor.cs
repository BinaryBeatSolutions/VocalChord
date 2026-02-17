namespace BinaryBeat.Core;

public record SpeechResult(string Text, float Confidence);

public interface IAiProcessor
{
    IAsyncEnumerable<SpeechResult> ProcessAudioAsync(byte[] pcmData, CancellationToken ct);
}

