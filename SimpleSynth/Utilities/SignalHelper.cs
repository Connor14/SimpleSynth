using MidiSharp;
using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using System;
using System.Linq;

namespace SimpleSynth.Utilities
{
    public enum SignalType
    {
        Sine,
        Sawtooth,
        Triangle,
        Square
    }

    public enum PercussionType
    {
        Bass,
        Snare,
        Crash,
        Ride
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

        // todo Abundant Music WebAudioPlayer has synthesis settings. How did it do the percussion?
        public static DiscreteSignal GetPercussionSignal(PercussionType type)
        {
            // Inspiration for KarplusStrongDrumBuilder settings: https://ccrma.stanford.edu/~sdill/220A-project/drums.html

            int sampleCount = GetPercussionSampleCount(type);

            switch (type)
            {
                // Bass drums
                case PercussionType.Bass:
                    var bass = new ChirpBuilder()
                        .SetParameter("start", 90)
                        .SetParameter("end", 120)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    var bass2 = new ChirpBuilder()
                        .SetParameter("start", 180)
                        .SetParameter("end", 240)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    var bass3 = new ChirpBuilder()
                        .SetParameter("start", 270)
                        .SetParameter("end", 360)
                        .SetParameter("low", -1f)
                        .SetParameter("high", 1f)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    // The chirp builder goes in the "up" direction, so we need to reverse it to go "down"
                    bass = new DiscreteSignal(Constants.SAMPLE_RATE, bass.Samples.Reverse());
                    bass2 = new DiscreteSignal(Constants.SAMPLE_RATE, bass2.Samples.Reverse());
                    bass3 = new DiscreteSignal(Constants.SAMPLE_RATE, bass3.Samples.Reverse());

                    // Combine the two bass signals so that the bass has more "presence"
                    bass = bass.Superimpose(bass2).Superimpose(bass3);

                    bass.Amplify(5);

                    // Same as default but with a AttackAmplitude of 3
                    var adsrParameters = new AdsrParameters(0.05, 0.1, 1, 0.075, 3);
                    bass.ApplyAdsr(GetAdsrEnvelope(adsrParameters, sampleCount));

                    return bass;

                // Snare drums
                case PercussionType.Snare:
                    var snare = new KarplusStrongDrumBuilder()
                        .SetParameter("probability", 0.5)
                        // correlates with wavetable length (SampleRate / frequency = wavetable length)
                        .SetParameter("frequency", 30)
                        .SetParameter("stretch", 1)
                        .SetParameter("feedback", 1)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    snare.Amplify(5);
                    snare.ApplyAdsr(GetAdsrEnvelope(AdsrParameters.Default, sampleCount));

                    return snare;

                // Crash drums
                case PercussionType.Crash:
                    var crash = new KarplusStrongDrumBuilder()
                        .SetParameter("probability", 0.5)
                        // correlates with wavetable length (SampleRate / frequency = wavetable length)
                        .SetParameter("frequency", 11)
                        .SetParameter("stretch", 1)
                        .SetParameter("feedback", 1)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    crash.Amplify(5);
                    crash.ApplyAdsr(GetAdsrEnvelope(AdsrParameters.Long, sampleCount));
                    return crash;

                // Ride drums
                case PercussionType.Ride:
                // Defaulting to a ride drum probably isn't good, but it will sound in lots of situations.
                default:
                    var ride = new KarplusStrongDrumBuilder()
                        .SetParameter("probability", 0.5)
                        // correlates with wavetable length (SampleRate / frequency = wavetable length)
                        .SetParameter("frequency", 60)
                        .SetParameter("stretch", 0.1)
                        .SetParameter("feedback", 1)
                        .OfLength(sampleCount)
                        .SampledAt(Constants.SAMPLE_RATE)
                        .Build();

                    ride.Amplify(5);
                    ride.ApplyAdsr(GetAdsrEnvelope(AdsrParameters.Short, sampleCount));

                    return ride;
            }
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

        // Groupings pulled from Abundant Music > Song Settings > Domains
        // To simplify synthesis, all instruments in a group will use the same signal generator.
        public static PercussionType GetPercussionType(GeneralMidiPercussion instrument)
        {
            switch (instrument)
            {
                // Bass drums
                case GeneralMidiPercussion.HighFloorTom:
                case GeneralMidiPercussion.LowFloorTom:
                case GeneralMidiPercussion.LowBongo:
                case GeneralMidiPercussion.LowConga:
                case GeneralMidiPercussion.LowTimbale:
                case GeneralMidiPercussion.BassDrum1:
                case GeneralMidiPercussion.BassDrum:
                    return PercussionType.Bass;
                // Snare drums
                case GeneralMidiPercussion.AcousticSnare:
                case GeneralMidiPercussion.HandClap:
                case GeneralMidiPercussion.HighTom:
                case GeneralMidiPercussion.HiMidTom:
                case GeneralMidiPercussion.LowMidTom:
                case GeneralMidiPercussion.LowTom:
                case GeneralMidiPercussion.OpenHiConga:
                case GeneralMidiPercussion.ElectricSnare:
                    return PercussionType.Snare;
                // Crash drums
                case GeneralMidiPercussion.ChineseCymbal:
                case GeneralMidiPercussion.SplashCymbal:
                case GeneralMidiPercussion.CrashCymbal1:
                case GeneralMidiPercussion.CrashCymbal2:
                    return PercussionType.Crash;
                // Ride drums
                case GeneralMidiPercussion.Maracas:
                case GeneralMidiPercussion.OpenTriangle:
                case GeneralMidiPercussion.MuteTriangle:
                case GeneralMidiPercussion.Claves:
                case GeneralMidiPercussion.RideBell:
                case GeneralMidiPercussion.SideStick:
                case GeneralMidiPercussion.RideCymbal:
                case GeneralMidiPercussion.RideCymbal2:
                case GeneralMidiPercussion.PedalHiHat:
                case GeneralMidiPercussion.ClosedHiHat:
                case GeneralMidiPercussion.OpenHiHat:
                // Defaulting to a ride drum probably isn't good, but it will sound ok in lots of situations.
                default:
                    return PercussionType.Ride;
            }
        }

        public static int GetPercussionSampleCount(PercussionType type)
        {
            // 440 ms (Default for Kicker in LMMS)
            int percussionSampleCount = Constants.PERCUSSION_SAMPLE_COUNT;

            // Bass drum has a shorter sample count to be "punchier"
            if (type == PercussionType.Bass)
                return (int)(percussionSampleCount * 0.5);

            return percussionSampleCount;
        }
    }
}
