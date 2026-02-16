using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Core
{
    

    public interface IAudioStreamer
    {
      
        // Vi skickar ut ljudet i chunks för att hålla minnesanvändningen nere
        IAsyncEnumerable<byte[]> StreamAudioAsync(CancellationToken ct);
        void Stop();
    }
}
