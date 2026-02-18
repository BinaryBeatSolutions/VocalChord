// Program.cs
using static BinaryBeat.Infrastructure.Utils;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ChordMapper>();
builder.Services.AddSingleton<MidiService>();
builder.Services.AddTransient<IAiProcessor>(sp => new WhisperAiProcessor(PathResolver.GetModelPath()));
builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();
builder.Services.AddTransient<AiEngine>();

using var host = builder.Build();

var engine = host.Services.GetRequiredService<AiEngine>();

Console.WriteLine("--- BinaryBeat Solutions: VocalChord LIVE ---");


using var cts = new CancellationTokenSource();

// Hantera t.ex. CTRL+C för att stänga ner snyggt
Console.CancelKeyPress += (s, e) => {
    e.Cancel = true;
    cts.Cancel();
};

try
{
    // Motorn sköter nu Capture och Analyze parallellt
    await engine.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n[BinaryBeat] Engine stopped gracefully.");
}