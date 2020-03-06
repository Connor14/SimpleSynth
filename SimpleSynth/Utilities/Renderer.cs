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

    public static class Renderer
    {
        public static DiscreteSignal GetSignal(SignalType signalType, double frequency, int sampleCount)
        {
            DiscreteSignal signal = null;

            switch (signalType)
            {
                case SignalType.Sine:
                    signal = new SineBuilder()
                        .SetParameter("frequency", frequency)
                        .SetParameter("phase", Math.PI / 6)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Sawtooth:
                    signal = new SawtoothBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Triangle:
                    signal = new TriangleWaveBuilder()
                        .SetParameter("low", -0.3f)
                        .SetParameter("high", 0.3f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                case SignalType.Square:
                    signal = new SquareWaveBuilder()
                        .SetParameter("low", -0.25f)
                        .SetParameter("high", 0.25f)
                        .SetParameter("freq", frequency)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return signal;
        }

        public static DiscreteSignal GetAdsrEnvelope(double frequency, AdsrParameters adsrParameters, int sampleCount)
        {
            DiscreteSignal signal = 
                new AdsrBuilder(
                    adsrParameters.AttackTime, 
                    adsrParameters.DecayTime, 
                    adsrParameters.GetSustainTime(Conversions.ConvertSamplesToSeconds(sampleCount)),
                    adsrParameters.ReleaseTime
                )
                .OfLength(sampleCount)
                .SampledAt(Constants.SAMPLE_RATE)
                .Build();

            return signal;
        }
    }
}
