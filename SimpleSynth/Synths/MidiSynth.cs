using NAudio.Midi;
using NWaves.Audio;
using NWaves.Signals;
using SimpleSynth.EventArguments;
using SimpleSynth.Extensions;
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

        public MidiFile MidiFile { get; private set; }
        public double MicrosecondsPerTick { get; private set; }
        public int TotalDurationSamples { get; private set; }
        public List<NoteSegment> NoteSegments { get; private set; }

        public MidiSynth(Stream midiStream)
        {
            MidiFile = new MidiFile(midiStream, true);

            TempoEvent tempoEvent = MidiFile.Events[0].OfType<TempoEvent>().First();

            int ticksPerBeat = MidiFile.DeltaTicksPerQuarterNote;
            int microsecondsPerBeat = tempoEvent.MicrosecondsPerQuarterNote;

            MicrosecondsPerTick = (double)microsecondsPerBeat / (double)ticksPerBeat;

            NoteSegments = new List<NoteSegment>();

            // for each track, select all of the NoteOnEvents and turn them into NoteSegments
            for (int track = 0; track < MidiFile.Tracks; track++)
            {
                var noteOnSegments = MidiFile.Events[track]
                    .OfType<NoteOnEvent>()
                    .Where(midiEvent => midiEvent.Channel != 10) // Don't include the Percussion channel
                    .Select(midiEvent => new NoteSegment(this, track, midiEvent as NoteOnEvent));

                NoteSegments.AddRange(noteOnSegments);
            }

            TotalDurationSamples = NoteSegments.Max(segment => segment.StartSample + segment.DurationSamples);
        }

        public async Task<MemoryStream> GenerateWAVAsync()
        {
            return await Task.Run(() => { return GenerateWAV(); });
        }

        public MemoryStream GenerateWAV()
        {
            stopwatch.Restart();

            // unique NoteSegments
            Dictionary<Tuple<int, int, int, int>, NoteSegment> noteSegmentsToRender = new Dictionary<Tuple<int, int, int, int>, NoteSegment>();

            // find unique note segments
            foreach (var segment in NoteSegments)
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
            ConcurrentDictionary<Tuple<int, int, int, int>, DiscreteSignal> signalCache = new ConcurrentDictionary<Tuple<int, int, int, int>, DiscreteSignal>();

            // generate unique signals in parallel
            Parallel.ForEach(noteSegmentsToRender, segment =>
            {
                signalCache[segment.Value.Identifier] = Render(segment.Value);

                Interlocked.Increment(ref notesRendered);
                OnProgressChanged(new NoteRenderedEventArguments(notesRendered, totalToRender, "Note rendered in parallel"));
            });

            OnProgressChanged(new GenerationProgressChangedEventArgs(2, 5, "Unique note rendering", stopwatch.Elapsed));
            stopwatch.Restart();

            float[] samples = new float[TotalDurationSamples];

            // assemble the final wav
            foreach (NoteSegment segment in NoteSegments)
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

            signal.NormalizeAmplitude(.9f); // adjust the samples to fit between [-1, 1)

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
