using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth
{
    public enum SignalType
    {
        Sine,
        Sawtooth,
        Triangle,
        Square,
        Adsr // this is an envelope, not an audio signal
    }
}
