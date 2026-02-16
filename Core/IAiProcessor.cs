using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Core
{

    public record SpeechResult(string Text, float Confidence);

    public interface IAiProcessor
    {
        IAsyncEnumerable<SpeechResult> ProcessAudioAsync(byte[] pcmData, CancellationToken ct);
    }
}
