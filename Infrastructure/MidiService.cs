using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace BinaryBeat.Infrastructure;

public class MidiService : IDisposable
{
    private OutputDevice? _outputDevice;
    private readonly string _portName = "BinaryBeat MIDI Out";

    public void Initialize()
    {
        // Vi letar specifikt efter porten i loopMIDI
        _outputDevice = OutputDevice.GetAll()
            .FirstOrDefault(d => d.Name.Equals("BinaryBeat Out", StringComparison.OrdinalIgnoreCase));

        if (_outputDevice != null)
        {
            Console.WriteLine($"[MIDI] Success: Connected to {_outputDevice.Name}");
        }
        else
        {
            Console.WriteLine("[MIDI] Error: 'BinaryBeat Out' not found. Is loopMIDI running?");
        }
    }

    public void SendChord(int[] notes, bool isOn)
    {
        if (_outputDevice == null) return;

        foreach (var note in notes)
        {
            var midiEvent = isOn
                ? (MidiEvent)new NoteOnEvent((SevenBitNumber)note, (SevenBitNumber)100)
                : (MidiEvent)new NoteOffEvent((SevenBitNumber)note, (SevenBitNumber)0);

            _outputDevice.SendEvent(midiEvent);
        }
    }

    public void Dispose() => _outputDevice?.Dispose();
}
