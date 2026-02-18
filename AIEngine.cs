using NAudio.Utils;

namespace BinaryBeat.Infrastructure;

public class AiEngine : IAEngine
{
    private readonly IAProcessor _processor;
    private readonly IAudioStreamer _streamer;
    private readonly ChordMapper _mapper;
    private readonly MidiService _midi;

    // En kanal som fungerar som en trådsäker kö för ljud-chunks
    private readonly Channel<byte[]> _audioQueue = Channel.CreateUnbounded<byte[]>();

    public AiEngine(IAProcessor processor, IAudioStreamer streamer, ChordMapper mapper, MidiService midi)
    {
        _processor = processor;
        _streamer = streamer;
        _mapper = mapper;
        _midi = midi;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // 1. Initiera allt först
        _midi.Initialize();
        await _processor.InitializeAsync();
        
        // 2. Starta två parallella uppgifter
        // Task.Run ser till att de körs på egna trådar och inte blockerar varandra
        var captureTask = Task.Run(() => CaptureAudio(ct), ct);
        var analyzeTask = Task.Run(() => AnalyzeAudio(ct), ct);

        await Task.WhenAll(captureTask, analyzeTask);
    }

    private async Task CaptureAudio(CancellationToken ct)
    {
        // Denna loop gör BARA en sak: Hämtar ljud och lägger i kön
        await foreach (var chunk in _streamer.StreamAudioAsync(ct))
        {
            await _audioQueue.Writer.WriteAsync(chunk, ct);
        }
    }

    private async Task AnalyzeAudio(CancellationToken ct)
    {
        List<byte> pcmBuffer = new();
        MusicalChord? activeChord = null;

        // Denna loop plockar från kön så fort det finns data
        await foreach (var chunk in _audioQueue.Reader.ReadAllAsync(ct))
        {
            pcmBuffer.AddRange(chunk);
           
            if (pcmBuffer.Count >= 48000) // 1.5sek
            {
                var data = pcmBuffer.ToArray();
                pcmBuffer.Clear();
              
                // Här kör vi AI:n utan att störa CaptureAudio-loopen
                var results = await _processor.ProcessAudioAsync(data, ct).ToListAsync(ct);

                if (results.Any(r => !string.IsNullOrWhiteSpace(r.Text))) 
                { 
                    var speech = results[0];
                    var detected = _mapper.MapToChord(speech.Text, speech.Confidence);
                    Console.WriteLine(speech);
                    if (detected != null && (activeChord == null || activeChord.Name != detected.Name))
                    {
                        if (activeChord != null) _midi.SendChord(activeChord.MidiNotes, false);
                        _midi.SendChord(detected.MidiNotes, true);
                        activeChord = detected;
                        Console.WriteLine($"[MATCH] {detected.Name}");
                    }
                }
                else if (activeChord != null)
                {
                    _midi.SendChord(activeChord.MidiNotes, false);
                    activeChord = null;
                }
            }
        }
    }
}