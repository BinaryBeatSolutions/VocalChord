using NAudio.Mixer;
using NAudio.Wave;
using static BinaryBeat.Infrastructure.Utils.AudioUtils;

namespace BinaryBeat.Infrastructure;

public class NaudioStreamer : IAudioStreamer, IDisposable
{
    private WaveInEvent? _waveIn;
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();

    public async IAsyncEnumerable<byte[]> StreamAudioAsync([EnumeratorCancellation] CancellationToken ct)
    {    
        using (_waveIn = new WaveInEvent())
        {
            //_waveIn.DeviceNumber = 1;
            _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);

            _waveIn.DataAvailable += (s, e) =>
            {
                if (IsSilence(e.Buffer, e.BytesRecorded)) 
                    return;

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
    }

    public void Stop<StoppedEventArgs>(Object o, StoppedEventArgs e) => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}