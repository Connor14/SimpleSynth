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
        public MidiSynth Synth { get; private set; }
        public byte Channel { get; private set; }
        public byte Note { get; private set; }

        public long StartTick { get; private set; }
        public long TickCount { get; private set; } = -1; // MUST be initialized to -1

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

        public DiscreteSignal SignalMix { get; protected set; }

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
        }

        // MUST be called before attempting to use the SignalMix propery
        public void UpdateSignalMix()
        {
            SignalMix = GetSignalMix();
        }

        protected abstract DiscreteSignal GetSignalMix();

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
                case SignalType.Adsr:
                    // short, medium, long
                    double attackTime = .01; // .01, .05, or .1
                    double decayTime = .02; // .02, .1, or .2
                    double releaseTime = .015; // .015, .075, or .15
                    signal = new AdsrBuilder(attackTime, decayTime, DurationSeconds - (attackTime + decayTime + releaseTime), releaseTime)
                        .OfLength(sampleCount)
                        .SampledAt(SynthConsts.SAMPLE_RATE)
                        .Build();
                    break;
                default:
                    throw new Exception();
            }

            return signal;
        }
    }
}
