using MidiSharp;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Parsing;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Creates NoteSegment instances for a MidiInterpretation.
    /// </summary>
    public class DefaultNoteSegmentProvider : INoteSegmentProvider
    {
        /// <inheritdoc />
        public NoteSegment CreateNoteSegment(MidiInterpretation midiInterpretation, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> onNoteEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> offNoteEvent)
        {
            // Logic for creating different types of note segments

            bool isPercussion = onNoteEvent.MidiEvent.Channel == (byte)SpecialChannel.Percussion;

            if (isPercussion)
            {
                var percussionInstrument = (GeneralMidiPercussion)onNoteEvent.MidiEvent.Note;
                var percussionType = PercussionHelper.GetPercussionType(percussionInstrument);
                int durationSamples = PercussionHelper.GetPercussionDurationSamples(percussionType); // Percussion has a fixed Duration depending on type of instrument

                return new PercussionNoteSegment(midiInterpretation, track, onNoteEvent, offNoteEvent, durationSamples, percussionInstrument);
            }
            else
            {
                int durationSamples = Conversions.ConvertTicksToSamples(midiInterpretation.MicrosecondsPerTick, offNoteEvent.Time - onNoteEvent.Time);

                return new MelodicNoteSegment(midiInterpretation, track, onNoteEvent, offNoteEvent, durationSamples);
            }
        }
    }
}
