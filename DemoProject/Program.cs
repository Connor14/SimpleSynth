using SimpleSynth;
using System;
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

            Console.Write("Use ADSR envelope? (Y/n): ");
            string useAdsr = Console.ReadLine();

            bool enableAdsr = true;
            if (useAdsr == "n")
            {
                enableAdsr = false;
            }

            SignalType[] signalTypes = new SignalType[] { SignalType.Sine, SignalType.Triangle, SignalType.Square, SignalType.Sawtooth };

            Console.WriteLine("Starting...");
            using (var stream = File.OpenRead(inputMidi))
            {
                MemoryStream result = MidiSynth.GenerateWAV(stream, signalTypes, enableAdsr);

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
