using NWaves.Signals;
using SimpleSynth.Parameters;
using SimpleSynth.Parsing;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Provides ADSR envelopes for synthesizer implementations.
    /// </summary>
    public class DefaultAdsrEnvelopeProvider : IAdsrEnvelopeProvider
    {
        public AdsrParameters MelodicAdsrParameters { get; }

        public DefaultAdsrEnvelopeProvider(AdsrParameters melodicAdsrParameters = null)
        {
            MelodicAdsrParameters = melodicAdsrParameters ?? AdsrParameters.Short;
        }

        /// <inheritdoc />
        public DiscreteSignal CreateEnvelope(NoteSegment segment)
        {
            var adsrParameters = MelodicAdsrParameters;

            if (segment is PercussionNoteSegment percussionNoteSegment)
            {
                adsrParameters = GetPercussionAdsrParameters(percussionNoteSegment.PercussionType);
            }

            return SignalHelper.GetAdsrEnvelope(adsrParameters, segment.DurationSamples);
        }

        private AdsrParameters GetPercussionAdsrParameters(PercussionType percussionType)
        {
            switch (percussionType)
            {
                case PercussionType.Bass:
                    // Same as default but with a AttackAmplitude of 3
                    return new AdsrParameters(0.05, 0.1, 1, 0.075, 3);
                case PercussionType.Snare:
                    return AdsrParameters.Default;
                case PercussionType.Crash:
                    return AdsrParameters.Long;
                case PercussionType.Ride:
                default:
                    return AdsrParameters.Short;
            }
        }
    }
}
