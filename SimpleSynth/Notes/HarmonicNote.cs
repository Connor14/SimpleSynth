using NWaves.Signals;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Notes
{
    public class HarmonicNote : NoteSegment
    {
        public int HarmonicCount;
        public bool AllHarmonics;

        public HarmonicNote(HarmonicSynth synth, byte channel, byte note, long startTick, int harmonicCount, bool allHarmonics) : base(synth, channel, note, startTick)
        {
            HarmonicCount = harmonicCount;
            AllHarmonics = allHarmonics;
        }

        public override DiscreteSignal GetSignalMix()
        {
            double frequency = SynthUtils.NoteToFrequency(this.Note);

            DiscreteSignal mainSignal = GetSignal(SignalType.Sine, frequency);

            for (int harmonic = 1; harmonic < HarmonicCount; harmonic++)
            {
                if(AllHarmonics || (!AllHarmonics && harmonic % 2 == 1))
                {
                    DiscreteSignal signal = GetSignal(SignalType.Sine, frequency * harmonic);

                    mainSignal.CombineAdd(signal);
                }
            }

            if (this.Synth.IncludeADSR)
            {
                DiscreteSignal adsr = GetSignal(SignalType.Adsr, frequency);

                for (int i = 0; i < mainSignal.Samples.Length; i++)
                {
                    mainSignal[i] = mainSignal.Samples[i] * adsr.Samples[i] * .5f;
                }
            }

            return mainSignal;
        }
    }
}
