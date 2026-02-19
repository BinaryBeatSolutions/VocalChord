using NAudio.Utils;

using static BinaryBeat.Infrastructure.Utils.AudioUtils;

namespace BinaryBeat.Infrastructure;

public class AiEngine : IAEngine
{
    private readonly IAProcessor _processor;
    private readonly IAudioStreamer _streamer;
    private readonly ChordMapper _mapper;
    private readonly MidiService _midi;

    private DateTime _lastDetectionTime = DateTime.MinValue;
    private MusicalChord? _activeChord;
    private readonly TimeSpan _chordTimeout = TimeSpan.FromMilliseconds(2000);

    // En kanal som fungerar som en trådsäker kö för ljud-chunks
    private readonly Channel<byte[]> _audioQueue = Channel.CreateUnbounded<byte[]>();

    public AiEngine(IAProcessor processor, IAudioStreamer streamer, ChordMapper mapper, MidiService midi)
    {
        _processor = processor;
        _streamer = streamer;
        _mapper = mapper;
        _midi = midi;
    }


    private async Task AnalyzeAudio(CancellationTokenSource cts)
    {
        List<byte> pcmBuffer = new();

        await foreach (var chunk in _audioQueue.Reader.ReadAllAsync(cts.Token))
        {
            // 1. Watchdog: Stäng av hängande noter om det varit tyst för länge
            if (_activeChord != null && DateTime.Now - _lastDetectionTime > _chordTimeout)
            {
                _midi.SendChord(_activeChord.MidiNotes, false);
                _activeChord = null;
                Console.WriteLine("[WATCHDOG] Note Off (Silence)");
            }

            pcmBuffer.AddRange(chunk);

            if (pcmBuffer.Count >= 32000) // 1 sek @ 16kHz
            {
                var data = pcmBuffer.ToArray();
                pcmBuffer.Clear();

                var results = await _processor.ProcessAudioAsync(data, cts.Token).ToListAsync(cts.Token);

                if (results.Count > 0 && !string.IsNullOrWhiteSpace(results[0].Text))
                {
                    var speech = results[0];
                    var detected = _mapper.MapToChord(speech.Text, speech.Confidence);

                    if (detected != null)
                    {
                        // Hantera ackordbyte
                        if (_activeChord != null && _activeChord.Name != detected.Name)
                        {
                            _midi.SendChord(_activeChord.MidiNotes, false);
                        }

                        if (_activeChord == null || _activeChord.Name != detected.Name)
                        {
                            _midi.SendChord(detected.MidiNotes, true);
                            _activeChord = detected;
                        }

                        _lastDetectionTime = DateTime.Now; // Reset watchdog
                        Console.WriteLine($"[MIDI] {detected.Name}, {detected.Confidence}");
                    }
                }
            }
        }
    }


    public async Task RunAsync()
    {
        _midi.Initialize();

        if (_processor != null)
        {
            await _processor.InitializeAsync();
        }

        Console.WriteLine(">>> BinaryBeat lyssnar nu. Säg ett ackord (t.ex. 'C Major')...");

        using var liveCts = new CancellationTokenSource();


       // await AnalyzeAudio(liveCts);

        var pcmBuffer = new List<byte>();

        MusicalChord lastChord = null;

        await foreach (var audioChunk in _streamer.StreamAudioAsync(liveCts.Token))
        {
            pcmBuffer.AddRange(audioChunk);

           // Console.WriteLine(pcmBuffer.Count);


            if (pcmBuffer.Count >= 48000)
            {
                var rawData = pcmBuffer.ToArray();
                pcmBuffer.Clear();

                var results = await _processor.ProcessAudioAsync(rawData, liveCts.Token).ToListAsync();

                Console.WriteLine($"[BinaryBeat] RMS: {CalculateRMS(rawData)}");

                if (results.Any())
                {
                    var speech = results.First();
                    var currentChord = _mapper.MapToChord(speech.Text, speech.Confidence);
                    var activeChord = _midi.ActiveChord;

                    if (currentChord != null && (activeChord == null || activeChord.Name != currentChord.Name))
                    {

                        // 1. Stoppa föregående ackord
                        if (activeChord != null)
                            _midi.SendChord(activeChord.MidiNotes, false);

                        // 2. Starta det nya
                        _midi.SendChord(currentChord.MidiNotes, true);
                        _midi.ActiveChord = currentChord;

                        Console.WriteLine($"[MIDI] Playing {currentChord.Name}");
                    }
                }
                else
                {
                    // AI:n hörde ingenting (tystnad) -> Återställ!
                    if (lastChord != null)
                    {
                        Console.WriteLine("[IDLE] No speech detected - Resetting.");
                        _midi.SendNoteOff(lastChord.MidiNotes);
                        lastChord = null;
                    }
                }
            }
        }
    }
}