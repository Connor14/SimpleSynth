using MidiSharp.Events;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Parsing;
using SimpleSynth.Synths;
using SimpleSynth.Utilities;
using System;

namespace SimpleSynth.Parsing
{
    public class NoteSegment
    {
        public MidiEventWithTime<OnNoteVoiceMidiEvent> NoteOnEvent;
        public MidiEventWithTime<OffNoteVoiceMidiEvent> NoteOffEvent;

        public int Track;

        /// <summary>
        /// This note's start time in samples
        /// </summary>
        public int StartSample;

        /// <summary>
        /// This note's total time in samples
        /// </summary>
        public int DurationSamples;

        /// <summary>
        /// Identifies this note based on its Track, Channel, Duration, and Note Number.
        /// 
        /// This does NOT uniquely identify a note. Rather, it is used to determine if a note can be reused. 
        /// </summary>
        public (int Track, byte Channel, int DurationSamples, byte Note) Identifier { get; private set; }

        public NoteSegment(MidiInterpretation interpretation, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> noteOnEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> noteOffEvent)
        {
            Track = track;
            NoteOnEvent = noteOnEvent;
            NoteOffEvent = noteOffEvent;

            StartSample = Conversions.ConvertTicksToSamples(interpretation.MicrosecondsPerTick, NoteOnEvent.Time);
            DurationSamples = Conversions.ConvertTicksToSamples(interpretation.MicrosecondsPerTick, NoteOffEvent.Time - NoteOnEvent.Time);

            Identifier = (Track, NoteOnEvent.MidiEvent.Channel, DurationSamples, NoteOnEvent.MidiEvent.Note);
        }
    }
}
