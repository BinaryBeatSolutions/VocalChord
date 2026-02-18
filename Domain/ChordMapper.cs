using FuzzySharp;

namespace BinaryBeat.Domain;

public class ChordMapper : IChordMapper
{
    private readonly Dictionary<string, int[]> _chordDictionary = new()
    {
        { "C Major", new[] { 60, 64, 67 } },
        { "A Minor", new[] { 57, 60, 64 } },
        { "G Major", new[] { 55, 59, 62 } }
    };

    public MusicalChord MapToChord(string input, float confidence)
    {
        // Bibliotek med ackord (skulle kunna ligga i en JSON-fil senare) eller låta AI generera alla kombinationer
        var library = new Dictionary<string, int[]>
        {
            { "C Major", [60, 64, 67] },
            { "A Minor", [57, 60, 64] },
            { "G Major", [55, 59, 62] }
        };

        // FuzzySharp finds the best match
        var result = Process.ExtractOne(input, library.Keys);

        if (result.Score > 75) // Tröskelvärde för att undvika "falska" ackord
        {
            return new MusicalChord(result.Value, library[result.Value], confidence);
        }

        return null;
    }
}
