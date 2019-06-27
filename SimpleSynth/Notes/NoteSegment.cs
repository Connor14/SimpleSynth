using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Notes
{
    public abstract class NoteSegment
    {
        public Tuple<byte, byte, long> Identifier { get; private set; } = null;

        public MidiSynth Synth { get; private set; }
        public byte Channel { get; private set; } // possible identifier
        public byte Note { get; private set; } // possible identifier

        public long StartTick { get; private set; }
        public long TickCount { get; private set; } = -1; // MUST be initialized to -1. possible identifier

        // The time this note starts with respect to the whole MIDI
        public double StartSeconds { get; private set; }

        // The duration of this note in seconds
        public double DurationSeconds { get; private set; }

        // The sample this note starts on (in relation to the ENTIRE midi)
        public int StartSample
        {
            get
            {
                return (int)(SynthConsts.SAMPLE_RATE * StartSeconds);

            }
        }

        // The duraton of this note in samples
        public int DurationSamples
        {
            get
            {
                return (int)(SynthConsts.SAMPLE_RATE * DurationSeconds);
            }
        }

        public bool Complete
        {
            get
            {
                return TickCount != -1;
            }
        }

        public NoteSegment(MidiSynth synth, byte channel, byte note, long startTick)
        {
            this.Synth = synth;
            this.Channel = channel;
            this.Note = note;
            this.StartTick = startTick;
        }

        public void SetEndingTick(long endTick)
        {
            this.TickCount = endTick - StartTick;

            int ticksPerBeat = Synth.Sequence.Division; // 192 ticks/beat
            int microsecondsPerBeat = Synth.TempoMicroSecondsPerBeat; // 600000 uS / beat

            double microsecondsPerTick = (double)microsecondsPerBeat / ticksPerBeat;

            long startMicroseconds = (long)(StartTick * microsecondsPerTick);
            long totalMicroseconds = (long)(TickCount * microsecondsPerTick);

            this.StartSeconds = (double)startMicroseconds / 1000000;
            this.DurationSeconds = (double)totalMicroseconds / 1000000;

            this.Identifier = new Tuple<byte, byte, long>(Channel, Note, TickCount);
        }

        public abstract DiscreteSignal GetSignalMix();

        protected DiscreteSignal GetSignal(SignalType signalType, double frequency)
        {
            DiscreteSignal signal = null;

            int sampleCount = DurationSamples;

            switch (signalType)
            {
                case SignalType.Sine:
                    signal = new SineBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("phase", Math.PI / 6)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Sawtooth:
                    signal = new SawtoothBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Triangle:
                    signal = new TriangleWaveBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Square:
                    signal = new SquareWaveBuilder()
                        .SetParameter("low", -0.25f)
                        .SetParameter("high", 0.25f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SAMPLE_RATE)
                        .Build();
                    break;
                default:
                    throw new Exception();
            }

            return signal;
        }

        protected DiscreteSignal GetAdsrEnvelope(double frequency, AdsrParameters adsrParameters)
        {
            int sampleCount = DurationSamples;

            DiscreteSignal signal = new AdsrBuilder(adsrParameters.AttackTime, adsrParameters.DecayTime, adsrParameters.GetSustainTime(DurationSeconds), adsrParameters.ReleaseTime)
                .OfLength(sampleCount)
                .SampledAt(SynthConsts.SAMPLE_RATE)
                .Build();

            return signal;
        }
    }
}
