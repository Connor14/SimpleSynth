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
        public NoteSegment CreateNoteSegment(TempoCollection tempoCollection, int track, MidiEventWithTime<OnNoteVoiceMidiEvent> onNoteEvent, MidiEventWithTime<OffNoteVoiceMidiEvent> offNoteEvent)
        {
            // Logic for creating different types of note segments

            bool isPercussion = onNoteEvent.MidiEvent.Channel == (byte)SpecialChannel.Percussion;

            int startSample = tempoCollection.GetTotalElapsedSamplesForTime(onNoteEvent.Time);

            if (isPercussion)
            {
                var percussionInstrument = (GeneralMidiPercussion)onNoteEvent.MidiEvent.Note;
                var percussionType = PercussionHelper.GetPercussionType(percussionInstrument);
                int durationSamples = PercussionHelper.GetPercussionDurationSamples(percussionType); // Percussion has a fixed Duration depending on type of instrument

                return new PercussionNoteSegment(track, onNoteEvent, offNoteEvent, startSample, durationSamples, percussionInstrument);
            }
            else
            {
                int endSample = tempoCollection.GetTotalElapsedSamplesForTime(offNoteEvent.Time); // get the ending sample based on all tempo changes.

                // We want our duration in samples to include any tempo changes that happened during the note.
                // Therefore, we take the different in end sample and start sample to be our duration
                // This will lead to smoother accelerando and ritardando
                int durationSamples = endSample - startSample;

                return new MelodicNoteSegment(track, onNoteEvent, offNoteEvent, startSample, durationSamples);
            }
        }
    }
}
