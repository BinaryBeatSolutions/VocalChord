using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Core
{
    public interface IChordMapper
    {
        MusicalChord? MapToChord(string input, float confidence);
    }
}
