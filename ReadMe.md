# VocalChord by BinaryBeat solution

## VocalChord by Binarybeat description
Binarybeat is a plugin for Abelton or other DAWs that allows you to create chords using your voice only. It is designed to be simple and intuitive, making it easy for anyone to create complex chords in a seemless way.


## Technical Architecture
- **Engine:** .NET 8 LTS (Long Term Support) for enterprise stability.
- **AI Core:** Local LLM/Whisper integration via `Whisper.net` for zero-latency and total privacy (no cloud dependency).
- **Audio Stack:** NAudio for high-performance PCM stream capturing.
- **Pattern Matching:** Fuzzy logic implementation to map natural language to musical theory (Chords).
- **Deployment:** Compiled using **Native AOT** for minimal footprint and maximum performance in a production environment.
- VocalChord utilizes quantized GGML models (Base/Tiny) to ensure high-performance inference on standard CPUs without requiring a dedicated GPU."

# BinaryBeat Library
A high-performance .NET 8 library for real-time AI voice-to-midi processing.

## Integration
Designed to be embedded into VST3/AU plugins or standalone DAWs.
- **Interoperability:** Exposed via Native AOT for high-speed C++ / JUCE integration.
- **Core Engine:** Encapsulates musical theory, fuzzy matching, and local Whisper AI.


### Core
Core innehåller de gränssnitt (Interfaces) och modeller som definierar flödet.

### Domain
Domain contains rules for musik. The logic for chords, sclaes and how VocalChord translates commands.


### Infrastructure
Infrastructure-layes responsible for the technical implementation communication with ghardware and/or an external library.

Innehåll:
AI Engine Implementation: Här lever den konkreta integrationen med Whisper.net. Den hanterar laddning av modellfiler (.bin) och omvandlar råa ljudvågor till textsträngar med hjälp av lokal maskininlärning.
Audio Capturing: Implementering av ljudströmning via NAudio. Det hanterar lågnivådetaljer som samplingshastighet (16kHz), buffertstorlekar och asynkron inläsning från ljudkortets WASAPI/ASIO-drivrutiner.
Interop Logic: Innehåller bryggan (Native AOT / UnmanagedCallersOnly) som gör att ditt VST3-projekt (C++) kan kommunicera med .NET-logiken utan prestandaförlust.
Persistence & Resources: Hantering av lokala resurser, såsom att strömma AI-modeller från disk till RAM på ett effektivt sätt.