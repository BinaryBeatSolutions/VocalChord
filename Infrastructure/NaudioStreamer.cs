using NAudio.Wave;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace BinaryBeat.Infrastructure;

public class NaudioStreamer : IAudioStreamer, IDisposable
{
    private WaveInEvent? _waveIn;
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    public async IAsyncEnumerable<byte[]> StreamAudioAsync([EnumeratorCancellation] CancellationToken ct)
    {
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1) // Whisper-standard
        };

        _waveIn.DataAvailable += (s, e) =>
        {
            var buffer = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, buffer, e.BytesRecorded);
            _channel.Writer.TryWrite(buffer);
        };

        _waveIn.StartRecording();

        await foreach (var data in _channel.Reader.ReadAllAsync(ct))
        {
            yield return data;
        }
    }

    public void Stop() => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}
