
using System.Drawing;

var _modelpath = Utils.PathResolver.GetModelPath();
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAiProcessor>(sp => new WhisperAiProcessor(_modelpath));
builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();
builder.Services.AddSingleton<IChordMapper, ChordMapper>();
builder.Services.AddSingleton<IMidiService, MidiService>();

using var host = builder.Build();

var iAiProcessor = host.Services.GetRequiredService<IAiProcessor>() as WhisperAiProcessor;
var iAudioStreamer = host.Services.GetRequiredService<IAudioStreamer>();
var iChordMapper = host.Services.GetRequiredService<IChordMapper>();
var iMidiService = host.Services.GetRequiredService<IMidiService>();

//Start AI
if (iAiProcessor != null)
    await iAiProcessor.InitializeAsync();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- VocalChord by BinaryBeat Solutions---");
Console.ForegroundColor = ConsoleColor.Green;

await (new AIEngine(iAudioStreamer, iAiProcessor, iChordMapper, iMidiService).Listen());

await host.RunAsync();