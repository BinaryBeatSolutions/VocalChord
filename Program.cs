
using System.Drawing;

var _modelpath = Utils.PathResolver.GetModelPath();
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAiProcessor>(sp => new WhisperAiProcessor(_modelpath));
builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();
builder.Services.AddSingleton<IChordMapper, ChordMapper>();
builder.Services.AddSingleton<IMidiService, MidiService>();

using var host = builder.Build();

var aiProcessor = host.Services.GetRequiredService<IAiProcessor>() as WhisperAiProcessor;
var streamer = host.Services.GetRequiredService<IAudioStreamer>();
var mapper = host.Services.GetRequiredService<IChordMapper>();
var midiService = host.Services.GetRequiredService<IMidiService>();

if (aiProcessor != null) 
    await aiProcessor.InitializeAsync();

midiService.Initialize();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- VocalChord by BinaryBeat Solutions---");
Console.ForegroundColor = ConsoleColor.Green;

await (new AIEngine(streamer, aiProcessor, mapper, midiService).Listen());

await host.RunAsync();