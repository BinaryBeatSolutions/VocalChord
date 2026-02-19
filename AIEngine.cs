using NAudio.Utils;

using static BinaryBeat.Infrastructure.Utils.AudioUtils;

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

    public async Task RunAsync()
    {
        _midi.Initialize();

        if (_processor != null)
        {
            await _processor.InitializeAsync();
        }

        Console.WriteLine(">>> BinaryBeat lyssnar nu. Säg ett ackord (t.ex. 'C Major')...");

        using var liveCts = new CancellationTokenSource();

        var pcmBuffer = new List<byte>();

        MusicalChord lastChord = null;

        await foreach (var audioChunk in _streamer.StreamAudioAsync(liveCts.Token))
        {
            pcmBuffer.AddRange(audioChunk);

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