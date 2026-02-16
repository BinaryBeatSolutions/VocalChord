using System.Runtime.InteropServices;

namespace BinaryBeat.Interop;

/// <summary>
/// VST3 bridge. Expose the API for the VST-host (DAW, Ableton, FL-Studio) to call into our AI-logic in C#.
/// </summary>
public static class VstBridge
{
    [UnmanagedCallersOnly(EntryPoint = "process_audio_chunk")]
    public static unsafe void ProcessAudioChunk(byte* audioData, int length)
    {
        // Här tar vi emot rådata VST-hosten (Ableton -> JUCE -> Hit)
        // Och skickar in i din AI-logik
    }

    [UnmanagedCallersOnly(EntryPoint = "get_detected_chord")]
    public static IntPtr GetDetectedChord()
    {
        // Returnerar det senast detekterade ackordet till VST-interfacet
        return Marshal.StringToCoTaskMemAnsi("C Major");
    }
}