using SimpleSynth;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DemoProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                Console.Write("Enter path to MIDI: ");
                string inputMidi = Console.ReadLine();

                Console.Write("Enter WAV output path: ");
                string outputWav = Console.ReadLine();

                Console.Write("Harmonic count: ");
                string inputHarmonicCount = Console.ReadLine();

                int harmonicCount = int.Parse(inputHarmonicCount);

                SignalType[] signalTypes = new SignalType[] { SignalType.Sine, SignalType.Triangle, SignalType.Square };

                Console.WriteLine("Starting...");
                using (var stream = File.OpenRead(inputMidi))
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();
                    HarmonicSynth synth = new HarmonicSynth(stream, new AdsrParameters(), harmonicCount);

                    Console.WriteLine("Segmented in: "+ stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();

                    MemoryStream result = await synth.GenerateWAV();

                    stopwatch.Stop();
                    Console.WriteLine("Rendered in: " + stopwatch.Elapsed.TotalSeconds);

                    using (var outputStream = File.OpenWrite(outputWav))
                    {
                        result.CopyTo(outputStream);
                    }

                    result.Dispose();
                }

                Console.WriteLine("Done");
                Console.ReadLine();
            }).Wait();
        }
    }
}
