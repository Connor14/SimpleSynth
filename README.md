# SimpleSynth
A simple C# library that can read a MIDI file and spit out a WAV file.

Find it on NuGet: https://www.nuget.org/packages/SimpleSynth/

## About

I created this project so that I had a simple way to create WAV files out of the MIDI files from the **AbundantMusic.NET** project (https://github.com/Connor14/AbundantMusic.NET). As of right now, it just synthesizes audio using a combination of a sine wave and a square wave (`BasicSynth`) or multiple harmonics of a sine wave (`HarmonicSynth`). Support for percussion synthesis was recently added to the `BasicSynth` class. 

Overall, the system can be easily expanded to support more forms of synthesis.

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
using SimpleSynth.Synths;
using System;
using System.IO;
...
using (var stream = File.OpenRead("YourMidiFile.mid"))
{
    var interpretation = new MidiInterpretation(stream);
    MidiSynth synth = new BasicSynth(interpretation, AdsrParameters.Default);
    MemoryStream result = synth.GenerateWAV();

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