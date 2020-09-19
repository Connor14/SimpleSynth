using MidiSharp;
using NWaves.Signals;
using NWaves.Signals.Builders;
using SimpleSynth.Extensions;
using SimpleSynth.Parameters;
using System;
using System.Linq;

namespace SimpleSynth.Utilities
{
    public enum PercussionType
    {
        Bass,
        Snare,
        Crash,
        Ride
    }

    public static class PercussionHelper
    {
        // Groupings pulled from Abundant Music > Song Settings > Domains
        // To simplify synthesis, all instruments in a group will use the same signal generator.
        /// <summary>
        /// Generalizes a General MIDI percussion instrument to a PercussionType.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the duration of a percussion note based on its generalized type. Used by the default INoteSegmentProvider implementation.
        /// </summary>
        /// <param name="percussionType"></param>
        /// <returns></returns>
        public static int GetPercussionDurationSamples(PercussionType percussionType)
        {
            // 440 ms (when samping at 44100 hz). This is the default for Kicker in LMMS
            int percussionSampleCount = 19404;

            // Bass drum has a shorter sample count to be "punchier"
            if (percussionType == PercussionType.Bass)
                return (int)(percussionSampleCount * 0.5);

            return percussionSampleCount;
        }
    }
}
