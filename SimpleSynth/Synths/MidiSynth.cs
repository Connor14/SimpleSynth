using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using NWaves.Audio;
using NWaves.Filters;
using NWaves.Signals;
using SimpleSynth.Notes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSynth.Synths
{
    public abstract class MidiSynth
    {
        public AdsrParameters AdsrParameters { get; set; }

        public MidiSequence Sequence { get; set; }

        // the total number of ticks in this sequence
        public long TickCount { get; protected set; }

        public TimeSpan Duration
        {
            get
            {
                return TimeSpan.FromSeconds(DurationSeconds);
            }
        }

        // the duration of the MIDI in seconds
        public double DurationSeconds
        {
            get
            {
                int ticksPerBeat = Sequence.Division; // 192 ticks/beat
                int microsecondsPerBeat = TempoMicroSecondsPerBeat; // 600000 uS / beat

                double microsecondsPerTick = (double)microsecondsPerBeat / ticksPerBeat;
                long totalMicroseconds = (long)(TickCount * microsecondsPerTick);

                return (double)totalMicroseconds / 1000000;
            }
        }

        // the duratioon of the MIDI in samples
        public int DurationSamples
        {
            get
            {
                return (int)(SynthConsts.SAMPLE_RATE * DurationSeconds);
            }
        }


        // the MIDI tempo in microseconds / quarter note
        public int TempoMicroSecondsPerBeat { get; protected set; } = -1; // MUST be initialized to -1

        // The Tempo in BPM
        public int TempoBeatsPerMinute
        {
            get
            {
                return SynthConsts.MICROSECONDS_PER_MINUTE / TempoMicroSecondsPerBeat;
            }
        }

        // All of the note data including their start times, durations, etc
        public List<NoteSegment> Segments { get; set; }

        public MidiSynth(Stream midiStream, AdsrParameters adsrParameters = null)
        {
            Sequence = MidiSequence.Open(midiStream);

            AdsrParameters = adsrParameters;
        }

        protected abstract List<NoteSegment> GetSegments();

        public async Task<MemoryStream> GenerateWAV()
        {
            return await Task.Run(() => { return GenerateWAVSynchronous(); });
        }

        private MemoryStream GenerateWAVSynchronous()
        {
            // store frequently used notes
            ConcurrentDictionary<Tuple<byte, byte, long>, NoteSegment> segmentCache = new ConcurrentDictionary<Tuple<byte, byte, long>, NoteSegment>();
            // store the frequently used signals
            ConcurrentDictionary<Tuple<byte, byte, long>, DiscreteSignal> noteSignals = new ConcurrentDictionary<Tuple<byte, byte, long>, DiscreteSignal>();

            // generate signals in parallel
            // saves significant amounts of time (Abundant Music.mid finished in 8 seconds syncronously @ 10 harmonics, 2.3 in parallel @ 10 harmonics)
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // find duplicate note segments
            foreach (var segment in Segments)
            {
                Tuple<byte, byte, long> identifier = new Tuple<byte, byte, long>(segment.Channel, segment.Note, segment.TickCount);

                if (!segmentCache.ContainsKey(identifier))
                {
                    segmentCache[identifier] = segment; // cache the first of a repeated signal
                }
            }

            // generate signals for duplicate notes
            List<NoteSegment> cachedSegments = segmentCache.Select(x => x.Value).ToList();
            Parallel.ForEach(cachedSegments, segment =>
            {
                noteSignals[segment.Identifier] = segment.GetSignalMix();
            });

            Debug.WriteLine("Rendering: " + stopwatch.Elapsed.TotalSeconds);
            stopwatch.Restart();

            float[] samples = new float[DurationSamples];

            // there was inconsistent sound between MIDI files because they had slightly different event orders and the averaging of the signals was the wrong way to do things
            // With the averaging method, we could make it top or bottom heavy depending on the sort. 
            //var OrdedSegments = Segments.OrderBy(seg => seg.Note).ThenByDescending(seg => seg.DurationSamples);
            //Console.WriteLine(OrdedSegments.First().DurationSamples + " " + OrdedSegments.First().Note);

            // assemble the final wav
            foreach (NoteSegment segment in Segments)
            {
                Tuple<byte, byte, long> identifier = new Tuple<byte, byte, long>(segment.Channel, segment.Note, segment.TickCount);

                if (segment.Channel == (byte)SpecialChannel.Percussion)
                {
                    continue;
                }

                long startSample = segment.StartSample;

                DiscreteSignal segmentSignal = noteSignals[identifier];

                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    // NOTE: Averaging the frequencies wasn't mathematically correct. 
                    // Adding them and then normalizing later gives the proper sound, regardless of MIDI order
                    samples[startSample + i] = samples[startSample + i] + segmentSignal.Samples[i]; // add the samples together
                }
            }

            stopwatch.Stop();
            Debug.WriteLine("Combining: " + stopwatch.Elapsed.TotalSeconds);

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            signal.NormalizeAmplitude(.9f); // adjust the samples to fit between [-1, 1)

            //var filter = new CombFeedforwardFilter(25);
            //signal = filter.ApplyTo(signal);

            //var frequency = 800/*Hz*/;
            //var notchFilter = new NotchFilter(frequency / signal.SamplingRate);
            //signal = notchFilter.ApplyTo(signal);

            // NOTE: I don't believe this is necessary when using the correct sample adding technique. I don't hear ANY pops.
            //var maFilter = new MovingAverageFilter();
            //signal = maFilter.ApplyTo(signal);

            MemoryStream output = null;
            using (MemoryStream stream = new MemoryStream())
            {
                var waveFile = new WaveFile(signal, 32);
                waveFile.SaveTo(stream);
                output = new MemoryStream(stream.ToArray());
            }

            return output;
        }
    }
}
