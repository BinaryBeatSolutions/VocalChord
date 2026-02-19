# VocalChord by BinaryBeat Solutions

## VocalChord description
VocalHord is a plugin (VST3) for Abelton or other DAWs that allows you to create chords using your voice only. It is designed to be simple and intuitive, making it easy for anyone to create complex chords in a seemless way.


## VocalChord technical architecture
- **Engine:** .NET 8 LTS (Long Term Support) for enterprise stability.
- **AI Core:** Local LLM/Whisper integration via `Whisper.net` for zero-latency and total privacy (no cloud dependency).
- **Audio Stack:** NAudio for high-performance PCM stream capturing.
- **Pattern Matching:** Fuzzy logic implementation to map natural language to musical theory (Chords).
- **Deployment:** Compiled using **Native AOT** for minimal footprint and maximum performance in a production environment.
- VocalChord utilizes quantized GGML models (Base/Tiny) to ensure high-performance inference on standard CPUs without requiring a dedicated GPU."

# VocalChord library
A high-performance .NET 8 library for real-time AI voice-to-midi processing.

## Integration
Designed to be embedded into VST3/AU plugins or standalone DAWs.
- **Interoperability:** Exposed via Native AOT for high-speed C++ / JUCE integration.
- **Core Engine:** Encapsulates musical theory, fuzzy matching, and local Whisper AI.


### Core
Core contains core the structure such as models and interfaces.

### Domain
Domain contains rules for music. The logic for chords, sclaes and how VocalChord translates commands.


### Infrastructure
Infrastructure-layes responsible for the technical implementation communication with hardware and/or an external library.

Contents:
AI Engine Implementation: 
This is where the concrete integration with Whisper.net lives. It handles loading model files (.bin) and converting raw audio waves to text strings using local machine learning.
Audio Capturing: Implementation of audio streaming via NAudio. It handles low-level details such as sample rate (16kHz), buffer sizes and asynchronous loading from the sound card's WASAPI/ASIO drivers.
Interop Logic: Contains the bridge (Native AOT / UnmanagedCallersOnly) that allows your VST3 project (C++) to communicate with the .NET logic without performance loss.
Persistence & Resources: Management of local resources, such as efficiently streaming AI models from disk to RAM.