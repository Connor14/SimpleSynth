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
            double frequency = Conversions.NoteToFrequency(segment.NoteOnEvent.MidiEvent.Note);

            // Combine a Sine wave and a Square wave 
            DiscreteSignal mainSignal = SignalHelper.GetSignal(SignalType.Sine, frequency, segment.DurationSamples);
            mainSignal.CombineAdd(SignalHelper.GetSignal(SignalType.Square, frequency, segment.DurationSamples));

            if (AdsrParameters != null)
            {
                mainSignal.ApplyAdsr(SignalHelper.GetAdsrEnvelope(AdsrParameters, segment.DurationSamples));
            }

            return mainSignal;
        }
    }
}
