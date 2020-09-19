using MidiSharp;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Parsing
{
    /// <summary>
    /// Reprents a note that might be played non-pitched percussion instrument (bass drum, snare drum, crash symbol, hi-hat, etc).
    /// </summary>
    public class PercussionNoteSegment : NoteSegment
    {
        /// <summary>
        /// The type of percussion instrument as defined by the General MIDI standard.
        /// </summary>
        public GeneralMidiPercussion PercussionInstrument { get; }

        /// <summary>
        /// A generalization of the PercussionInstrument property.
        /// </summary>
        public PercussionType PercussionType { get; }

        public PercussionNoteSegment(
            MidiInterpretation interpretation,
            int track,
            MidiEventWithTime<OnNoteVoiceMidiEvent> onNoteEvent,
            MidiEventWithTime<OffNoteVoiceMidiEvent> offNoteEvent,
            int durationSamples,
            GeneralMidiPercussion instrument)
            : base(interpretation, track, onNoteEvent, offNoteEvent, durationSamples)
        {
            PercussionInstrument = instrument;
            PercussionType = PercussionHelper.GetPercussionType(instrument);
        }
    }
}
