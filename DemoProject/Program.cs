using SimpleSynth;
using SimpleSynth.Parameters;
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
            Console.Write("Enter path to MIDI: ");
            string inputMidi = Console.ReadLine();

            Console.Write("Enter WAV output path: ");
            string outputWav = Console.ReadLine();

            Console.Write("Enter harmonic count: ");
            int harmonicCount = int.Parse(Console.ReadLine());

            Console.WriteLine("Starting...");
            using (var stream = File.OpenRead(inputMidi))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                MidiSynth synth = new HarmonicSynth(stream, harmonicCount, AdsrParameters.Default);

                MemoryStream result = synth.GenerateWAV();

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
        }
    }
}
