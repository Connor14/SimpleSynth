using MidiSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSynth
{
    public class HarmonicChannel
    {

        public int Channel { get; private set; }
        public GeneralMidiInstrument Instrument { get; private set; }

        public List<double> Harmonics { get; private set; }

        public HarmonicChannel(int channel, List<double> defaultHarmonics)
        {
            this.Channel = channel;
            this.Harmonics = defaultHarmonics;
        }

        public void SetInstrument(byte instrument)
        {
            this.Instrument = (GeneralMidiInstrument)instrument;
        }

        public void SetHarmonics(int harmonicCount)
        {
            SetHarmonics(Enumerable.Range(1, harmonicCount).Select(x => (double)x));
        }

        public void SetHarmonics(IEnumerable<double> harmonics)
        {
            // since we pass by reference to the HarmonicNotes, we cannot create a new list
            this.Harmonics.Clear();
            this.Harmonics.AddRange(harmonics);
        }
    }
}
