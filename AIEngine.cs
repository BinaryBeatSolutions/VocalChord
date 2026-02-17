using Whisper.net;

namespace BinaryBeat;

public class AIEngine
{
    private IAudioStreamer? _streamer;
    private IAiProcessor? _iProcessor;
    private IChordMapper? _chordMapper;
    private IMidiService? _midiService;
    private List<byte> _pcmBuffer;
    private MusicalChord _lastChord = null;
    private CancellationTokenSource _liveCts;

    public AIEngine(IAudioStreamer iAudioStreamer, 
        IAiProcessor _iAiProcessor, 
        IChordMapper iChordMapper, 
        IMidiService iMidiservice)
    {
        this._streamer = iAudioStreamer;
        this._iProcessor = _iAiProcessor;
        this._chordMapper = iChordMapper;
        this._midiService = iMidiservice;

        _pcmBuffer = new List<byte>();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[BinaryBeat] Listening. Say a chord (for.eg. 'C Major')...");
       
        _liveCts = new CancellationTokenSource();
    }

    public async Task Listen()
    {
        await foreach (var audioChunk in _streamer.StreamAudioAsync(_liveCts.Token))
        {
            _pcmBuffer.AddRange(audioChunk);

            if (_pcmBuffer.Count >= 44000)
            {
                var rawData = _pcmBuffer.ToArray();
                _pcmBuffer.Clear();

                var results = await _iProcessor.ProcessAudioAsync(rawData, _liveCts.Token).ToListAsync();

                if (results.Any())
                {
                    var speech = results.First();
                    var currentChord = _chordMapper.MapToChord(speech.Text, speech.Confidence);
                    MusicalChord? activeChord = null;

                    if (currentChord != null && (activeChord == null || activeChord.Name != currentChord.Name))
                    {
                        // 1. Stoppa föregående ackord
                        if (activeChord != null)
                            _midiService.SendChord(activeChord.MidiNotes, false);

                        // 2. Starta det nya
                        _midiService.SendChord(currentChord.MidiNotes, true);
                        activeChord = currentChord;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[MIDI] Playing {currentChord.Name}");
                    }
                }
                else
                {
                    // AI:n hörde ingenting (tystnad) -> Återställ!
                    if (_lastChord != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[IDLE] No speech detected - Resetting.");
                        // midiService.SendNoteOff(lastChord.MidiNotes);
                        _lastChord = null;
                    }
                }
            }
        }
    }
}