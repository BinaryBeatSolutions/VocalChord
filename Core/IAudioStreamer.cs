
using NAudio.Wave;

namespace BinaryBeat.Core;

public interface IAudioStreamer
{
    // Vi skickar ut ljudet i chunks för att hålla minnesanvändningen nere
   public IAsyncEnumerable<byte[]> StreamAudioAsync(CancellationToken ct);

    public void Stop<StoppedEventArgs>(object o, StoppedEventArgs e);
}
