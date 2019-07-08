using SimpleSynth;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

                // Not used if we call SetHarmonics below
                //Console.Write("Harmonic count: ");
                //string inputHarmonicCount = Console.ReadLine();

                //int harmonicCount = int.Parse(inputHarmonicCount);

                Console.WriteLine("Starting...");
                using (var stream = File.OpenRead(inputMidi))
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();
                    HarmonicSynth synth = new HarmonicSynth(stream, AdsrParameters.Short);

                    int maxKey = synth.HarmonicChannels.Keys.Max();
                    foreach (int key in synth.HarmonicChannels.Keys)
                    {
                        Console.WriteLine("Channel: " + key + ", Instrument: " + synth.HarmonicChannels[key].Instrument);
                        synth.HarmonicChannels[key].SetHarmonics(maxKey - key + 1);
                    }

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
