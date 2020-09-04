using MidiSharp;
using NWaves.Signals;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using SimpleSynth.Parsing;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleSynth.Synths
{
    public class BasicSynth : MidiSynth
    {
        public AdsrParameters AdsrParameters { get; private set; }

        public BasicSynth(MidiInterpretation interpretation, AdsrParameters adsrParameters) : base(interpretation)
        {
            AdsrParameters = adsrParameters;
        }

        protected override DiscreteSignal Render(NoteSegment segment)
        {
            DiscreteSignal mainSignal = null;

            if (segment.IsPercussion)
            {
                mainSignal = SignalHelper.GetPercussionSignal(segment.PercussionType.Value);
            }
            else
            {
                double frequency = Conversions.NoteToFrequency(segment.NoteOnEvent.MidiEvent.Note);

                // Combine a Sine wave and a Square wave 
                mainSignal = SignalHelper.GetSignal(SignalType.Sine, frequency, segment.DurationSamples);
                mainSignal.CombineAdd(SignalHelper.GetSignal(SignalType.Square, frequency, segment.DurationSamples));

                // A simple way of doing an equalizer
                // Scale the signals based on whether or not they lie within the given frequency range
                // todo improve the "equalizer" by allowing parameters to be provided.
                float factor = 1.0f;

                if (frequency <= 160)
                {
                    factor = 2.0f;
                }
                else if (frequency <= 2500)
                {
                    factor = 1.0f;
                }
                else
                {
                    factor = 0.5f;
                }

                mainSignal *= factor; // is this the correct operation?

                if (AdsrParameters != null)
                {
                    mainSignal.ApplyAdsr(SignalHelper.GetAdsrEnvelope(AdsrParameters, segment.DurationSamples));
                }
            }

            return mainSignal;
        }
    }
}
