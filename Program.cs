
using static BinaryBeat.Infrastructure.Utils;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ChordMapper>();
builder.Services.AddSingleton<MidiService>();
builder.Services.AddTransient<IAProcessor>(sp => new WhisperAiProcessor(PathResolver.GetModelPath()));
builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();
builder.Services.AddTransient<AiEngine>();

using var host = builder.Build();

var engine = host.Services.GetRequiredService<AiEngine>();

using var cts = new CancellationTokenSource();
await engine.RunAsync(cts.Token);
