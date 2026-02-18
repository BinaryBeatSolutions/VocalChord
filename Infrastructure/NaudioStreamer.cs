using NAudio.Wave;

namespace BinaryBeat.Infrastructure;

public class NaudioStreamer : IAudioStreamer, IDisposable
{
    private readonly WaveInEvent _waveIn;

    public async IAsyncEnumerable<byte[]> StreamAudioAsync([EnumeratorCancellation] CancellationToken ct)
    {
        using var _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(48000, 16, 1),
            BufferMilliseconds = 1000 // Hur ofta den ska skicka data (lägre = snabbare)
        };

        // WaveInEvent är för mikrofoner. 
        // DeviceNumber 0 är oftast din standardmikrofon i Windows.


        var _channel = Channel.CreateUnbounded<byte[]>();

        _waveIn.DataAvailable += (s, e) =>
        {
            var buffer = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, buffer, e.BytesRecorded);
            _channel.Writer.WriteAsync(buffer);
            
        };
       
        Console.WriteLine(_waveIn.WaveFormat);
        _waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(Stop);
        _waveIn.StartRecording();


        Console.WriteLine(Utils.APP_START_DEV_MESSAGE);

        await foreach (var data in _channel.Reader.ReadAllAsync(ct).WithCancellation(ct))
        {
            yield return data;
        }
    }

    public void Stop<StoppedEventArgs>(object o, StoppedEventArgs e) => _waveIn?.StopRecording();
    public void Dispose() => _waveIn?.Dispose();
}
