﻿using NWaves.Signals;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using SimpleSynth.Utilities;
using System.IO;

namespace SimpleSynth.Synths
{
    public class HarmonicSynth : MidiSynth
    {
        public AdsrParameters AdsrParameters { get; private set; }

        public int HarmonicCount { get; private set; }

        public HarmonicSynth(Stream midiStream, int harmonicCount, AdsrParameters adsrParameters) : base(midiStream)
        {
            HarmonicCount = harmonicCount;
            AdsrParameters = adsrParameters;
        }

        protected override DiscreteSignal Render(NoteSegment segment)
        {
            double frequency = Conversions.NoteToFrequency(segment.NoteOnEvent.NoteNumber);

            // render the first harmonic in the list to prevent duplication
            DiscreteSignal mainSignal = SignalHelper.GetSignal(SignalType.Sine, frequency * 1, segment.DurationSamples);

            // Skip the first harmonic because we already generated it as the mainSignal
            for (int i = 2; i < HarmonicCount; i++)
            {
                DiscreteSignal signal = SignalHelper.GetSignal(SignalType.Sine, frequency * i, segment.DurationSamples);

                mainSignal.CombineAdd(signal);
            }

            if (AdsrParameters != null)
            {
                mainSignal.ApplyAdsr(SignalHelper.GetAdsrEnvelope(AdsrParameters, segment.DurationSamples));
            }

            return mainSignal;
        }
    }
}
