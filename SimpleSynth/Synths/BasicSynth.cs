using MidiSharp;
using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using SimpleSynth.Parsing;
using SimpleSynth.Providers;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleSynth.Synths
{
    /// <summary>
    /// A basic implementation of a synthesizer.
    /// </summary>
    public class BasicSynth : MidiSynth
    {
        public IAdsrEnvelopeProvider AdsrEnvelopeProvider { get; }
        public IBalanceProvider BalanceProvider { get; }

        public BasicSynth(MidiInterpretation interpretation, IAdsrEnvelopeProvider adsrEnvelopeProvider = null, IBalanceProvider balanceProvider = null) : base(interpretation)
        {
            AdsrEnvelopeProvider = adsrEnvelopeProvider ?? new DefaultAdsrEnvelopeProvider(AdsrParameters.Short);
            BalanceProvider = balanceProvider ?? new DefaultBalanceProvider();
        }

        protected override DiscreteSignal Render(NoteSegment segment)
        {
            DiscreteSignal mainSignal = null;

            if (segment is PercussionNoteSegment percussionNoteSegment)
            {
                mainSignal = GetPercussionSignal(percussionNoteSegment.PercussionType, percussionNoteSegment.DurationSamples);
            }
            else if (segment is MelodicNoteSegment melodicNoteSegment)
            {
                // Combine a Sine wave and a Square wave 
                mainSignal = SignalHelper.GetSine(melodicNoteSegment.Frequency, melodicNoteSegment.DurationSamples);
                mainSignal.CombineAdd(SignalHelper.GetSquare(melodicNoteSegment.Frequency, melodicNoteSegment.DurationSamples));
            }

            float velocityMultiplier = segment.Velocity / 127f; // Velocity ranges from 0 - 127

            // A simple way of doing an equalizer
            float balanceMultiplier = BalanceProvider.GetMultiplier(segment);

            // Scale the signals based on their velocity and balance multipliers
            mainSignal.Amplify(velocityMultiplier * balanceMultiplier);
            mainSignal.ApplyAdsr(AdsrEnvelopeProvider.CreateEnvelope(segment));

            return mainSignal;
        }

        // todo Abundant Music WebAudioPlayer has synthesis settings. How did it do the percussion?
        private DiscreteSignal GetPercussionSignal(PercussionType percussionType, int sampleCount)
        {
            switch (percussionType)
            {
                // Bass drums
                case PercussionType.Bass:
                    var bass = SignalHelper.GetChirp(90, 120, sampleCount);
                    var bass2 = SignalHelper.GetChirp(180, 240, sampleCount);
                    var bass3 = SignalHelper.GetChirp(270, 360, sampleCount);

                    // The chirp builder goes in the "up" direction, so we need to reverse it to go "down"
                    bass = new DiscreteSignal(Constants.SAMPLE_RATE, bass.Samples.Reverse());
                    bass2 = new DiscreteSignal(Constants.SAMPLE_RATE, bass2.Samples.Reverse());
                    bass3 = new DiscreteSignal(Constants.SAMPLE_RATE, bass3.Samples.Reverse());

                    // Combine the two bass signals so that the bass has more "presence"
                    bass = bass.Superimpose(bass2).Superimpose(bass3);

                    return bass;

                // Snare drums
                case PercussionType.Snare:
                    var snare = SignalHelper.GetKarplusStrongDrum(0.5, 30, 1, 1, sampleCount);

                    return snare;

                // Crash drums
                case PercussionType.Crash:
                    var crash = SignalHelper.GetKarplusStrongDrum(0.5, 11, 1, 1, sampleCount);

                    return crash;

                // Ride drums
                case PercussionType.Ride:
                // Defaulting to a ride drum probably isn't good, but it will sound in lots of situations.
                default:
                    var ride = SignalHelper.GetKarplusStrongDrum(0.5, 60, 0.1, 1, sampleCount);

                    return ride;
            }
        }
    }
}
