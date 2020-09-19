using MidiSharp.Events.Voice.Note;
using SimpleSynth.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Creates NoteSegment instances for a MidiInterpretation.
    /// </summary>
    public interface INoteSegmentProvider
    {
        /// <summary>
        /// Create a NoteSegment for the given parameters.
        /// </summary>
        /// <param name="tempoCollection"></param>
        /// <param name="track"></param>
        /// <param name="onNoteEvent"></param>
        /// <param name="offNoteEvent"></param>
        /// <returns></returns>
        NoteSegment CreateNoteSegment(TempoCollection tempoCollection, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> onNoteEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> offNoteEvent);
    }
}
