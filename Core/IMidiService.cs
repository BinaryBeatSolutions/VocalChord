namespace BinaryBeat.Core;

public interface IMidiService 
{
    void Initialize();
    void SendChord(int[] notes, bool isOn);
}