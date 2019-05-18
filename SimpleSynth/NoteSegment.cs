using NWaves.Operations;
using NWaves.Signals;
using NWaves.Signals.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSynth
{

    public class NoteSegment
    {
        public MidiSegmenter Segmenter;
        public byte Channel;
        public byte Note;

        public long StartTick;

        // The time this note starts with respect to the whole MIDI
        public double StartSeconds
        {
            get
            {
                int ticksPerBeat = Segmenter.Sequence.Division; // 192 ticks/beat
                int microsecondsPerBeat = Segmenter.TempoMicroSecondsPerBeat; // 600000 uS / beat

                double microsecondsPerTick = (double)microsecondsPerBeat / ticksPerBeat;
                long totalMicroseconds = (long)(StartTick * microsecondsPerTick);

                return (double)totalMicroseconds / 1000000;
            }
        }

        // The sample this note starts on (in relation to the ENTIRE midi)
        public int StartSample
        {
            get
            {
                return (int)(SynthConsts.SampleRate * StartSeconds);

            }
        }

        public bool Complete
        {
            get
            {
                return TickCount != -1;
            }
        }

        public long TickCount = -1;
        // The duration of this note in seconds
        public double DurationSeconds
        {
            get
            {
                int ticksPerBeat = Segmenter.Sequence.Division; // 192 ticks/beat
                int microsecondsPerBeat = Segmenter.TempoMicroSecondsPerBeat; // 600000 uS / beat

                double microsecondsPerTick = (double)microsecondsPerBeat / ticksPerBeat;
                long totalMicroseconds = (long)(TickCount * microsecondsPerTick);

                return (double)totalMicroseconds / 1000000;
            }
        }

        // The duraton of this note in samples
        public int DurationSamples
        {
            get
            {
                return (int)(SynthConsts.SampleRate * DurationSeconds);
            }
        }

        public NoteSegment(MidiSegmenter segmenter, byte channel, byte note, long startTick)
        {
            this.Segmenter = segmenter;
            this.Channel = channel;
            this.Note = note;
            this.StartTick = startTick;
        }

        public void SetEndingTick(long endTick)
        {
            this.TickCount = endTick - StartTick;
        }

        public DiscreteSignal GetSignalMix(SignalType[] types, bool includeAdsr)
        {
            if (types == null || types.Length == 0)
            {
                throw new Exception();
            }

            DiscreteSignal mainSignal = GetSignal(types[0]);

            for (int t = 1; t < types.Length; t++)
            {
                DiscreteSignal signal = GetSignal(types[t]);

                mainSignal.Combine(signal);
            }

            if (includeAdsr)
            {
                DiscreteSignal adsr = GetSignal(SignalType.Adsr);

                for (int i = 0; i < mainSignal.Samples.Length; i++)
                {
                    mainSignal[i] = mainSignal.Samples[i] * adsr.Samples[i] * .5f;
                }
            }

            return mainSignal;
        }

        private DiscreteSignal GetSignal(SignalType signalType)
        {
            DiscreteSignal signal = null;

            int sampleCount = DurationSamples;
            double frequency = SynthUtils.NoteToFrequency(Note);

            switch (signalType)
            {
                case SignalType.Sine:
                    signal = new SineBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("phase", Math.PI / 6)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SampleRate)
                        .Build();
                    break;
                case SignalType.Sawtooth:
                    signal = new SawtoothBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SampleRate)
                        .Build();
                    break;
                case SignalType.Triangle:
                    signal = new TriangleWaveBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SampleRate)
                        .Build();
                    break;
                case SignalType.Square:
                    signal = new SquareWaveBuilder()
                        .SetParameter("low", -0.25f)
                        .SetParameter("high", 0.25f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SampleRate)
                        .Build();
                    break;
                case SignalType.Adsr:
                                            // short, medium, long
                    double attackTime = .01; // .01, .05, or .1
                    double decayTime = .02; // .02, .1, or .2
                    double releaseTime = .015; // .015, .075, or .15
                    signal = new AdsrBuilder(attackTime, decayTime, DurationSeconds - (attackTime + decayTime + releaseTime), releaseTime)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SampleRate)
                        .Build();
                    break;
                default:
                    throw new Exception();
            }

            return signal;
        }
    }
}
