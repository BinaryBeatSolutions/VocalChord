using BinaryBeat.Core;
using BinaryBeat.Domain;
using BinaryBeat.Domain.Models;
using BinaryBeat.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

// 1. Sätt upp Dependency Injection och Konfiguration
var builder = Host.CreateApplicationBuilder(args);

var _modelpath = PathResolver.GetModelPath("ggml-tiny.bin");


// Get the directory where the executing assembly (EXE) is located
// Registrera dina komponenter
builder.Services.AddSingleton<ChordMapper>();
builder.Services.AddSingleton<MidiService>();
builder.Services.AddSingleton<IAiProcessor>(sp =>
    new WhisperAiProcessor(_modelpath)); // Sökväg till modell

// Här kan du lägga till din AudioStreamer senare
builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();

using var host = builder.Build();

// 2. Starta logiken
Console.WriteLine("--- BinaryBeat AI Engine Starting ---");

var aiProcessor = host.Services.GetRequiredService<IAiProcessor>() as WhisperAiProcessor;
if (aiProcessor != null)
{
    await aiProcessor.InitializeAsync();
}

var midiService = host.Services.GetRequiredService<MidiService>();
var mapper = host.Services.GetRequiredService<ChordMapper>();
var streamer = host.Services.GetRequiredService<IAudioStreamer>();

midiService.Initialize();

Console.WriteLine(">>> BinaryBeat lyssnar nu. Säg ett ackord (t.ex. 'C Major')...");

using var liveCts = new CancellationTokenSource();

// Vi använder en enkel lista för att ackumulera ljud (buffring)
var pcmBuffer = new List<byte>();

MusicalChord? lastChord = null;

await foreach (var audioChunk in streamer.StreamAudioAsync(liveCts.Token))
{
    pcmBuffer.AddRange(audioChunk);

    if (pcmBuffer.Count >= 48000)
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