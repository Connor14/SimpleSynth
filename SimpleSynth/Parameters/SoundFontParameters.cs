using MidiSharp;
using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSynth.Parameters
{
    public class SoundFontParameters
    {

        public int Channel { get; private set; }
        public GeneralMidiInstrument Instrument { get; private set; }

        public SampleHeader SampleHeader { get; private set; }

        public SoundFontParameters(int channel)
        {
            this.Channel = channel;
        }

        public void SetInstrument(byte instrument)
        {
            this.Instrument = (GeneralMidiInstrument)instrument;
        }

        public void SetSampleHeader(SampleHeader sampleHeader)
        {
            this.SampleHeader = sampleHeader;
        }
    }
}
