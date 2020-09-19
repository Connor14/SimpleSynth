using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Utilities
{
    public static class Conversions
    {
        // abundant-music audioplayer.js
        // https://newt.phys.unsw.edu.au/jw/notes.html
        public static double NoteToFrequency(int note)
        {
            var n = note - 69; // A4;
            var p = Math.Pow(2.0, n / 12.0);

            return 440.0 * p;
        }

        public static int ConvertTicksToSamples(double microsecondsPerTick, long ticks)
        {
            double microseconds = ticks * microsecondsPerTick;
            double seconds = (double)microseconds / (double)Constants.MICROSECONDS_PER_SECOND;

            int samples = (int)(Constants.SAMPLE_RATE * seconds);

            return samples;
        }

        public static double ConvertSamplesToSeconds(int samples)
        {
            return (double)samples / Constants.SAMPLE_RATE;
        }
    }
}
