using MidiSharp.Events.Voice.Note;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Parsing
{
    /// <summary>
    /// Reprents a note that might be played by a woodwind, brass, pitched percussion, or other pitched instrument.
    /// </summary>
    public class MelodicNoteSegment : NoteSegment
    {
        public double Frequency { get; }

        public MelodicNoteSegment(
            MidiInterpretation interpretation, 
            int track, 
            MidiEventWithTime<OnNoteVoiceMidiEvent> onNoteEvent, 
            MidiEventWithTime<OffNoteVoiceMidiEvent> offNoteEvent,
            int durationSamples) 
            : base(interpretation, track, onNoteEvent, offNoteEvent, durationSamples)
        {
            Frequency = Conversions.NoteToFrequency(Note);
        }
    }
}
