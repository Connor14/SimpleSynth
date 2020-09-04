using MidiSharp;
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

        public int Track { get; }

        /// <summary>
        /// This note's start time in samples
        /// </summary>
        public int StartSample { get; }

        /// <summary>
        /// This note's total time in samples
        /// </summary>
        public int DurationSamples { get; }

        /// <summary>
        /// Identifies this note based on its Track, Channel, Duration, and Note Number.
        /// 
        /// This does NOT uniquely identify a note. Rather, it is used to determine if a note can be reused. 
        /// </summary>
        public (int Track, byte Channel, int DurationSamples, byte Note) Identifier { get; private set; }

        /// <summary>
        /// Returns true if this NoteSegment represents the percussion channel.
        /// </summary>
        public bool IsPercussion => NoteOnEvent.MidiEvent.Channel == (byte)SpecialChannel.Percussion;

        /// <summary>
        /// Returns the general type of percussion or null if not percussion.
        /// </summary>
        public PercussionType? PercussionType => IsPercussion ? SignalHelper.GetPercussionType((GeneralMidiPercussion)NoteOnEvent.MidiEvent.Note) : (PercussionType?)null;

        public NoteSegment(MidiInterpretation interpretation, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> noteOnEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> noteOffEvent)
        {
            Track = track;
            NoteOnEvent = noteOnEvent;
            NoteOffEvent = noteOffEvent;

            StartSample = Conversions.ConvertTicksToSamples(interpretation.MicrosecondsPerTick, NoteOnEvent.Time);

            // Percussion has a fixed Duration depending on type of instrument

            // TODO: this logic is too specific and not extendable. 
            // Need to provide a different mechanism for specifying percussion note length since users of the library might want to provide their own drum synthesis
            if(IsPercussion)
            {
                DurationSamples = SignalHelper.GetPercussionSampleCount(PercussionType.Value);
            }
            else
            {
                DurationSamples = Conversions.ConvertTicksToSamples(interpretation.MicrosecondsPerTick, NoteOffEvent.Time - NoteOnEvent.Time);
            }

            Identifier = (Track, NoteOnEvent.MidiEvent.Channel, DurationSamples, NoteOnEvent.MidiEvent.Note);
        }
    }
}
