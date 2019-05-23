using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using NWaves.Audio;
using NWaves.Filters;
using NWaves.Signals;
using SimpleSynth.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleSynth.Synths
{
    public abstract class MidiSynth
    {
        public bool IncludeADSR { get; set; }

        public MidiSequence Sequence { get; set; }

        // the total number of ticks in this sequence
        public long TickCount { get; protected set; }

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

        public MidiSynth(Stream midiStream, bool includeAdsr)
        {
            Sequence = MidiSequence.Open(midiStream);

            IncludeADSR = includeAdsr;
        }

        protected abstract List<NoteSegment> GetSegments();

        public MemoryStream GenerateWAV()
        {
            float[] samples = new float[DurationSamples];
            foreach (NoteSegment segment in Segments)
            {
                // Exclude percussion because it makes the signal dirty
                if (segment.Channel == (byte)SpecialChannel.Percussion)
                {
                    continue;
                }

                long startSample = segment.StartSample;

                DiscreteSignal segmentSignal = segment.GetSignalMix();

                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    samples[startSample + i] = (samples[startSample + i] + segmentSignal.Samples[i]) / 2f; // average the data
                }

            }

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            signal.NormalizeAmplitude(1f);

            //var filter = new CombFeedforwardFilter(25);
            //signal = filter.ApplyTo(signal);

            //var frequency = 800/*Hz*/;
            //var notchFilter = new NotchFilter(frequency / signal.SamplingRate);
            //signal = notchFilter.ApplyTo(signal);

            var maFilter = new MovingAverageFilter();
            signal = maFilter.ApplyTo(signal);

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
