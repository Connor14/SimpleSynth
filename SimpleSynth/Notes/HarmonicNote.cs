using NWaves.Signals;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Notes
{
    public class HarmonicNote : NoteSegment
    {
        public List<double> Harmonics { get; set; }

        public HarmonicNote(HarmonicSynth synth, byte channel, byte note, long startTick, List<double> harmonics) : base(synth, channel, note, startTick)
        {
            this.Harmonics = harmonics;
        }

        public override DiscreteSignal GetSignalMix()
        {
            double frequency = SynthUtils.NoteToFrequency(this.Note);

            DiscreteSignal mainSignal = GetSignal(SignalType.Sine, frequency);

            foreach(double harmonic in Harmonics)
            {
                DiscreteSignal signal = GetSignal(SignalType.Sine, frequency * harmonic);

                mainSignal.CombineAdd(signal);
            }

            // nothing special about .9f. Just needs to be less than 1
            mainSignal.NormalizeAmplitude(.9f); // might help make a nicer ADSR effect because we don't normalize amplitudes AFTER ADSR until the final WAV is made. Just a theory.

            if (Synth.AdsrParameters != null)
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
