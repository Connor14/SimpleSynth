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

        protected override DiscreteSignal GetSignalMix()
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

            mainSignal.NormalizeAmplitude(1f); // might help make a nicer ADSR effect because we don't normalize amplitudes AFTER ADSR until the final WAV is made. Just a theory.

            if (this.Synth.AdsrParameters != null)
            {
                DiscreteSignal adsr = GetAdsrEnvelope(frequency, Synth.AdsrParameters);

                for (int i = 0; i < mainSignal.Samples.Length; i++)
                {
                    mainSignal[i] = mainSignal.Samples[i] * adsr.Samples[i] * .5f;
                }
            }

            return mainSignal;
        }
    }
}
