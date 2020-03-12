using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Parameters;
using System;

namespace SimpleSynth.Utilities
{
    public enum SignalType
    {
        Sine,
        Sawtooth,
        Triangle,
        Square
    }

    public static class SignalHelper
    {
        public static DiscreteSignal GetSignal(SignalType signalType, double frequency, int sampleCount)
        {
            DiscreteSignal signal = null;

            switch (signalType)
            {
                case SignalType.Sine:
                    signal = new SineBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Sawtooth:
                    signal = new SawtoothBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Triangle:
                    signal = new TriangleWaveBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Square:
                    signal = new SquareWaveBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return signal;
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
