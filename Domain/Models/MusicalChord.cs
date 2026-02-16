using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBeat.Domain.Models
{
    public record MusicalChord(
     string Name,          // T.ex. "C Major"
     int[] MidiNotes,      // T.ex. { 60, 64, 67 }
     float Confidence      // AI-säkerheten från Whisper
 );
}
