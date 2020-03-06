using NAudio.Midi;
using SimpleSynth.Synths;
using SimpleSynth.Utilities;
using System;

namespace SimpleSynth.Notes
{
    public class NoteSegment
    {
        public NoteOnEvent NoteOnEvent;

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
        public Tuple<int, int, int, int> Identifier { get; private set; }

        public NoteSegment(MidiSynth synth, int track, NoteOnEvent noteOnEvent)
        {
            Track = track;
            NoteOnEvent = noteOnEvent;

            StartSample = Conversions.ConvertTicksToSamples(synth.MicrosecondsPerTick, NoteOnEvent.AbsoluteTime);
            DurationSamples = Conversions.ConvertTicksToSamples(synth.MicrosecondsPerTick, NoteOnEvent.OffEvent.AbsoluteTime - NoteOnEvent.AbsoluteTime);

            Identifier = new Tuple<int, int, int, int>(Track, NoteOnEvent.Channel, DurationSamples, NoteOnEvent.NoteNumber);
        }
    }
}
