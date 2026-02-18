namespace BinaryBeat.Core;

public record SpeechResult(string Text, float Confidence);

public interface IAProcessor
{
    IAsyncEnumerable<SpeechResult> ProcessAudioAsync(byte[] pcmData, CancellationToken ct);

    public Task InitializeAsync();
}

