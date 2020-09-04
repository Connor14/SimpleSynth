using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using NWaves.Audio;
using NWaves.Signals;
using SimpleSynth.EventArguments;
using SimpleSynth.Extensions;
using SimpleSynth.Parsing;
using SimpleSynth.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSynth.Synths
{
    public abstract class MidiSynth
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public MidiInterpretation Interpretation { get; private set; }

        public MidiSynth(MidiInterpretation interpretation)
        {
            Interpretation = interpretation;
        }

        public MemoryStream GenerateWAV()
        {
            stopwatch.Restart();

            // unique NoteSegments
            Dictionary<(int Track, byte Channel, int DurationSamples, byte Note), NoteSegment> noteSegmentsToRender = new Dictionary<(int Track, byte Channel, int DurationSamples, byte Note), NoteSegment>();

            // find unique note segments
            foreach (var segment in Interpretation.NoteSegments)
            {
                // If the dictionary does not contain the segment's identifierr, add the segment
                if (!noteSegmentsToRender.ContainsKey(segment.Identifier))
                {
                    noteSegmentsToRender[segment.Identifier] = segment; // cache the first of a repeated signal
                }
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(1, 5, "Duplicate note consolidation", stopwatch.Elapsed));
            stopwatch.Restart();

            int notesRendered = 0;
            int totalToRender = noteSegmentsToRender.Count;

            // unique DiscreteSignals
            ConcurrentDictionary<(int Track, byte Channel, int DurationSamples, byte Note), DiscreteSignal> signalCache = new ConcurrentDictionary<(int Track, byte Channel, int DurationSamples, byte Note), DiscreteSignal>();

            // generate unique signals in parallel
            Parallel.ForEach(noteSegmentsToRender, segment =>
            {
                signalCache[segment.Value.Identifier] = Render(segment.Value);

                Interlocked.Increment(ref notesRendered);
                OnProgressChanged(new NoteRenderedEventArguments(notesRendered, totalToRender, "Note rendered in parallel"));
            });

            OnProgressChanged(new GenerationProgressChangedEventArgs(2, 5, "Unique note rendering", stopwatch.Elapsed));
            stopwatch.Restart();

            float[] samples = new float[Interpretation.TotalDurationSamples];

            // assemble the final wav
            foreach (NoteSegment segment in Interpretation.NoteSegments)
            {
                DiscreteSignal segmentSignal = signalCache[segment.Identifier];

                long startSample = segment.StartSample;

                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    samples[startSample + i] = samples[startSample + i] + segmentSignal.Samples[i]; // add the samples together
                }
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(3, 5, "Wave assembly", stopwatch.Elapsed));
            stopwatch.Restart();

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            signal.ScaleAmplitude(0.9f); // adjust the samples to fit between [-1, 1] (using a value of 1 sometimes seems to cause clipping)

            OnProgressChanged(new GenerationProgressChangedEventArgs(4, 5, "Wave normalization", stopwatch.Elapsed));
            stopwatch.Restart();

            MemoryStream output;
            using (MemoryStream stream = new MemoryStream())
            {
                var waveFile = new WaveFile(signal, 32);
                waveFile.SaveTo(stream);
                output = new MemoryStream(stream.ToArray());
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(5, 5, "MemoryStream writing", stopwatch.Elapsed));
            stopwatch.Stop();

            return output;
        }

        /// <summary>
        /// The logic to turn NoteSegments into DiscreteSignals
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        protected abstract DiscreteSignal Render(NoteSegment segment);

        public event EventHandler ProgressChanged;

        protected virtual void OnProgressChanged(EventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
