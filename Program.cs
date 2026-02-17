

var builder = Host.CreateApplicationBuilder(args);
using var host = builder.ConfigureServices().Build();

Console.WriteLine("--- VocalChord by BinaryBeat AI Engine Starting ---");

var aiProcessor = host.Services.GetRequiredService<IAiProcessor>() as WhisperAiProcessor;
var midiService = host.Services.GetRequiredService<MidiService>();
var mapper = host.Services.GetRequiredService<ChordMapper>();
var streamer = host.Services.GetRequiredService<IAudioStreamer>();

if (aiProcessor != null) await aiProcessor.InitializeAsync();

midiService.Initialize();


Console.WriteLine(">>> BinaryBeat is listening. Say a chord (for.eg. 'C Major')...");


using var liveCts = new CancellationTokenSource();

// Enkel lista för att ackumulera ljud (buffring)
var pcmBuffer = new List<byte>();

MusicalChord? lastChord = null;

await foreach (var audioChunk in streamer.StreamAudioAsync(liveCts.Token))
{
    pcmBuffer.AddRange(audioChunk);

    if (pcmBuffer.Count >= 44000)
    {
        var rawData = pcmBuffer.ToArray();
        pcmBuffer.Clear();

        var results = await aiProcessor.ProcessAudioAsync(rawData, liveCts.Token).ToListAsync();

        if (results.Any())
        {
            var speech = results.First();
            var currentChord = mapper.MapToChord(speech.Text, speech.Confidence);
            MusicalChord? activeChord = null;

            if (currentChord != null && (activeChord == null || activeChord.Name != currentChord.Name))
            {
                // 1. Stoppa föregående ackord
                if (activeChord != null)
                    midiService.SendChord(activeChord.MidiNotes, false);

                // 2. Starta det nya
                midiService.SendChord(currentChord.MidiNotes, true);
                activeChord = currentChord;

                Console.WriteLine($"[MIDI] Playing {currentChord.Name}");
            }
        }
        else
        {
            // AI:n hörde ingenting (tystnad) -> Återställ!
            if (lastChord != null)
            {
                Console.WriteLine("[IDLE] No speech detected - Resetting.");
                // midiService.SendNoteOff(lastChord.MidiNotes);
                lastChord = null;
            }
        }
    }
}

await host.RunAsync();