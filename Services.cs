using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat;

public static class Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static HostApplicationBuilder ConfigureServices(this IHostApplicationBuilder builder) 
    {
        var _modelpath = Utils.PathResolver.GetModelPath();

        builder.Services.AddSingleton<ChordMapper>();
        builder.Services.AddSingleton<MidiService>();
        builder.Services.AddSingleton<IAiProcessor>(sp => new WhisperAiProcessor(_modelpath));
        builder.Services.AddSingleton<IAudioStreamer, NaudioStreamer>();

        return (HostApplicationBuilder)builder;
    }
}