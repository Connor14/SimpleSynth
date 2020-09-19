using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Parsing;
using SimpleSynth.Synths;
using SimpleSynth.Utilities;
using System;

namespace SimpleSynth.Parsing
{
    /// <summary>
    /// Represents a particular note in a MIDI file.
    /// </summary>
    public abstract class NoteSegment
    {
        /// <summary>
        /// Identifies this note based on its Track, Channel, Duration, and Note number.
        /// 
        /// This does NOT uniquely identify a note. Rather, it is used to determine if a note can be reused. 
        /// </summary>
        public (int Track, byte Channel, int DurationSamples, byte Note) ReuseIdentifier { get; }

        public int Track { get; }

        public byte Channel { get; }

        /// <summary>
        /// The MIDI Note number
        /// </summary>
        public byte Note { get; }

        public byte Velocity { get; }

        public MidiEventWithTime<OnNoteVoiceMidiEvent> NoteOnEvent { get; }
        public MidiEventWithTime<OffNoteVoiceMidiEvent> NoteOffEvent { get; }

        /// <summary>
        /// This note's start time in samples
        /// </summary>
        public int StartSample { get; }

        /// <summary>
        /// This note's total time in samples
        /// </summary>
        public int DurationSamples { get; }

        public NoteSegment(
            int track, 
            MidiEventWithTime<OnNoteVoiceMidiEvent> noteOnEvent, 
            MidiEventWithTime<OffNoteVoiceMidiEvent> noteOffEvent, 
            int startSample,
            int durationSamples)
        {
            ReuseIdentifier = (track, noteOnEvent.MidiEvent.Channel, durationSamples, noteOnEvent.MidiEvent.Note);

            Track = track;
            Channel = noteOnEvent.MidiEvent.Channel;
            Note = noteOnEvent.MidiEvent.Note;
            Velocity = noteOnEvent.MidiEvent.Velocity;

            NoteOnEvent = noteOnEvent;
            NoteOffEvent = noteOffEvent;

            StartSample = startSample;
            DurationSamples = durationSamples;
        }
    }
}
