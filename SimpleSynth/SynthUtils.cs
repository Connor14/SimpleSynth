using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth
{
    public static class SynthUtils
    {
        // abundant-music audioplayer.js
        // https://newt.phys.unsw.edu.au/jw/notes.html
        public static double NoteToFrequency(int note)
        {
            var n = note - 69; // A4;
            var p = Math.Pow(2.0, n / 12.0);

            return 440.0 * p;
        }
    }
}
