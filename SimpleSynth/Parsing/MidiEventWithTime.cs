using MidiSharp.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Parsing
{
    public class MidiEventWithTime<TEvent> where TEvent : MidiEvent
    {
        public long Time { get; }
        public TEvent MidiEvent { get; }

        public MidiEventWithTime(long time, TEvent midiEvent)
        {
            Time = time;
            MidiEvent = midiEvent;
        }
    }
}
