﻿using SimpleSynth;
using SimpleSynth.Synths;
using System;
using System.Diagnostics;
using System.IO;

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

            Console.Write("Harmonic count: ");
            string inputHarmonicCount = Console.ReadLine();

            Console.Write("Include ALL harmonics (n = odd only)? (Y/n): ");
            string inputAllHarmonics = Console.ReadLine();

            Console.Write("Use ADSR envelope? (Y/n): ");
            string useAdsr = Console.ReadLine();

            bool enableAdsr = true;
            if (useAdsr == "n")
            {
                enableAdsr = false;
            }

            int harmonicCount = int.Parse(inputHarmonicCount);

            bool allHarmonics = true;
            if(inputAllHarmonics == "n")
            {
                allHarmonics = false; // odd only
            }

            SignalType[] signalTypes = new SignalType[] { SignalType.Sine, SignalType.Triangle, SignalType.Square };

            Console.WriteLine("Starting...");
            using (var stream = File.OpenRead(inputMidi))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                HarmonicSynth synth = new HarmonicSynth(stream, enableAdsr, harmonicCount, allHarmonics);

                MemoryStream result = synth.GenerateWAV();
                stopwatch.Stop();

                Console.WriteLine("Finished in: " + stopwatch.Elapsed.TotalSeconds);

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
