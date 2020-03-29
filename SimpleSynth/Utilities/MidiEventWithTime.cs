using MidiSharp.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Utilities
{
    public struct MidiEventWithTime<TEvent> where TEvent : MidiEvent
    {
        public long Time { get; set; }
        public TEvent MidiEvent { get; set; }

        public MidiEventWithTime(long time, TEvent midiEvent)
        {
            Time = time;
            MidiEvent = midiEvent;
        }
    }
}
