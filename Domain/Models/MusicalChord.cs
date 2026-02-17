

namespace BinaryBeat.Domain.Models
{
    public record MusicalChord(
     string Name,          // T.ex. "C Major"
     int[] MidiNotes,      // T.ex. { 60, 64, 67 }
     float Confidence      // AI-säkerheten från Whisper
 );
}
