using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace BinaryBeat.Infrastructure;

public class MidiService : IDisposable
{
    private OutputDevice? _outputDevice;
    private const string VirtualPortName = "BinaryBeat MIDI Out";

    public void Initialize()
    {
        try
        {
            // Vi försöker hitta en port (t.ex. skapad i loopMIDI) 
            // eller använda Microsoft GS Wavetable som fallback för test
            _outputDevice = OutputDevice.GetAll().FirstOrDefault(d => d.Name.Contains("loopMIDI"))
                            ?? OutputDevice.GetAll().FirstOrDefault();

            if (_outputDevice != null)
            {
                Console.WriteLine($"[MIDI] Connected to: {_outputDevice.Name}");
            }
            else
            {
                Console.WriteLine("[MIDI] No output device found. Install loopMIDI for best results.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MIDI] Initialization error: {ex.Message}");
        }
    }

    public void SendChord(int[] midiNotes, bool isOn, int velocity = 100)
    {
        if (_outputDevice == null) return;

        foreach (var noteNumber in midiNotes)
        {
            // DryWetMidi använder SevenBitNumber för att garantera MIDI-standard (0-127)
            var note = (SevenBitNumber)noteNumber;
            var vel = (SevenBitNumber)velocity;

            MidiEvent midiEvent = isOn
                ? new NoteOnEvent(note, vel)
                : new NoteOffEvent(note, (SevenBitNumber)0);

            _outputDevice.SendEvent(midiEvent);
        }
    }

    public void Dispose()
    {
        _outputDevice?.Dispose();
    }
}