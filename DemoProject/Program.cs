using SimpleSynth.EventArguments;
using SimpleSynth.Parameters;
using SimpleSynth.Synths;
using System;
using System.Diagnostics;
using System.IO;

namespace DemoProject
{
    public class Program
    {
        private static object consoleSync = new object();

        static void Main(string[] args)
        {
            Console.Write("Enter path to MIDI: ");
            string inputMidi = Console.ReadLine();

            Console.Write("Enter WAV output path: ");
            string outputWav = Console.ReadLine();

            Console.WriteLine("Starting...");
            using (var stream = File.OpenRead(inputMidi))
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                MidiSynth synth = new BasicSynth(stream, AdsrParameters.Default);
                synth.ProgressChanged += Synth_ProgressChanged;

                MemoryStream result = synth.GenerateWAV();

                stopwatch.Stop();
                Console.WriteLine("Rendered in: " + stopwatch.Elapsed);

                using (var outputStream = File.OpenWrite(outputWav))
                {
                    result.CopyTo(outputStream);
                }

                result.Dispose();
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        // Whenever there is an event during the GenerateWAV function, we will receive an event
        private static void Synth_ProgressChanged(object sender, EventArgs e)
        {
            // Synchronize the console so that setting the cursor position behave correctly
            lock (consoleSync)
            {
                if (e is GenerationProgressChangedEventArgs)
                {
                    // If the cursor is not at the beginning of the line, we are likely at the end of a line from a regular "Console.Write"
                    // In this case, go to a new line
                    if(Console.CursorLeft != 0)
                    {
                        Console.WriteLine();
                    }

                    Console.WriteLine(e);
                }
                else if (e is NoteRenderedEventArguments)
                {
                    // Set the cursor position to the beginning of the line and overwrite the text
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(e);
                }
            }
        }
    }
}
