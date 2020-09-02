using MidiSharp;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleSynth.Parsing
{
    public class MidiInterpretation
    {
        public MidiSequence MidiFile { get; private set; }
        public double MicrosecondsPerTick { get; private set; }
        public int TotalDurationSamples { get; private set; }
        public List<NoteSegment> NoteSegments { get; private set; }

        public MidiInterpretation(Stream midiStream)
        {
            MidiFile = MidiSequence.Open(midiStream);

            TempoMetaMidiEvent tempoEvent = MidiFile.Tracks[0].OfType<TempoMetaMidiEvent>().First();

            int ticksPerBeat = MidiFile.TicksPerBeatOrFrame;
            int microsecondsPerBeat = tempoEvent.Value; // microseconds / beat

            MicrosecondsPerTick = (double)microsecondsPerBeat / (double)ticksPerBeat;

            NoteSegments = new List<NoteSegment>();

            // Calculate the absolute times for all events in each track
            // Also pair note on events with note off events
            for (int track = 0; track < MidiFile.Tracks.Count; track++)
            {
                // Key is a tuple of Channel and Note
                var onEvents = new Dictionary<(byte Channel, int Note), Queue<MidiEventWithTime<OnNoteVoiceMidiEvent>>>();

                long time = 0;
                foreach (var midiEvent in MidiFile.Tracks[track].Events)
                {
                    if (midiEvent.DeltaTime > 0)
                    {
                        time += midiEvent.DeltaTime;
                    }

                    if (midiEvent is OnNoteVoiceMidiEvent onNote)
                    {
                        // Skip the percussion channel
                        if (onNote.Channel == (byte)SpecialChannel.Percussion)
                            continue;

                        var onNoteIdentifier = (onNote.Channel, onNote.Note);

                        if (!onEvents.ContainsKey(onNoteIdentifier))
                        {
                            onEvents[onNoteIdentifier] = new Queue<MidiEventWithTime<OnNoteVoiceMidiEvent>>();
                        }

                        onEvents[onNoteIdentifier].Enqueue(new MidiEventWithTime<OnNoteVoiceMidiEvent>(time, onNote));
                    }
                    else if (midiEvent is OffNoteVoiceMidiEvent offNote)
                    {
                        // Skip the percussion channel
                        if (offNote.Channel == (byte)SpecialChannel.Percussion)
                            continue;

                        var offNoteIdentifer = (offNote.Channel, offNote.Note);

                        NoteSegments.Add(new NoteSegment(
                            this,
                            track,
                            onEvents[offNoteIdentifer].Dequeue(), // Get the first matching On Event 
                            new MidiEventWithTime<OffNoteVoiceMidiEvent>(time, offNote))
                        );
                    }
                }
            }

            TotalDurationSamples = NoteSegments.Max(segment => segment.StartSample + segment.DurationSamples);
        }
    }
}
