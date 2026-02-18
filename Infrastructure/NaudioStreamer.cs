using NAudio.Wave;

namespace BinaryBeat.Infrastructure;

public class NaudioStreamer : IAudioStreamer, IDisposable
{
    private WaveInEvent _waveIn;
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    public async IAsyncEnumerable<byte[]> StreamAudioAsync([EnumeratorCancellation] CancellationToken ct)
    {
        // WaveInEvent är för mikrofoner. 
        // DeviceNumber 0 är oftast din standardmikrofon i Windows.
        using var waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 100 // Hur ofta den ska skicka data (lägre = snabbare)
        };

        var channel = Channel.CreateUnbounded<byte[]>();

        waveIn.DataAvailable += (s, e) =>
        {
            var buffer = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, buffer, e.BytesRecorded);
            channel.Writer.TryWrite(buffer);
        };

        waveIn.StartRecording();

        Console.WriteLine("[DEBUG] NAudio recording started...");

        await foreach (var data in _channel.Reader.ReadAllAsync(ct).WithCancellation(ct))
        {
            Console.WriteLine("DATA");
            yield return data;
        }
    }


    public void Stop() => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}
