using NAudio.Midi;
using NWaves.Audio;
using NWaves.Signals;
using SimpleSynth.Extensions;
using SimpleSynth.Notes;
using SimpleSynth.Parameters;
using SimpleSynth.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleSynth.Synths
{
    public abstract class MidiSynth
    {
        public MidiFile MidiFile { get; private set; }

        public AdsrParameters AdsrParameters { get; private set; }

        public double MicrosecondsPerTick { get; private set; }
        public int TotalDurationSamples { get; private set; }

        public List<NoteSegment> NoteSegments { get; private set; }

        public MidiSynth(Stream midiStream, AdsrParameters adsrParameters = null)
        {
            MidiFile = new MidiFile(midiStream, true);
            AdsrParameters = adsrParameters;

            TempoEvent tempoEvent = MidiFile.Events[0].OfType<TempoEvent>().First();

            int ticksPerBeat = MidiFile.DeltaTicksPerQuarterNote;
            int microsecondsPerBeat = tempoEvent.MicrosecondsPerQuarterNote;

            MicrosecondsPerTick = (double)microsecondsPerBeat / (double)ticksPerBeat;

            NoteSegments = new List<NoteSegment>();

            // for each track, select all of the NoteOnEvents and turn them into NoteSegments
            for (int track = 0; track < MidiFile.Tracks; track++)
            {
                var noteOnSegments = MidiFile.Events[track].OfType<NoteOnEvent>().Select(midiEvent => new NoteSegment(this, track, midiEvent as NoteOnEvent));

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
            // store duplicate notes for reuse
            ConcurrentDictionary<Tuple<int, int, int, int>, NoteSegment> noteSegmentCache = new ConcurrentDictionary<Tuple<int, int, int, int>, NoteSegment>();

            // store generated signals for each item in the cache
            ConcurrentDictionary<Tuple<int, int, int, int>, DiscreteSignal> signalCache = new ConcurrentDictionary<Tuple<int, int, int, int>, DiscreteSignal>();

            // find duplicate note segments
            foreach (var segment in NoteSegments)
            {
                // If the cache does not contain the key, add it
                if (!noteSegmentCache.ContainsKey(segment.Identifier))
                {
                    noteSegmentCache[segment.Identifier] = segment; // cache the first of a repeated signal
                }
            }

            // generate required signals in parallel
            Parallel.ForEach(noteSegmentCache, segment =>
            {
                signalCache[segment.Value.Identifier] = Render(segment.Value);
            });

            float[] samples = new float[TotalDurationSamples];

            // assemble the final wav
            foreach (NoteSegment segment in NoteSegments)
            {
                // Skip the percussion channnel
                if (segment.NoteOnEvent.Channel == 10)
                {
                    continue;
                }

                DiscreteSignal segmentSignal = signalCache[segment.Identifier];

                long startSample = segment.StartSample;

                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    samples[startSample + i] = samples[startSample + i] + segmentSignal.Samples[i]; // add the samples together
                }
            }

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            signal.NormalizeAmplitude(.9f); // adjust the samples to fit between [-1, 1)

            MemoryStream output;
            using (MemoryStream stream = new MemoryStream())
            {
                var waveFile = new WaveFile(signal, 32);
                waveFile.SaveTo(stream);
                output = new MemoryStream(stream.ToArray());
            }

            return output;
        }

        /// <summary>
        /// The logic to turn NoteSegments into DiscreteSignals
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        protected abstract DiscreteSignal Render(NoteSegment segment);
    }
}
