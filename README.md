# SimpleSynth
A simple C# library that can read a MIDI file and spit out a WAV file.

## About

I created this project so that I had a simple way to create WAV files out of the MIDI files from the **AbundantMusic.NET** project (https://github.com/Connor14/AbundantMusic.NET). As of right now, it just synthesizes sound using multiple harmonics of a Sine wave, but it could potentially be expanded to support more forms of synthesis.

## Tools / Libraries

##### SimpleSynth

* .NET Standard 2.0
* NWaves (https://github.com/ar1st0crat/NWaves)
* MidiSharp (https://github.com/stephentoub/MidiSharp)

##### DemoProject

* .NET Core 2.2

## Getting Started

Create an instance of the `HarmonicSynth` class and run the `GenerateWAV` method. The resulting `MemoryStream` is your synthesized WAV file. You can save the `MemoryStream` to disk or consume it in some other way.

```
...
using SimpleSynth;
using SimpleSynth.Synths;
...
HarmonicSynth synth = new HarmonicSynth(midiFileStream, new AdsrParameters(), harmonicCount, allHarmonics);
MemoryStream result = synth.GenerateWAV();
...
```

See `DemoProject` for a more detailed example.

## License Information

##### NWaves

**NWaves** is listed under the *MIT* license and includes *Copyright (c) 2017 Tim*.

##### MidiSharp

**MidiSharp** is listed under the *MIT* license and includes *Copyright (c) 2014 Stephen Toub*.

## Future Development

Ideally I would like to implement better sounding synths. Maybe even a SoundFont synth.
