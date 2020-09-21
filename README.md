# SimpleSynth
A simple C# library that can read a MIDI file and spit out a WAV file.

Find it on NuGet: https://www.nuget.org/packages/SimpleSynth/

## About

I created SimpleSynth so that I would have an easy way to create WAV audio files from MIDI files output by the **AbundantMusic.NET** project (https://github.com/Connor14/AbundantMusic.NET). I tried to design SimpleSynth with customization and extensibility in mind, so I am hopeful that users of the library will be able to tweak the generated audio to their liking by implementing new `Providers` or by extending `MidiSynth` and implementing new synthesizers.

The included `BasicSynth` synthesizer creates audio using a combination of a sine wave and a square wave and supports basic percussion synthesis.

## Features

Beyond synthesizing the notes that make up the harmonies and melodies of MIDI files, SimpleSynth also supports the following:

* Percussion synthesis
  * Percussion instruments are generalized into one of four types (bass, snare, crash, ride) and are synthesized as such.
* Dynamic tempo
  * MIDI files can contain tempo changes and these changes will be reflected in the final audio output.
* NoteOn event Velocity interpretation
  * The Velocity value of a NoteOn MIDI event is used to amplify a note's sound to make it louder or softer when compared to its neighbors. The results might be audible in cases where accents are used, for example.

## Tools / Libraries

##### SimpleSynth

* .NET Standard 2.0
* MidiSharp (https://github.com/stephentoub/MidiSharp)
* NWaves (https://github.com/ar1st0crat/NWaves)

##### DemoProject

* .NET Core 3.1

## Getting Started

Create an instance of the `MidiInterpretation` class and pass it to an instance of the `BasicSynth` class. Then run the `GenerateWAV` method. The resulting `MemoryStream` is your synthesized WAV file. You can save the `MemoryStream` to disk or consume it in some other way.

```
using SimpleSynth.EventArguments;
using SimpleSynth.Parameters;
using SimpleSynth.Parsing;
using SimpleSynth.Providers;
using SimpleSynth.Synths;
using System;
using System.IO;
...
using (var stream = File.OpenRead("YourMidiFile.mid"))
{
    // Parse the provided MIDI file.
    var interpretation = new MidiInterpretation(stream, new DefaultNoteSegmentProvider());

    // Create a new synthesizer with default providers.
    var synth = new BasicSynth(interpretation, new DefaultAdsrEnvelopeProvider(AdsrParameters.Short), new DefaultBalanceProvider());

    // Generate the WAV file
    MemoryStream result = synth.GenerateWAV();

    // Write WAV file to disk
    using (var outputStream = File.OpenWrite("YourOutputWave.wav"))
    {
        result.CopyTo(outputStream);
    }

    result.Dispose();
}
```

See `DemoProject` for a more detailed example.

## License Information

##### MidiSharp

**MidiSharp** is listed under the *MIT* license and includes *Copyright (c) 2014 Stephen Toub*.

##### NWaves

**NWaves** is listed under the *MIT* license and includes *Copyright (c) 2017 Tim*.

## Future Development

I would like to implement more synths for a wider range of sounds. Maybe even a SoundFont synth.

I would like to improve the efficiency of the synthesis where I can. 

I would like to implement "on-line" / realtime synthesis and playback for MIDI files.