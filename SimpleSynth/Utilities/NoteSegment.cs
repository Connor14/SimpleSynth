using MidiSharp.Events;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Synths;
using System;

namespace SimpleSynth.Utilities
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
        public Tuple<int, byte, int, byte> Identifier { get; private set; }

        public NoteSegment(MidiSynth synth, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> noteOnEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> noteOffEvent)
        {
            Track = track;
            NoteOnEvent = noteOnEvent;
            NoteOffEvent = noteOffEvent;

            StartSample = Conversions.ConvertTicksToSamples(synth.MicrosecondsPerTick, NoteOnEvent.Time);
            DurationSamples = Conversions.ConvertTicksToSamples(synth.MicrosecondsPerTick, NoteOffEvent.Time - NoteOnEvent.Time);

            Identifier = new Tuple<int, byte, int, byte>(Track, NoteOnEvent.MidiEvent.Channel, DurationSamples, NoteOnEvent.MidiEvent.Note);
        }
    }
}
