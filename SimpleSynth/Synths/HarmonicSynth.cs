using NWaves.Signals;
using SimpleSynth.Extensions;
using SimpleSynth.Notes;
using SimpleSynth.Parameters;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleSynth.Synths
{
    public class HarmonicSynth : MidiSynth
    {
        public int HarmonicCount { get; private set; }

        public HarmonicSynth(Stream midiStream, int harmonicCount, AdsrParameters adsrParameters = null) : base(midiStream, adsrParameters)
        {
            HarmonicCount = harmonicCount;
        }

        protected override DiscreteSignal Render(NoteSegment segment)
        {
            double frequency = Conversions.NoteToFrequency(segment.NoteOnEvent.NoteNumber);

            // render the first harmonic in the list to prevent duplication
            DiscreteSignal mainSignal = Renderer.GetSignal(SignalType.Sine, frequency * 1, segment.DurationSamples);

            for (int i = 1; i < HarmonicCount; i++)
            {
                DiscreteSignal signal = Renderer.GetSignal(SignalType.Sine, frequency * i, segment.DurationSamples);

                mainSignal.CombineAdd(signal);
            }

            // nothing special about .9f. Just needs to be less than 1
            mainSignal.NormalizeAmplitude(.9f); // might help make a nicer ADSR effect because we don't normalize amplitudes AFTER ADSR until the final WAV is made. Just a theory.

            if (AdsrParameters != null)
            {
                DiscreteSignal adsr = Renderer.GetAdsrEnvelope(frequency, AdsrParameters, segment.DurationSamples);

                for (int i = 0; i < mainSignal.Samples.Length; i++)
                {
                    mainSignal[i] = mainSignal.Samples[i] * adsr.Samples[i] * .5f;
                }
            }

            return mainSignal;
        }
    }
}
