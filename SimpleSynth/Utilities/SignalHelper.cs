using MidiSharp;
using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using System;
using System.Linq;

namespace SimpleSynth.Utilities
{
    /// <summary>
    /// Contains simple methods to help with generating DiscreteSignals.
    /// </summary>
    public static class SignalHelper
    {
        public static DiscreteSignal GetSine(double frequency, int sampleCount)
        {
            return new SineBuilder()
                .SetParameter("frequency", frequency)
                .SetParameter("low", -1f)
                .SetParameter("high", 1f)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();
        }

        public static DiscreteSignal GetSawtooth(double frequency, int sampleCount)
        {
            return new SawtoothBuilder()
                .SetParameter("frequency", frequency)
                .SetParameter("low", -1f)
                .SetParameter("high", 1f)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build(); ;
        }

        public static DiscreteSignal GetTriangle(double frequency, int sampleCount)
        {
            return new TriangleWaveBuilder()
                .SetParameter("frequency", frequency)
                .SetParameter("low", -1f)
                .SetParameter("high", 1f)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();
        }

        public static DiscreteSignal GetSquare(double frequency, int sampleCount)
        {
            return new SquareWaveBuilder()
                .SetParameter("frequency", frequency)
                .SetParameter("low", -1f)
                .SetParameter("high", 1f)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();
        }

        public static DiscreteSignal GetChirp(double startFrequency, double endFrequency, int sampleCount)
        {
            return new ChirpBuilder()
                .SetParameter("start", startFrequency)
                .SetParameter("end", endFrequency)
                .SetParameter("low", -1f)
                .SetParameter("high", 1f)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();
        }

        public static DiscreteSignal GetKarplusStrongDrum(double probability, double frequency, double stretch, double feedback, int sampleCount)
        {
            return new KarplusStrongDrumBuilder()
                .SetParameter("probability", probability)
                // correlates with wavetable length (SampleRate / frequency = wavetable length)
                .SetParameter("frequency", frequency)
                .SetParameter("stretch", stretch)
                .SetParameter("feedback", feedback)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();
        }

        public static DiscreteSignal GetAdsrEnvelope(AdsrParameters adsrParameters, int sampleCount)
        {
            DiscreteSignal signal =
                new AdsrBuilder(
                    adsrParameters.AttackDuration,
                    adsrParameters.DecayDuration,
                    adsrParameters.GetSustainDurationSeconds(Conversions.ConvertSamplesToSeconds(sampleCount)),
                    adsrParameters.ReleaseDuration
                )
                .SetParameter("attackAmp", adsrParameters.AttackAmplitude)
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();

            return signal;
        }
    }
}
