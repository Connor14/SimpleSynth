using MidiSharp;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using SimpleSynth.Providers;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleSynth.Parsing
{
    /// <summary>
    /// Contains information about a parsed MIDI file.
    /// </summary>
    public class MidiInterpretation
    {
        public MidiSequence MidiFile { get; private set; }
        public double MicrosecondsPerTick { get; private set; }
        public int TotalDurationSamples { get; private set; }
        public List<NoteSegment> NoteSegments { get; private set; }

        public MidiInterpretation(Stream midiStream) : this(midiStream, new DefaultNoteSegmentProvider())
        {
            
        }

        public MidiInterpretation(Stream midiStream, INoteSegmentProvider noteSegmentProvider)
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
                        var onNoteIdentifier = (onNote.Channel, onNote.Note);

                        if (!onEvents.ContainsKey(onNoteIdentifier))
                        {
                            onEvents[onNoteIdentifier] = new Queue<MidiEventWithTime<OnNoteVoiceMidiEvent>>();
                        }

                        // If the Velocity is 0, we are turning off a note using another OnNote (see https://stackoverflow.com/a/43322203/1984712)
                        // Basically, if a NoteOn event is received with a velocity of 0, we effectively have a NoteOff event.
                        if (onNote.Velocity == 0)
                        {
                            if (onEvents.TryGetValue(onNoteIdentifier, out var midiEventQueue))
                            {
                                NoteSegments.Add(noteSegmentProvider.CreateNoteSegment(
                                    this,
                                    track,
                                    midiEventQueue.Dequeue(), // Get the first matching On Event that matches this identifier
                                    new MidiEventWithTime<OffNoteVoiceMidiEvent>(time, CreateOffNoteFromOnNote(onNote))
                                ));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("OnNote event with Velocity = 0 at [ Channel: {0}; Note: {1} ] is missing a corresponding OnNote event with Velocity > 0.", onNoteIdentifier.Channel, onNoteIdentifier.Note));
                            }
                        }
                        // Otherwise, queue the note so that an OffNote can match to it
                        else
                        {
                            onEvents[onNoteIdentifier].Enqueue(new MidiEventWithTime<OnNoteVoiceMidiEvent>(time, onNote));
                        }
                    }
                    else if (midiEvent is OffNoteVoiceMidiEvent offNote)
                    {
                        var offNoteIdentifer = (offNote.Channel, offNote.Note);

                        if(onEvents.TryGetValue(offNoteIdentifer, out var midiEventQueue))
                        {
                            NoteSegments.Add(noteSegmentProvider.CreateNoteSegment(
                                this,
                                track,
                                midiEventQueue.Dequeue(), // Get the first matching On Event 
                                new MidiEventWithTime<OffNoteVoiceMidiEvent>(time, offNote))
                            );
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("OffNote event at [ Channel: {0}; Note: {1} ] is missing a corresponding OnNote event.", offNoteIdentifer.Channel, offNoteIdentifer.Note));
                        }
                    }
                }

                if (onEvents.Any(e => e.Value.Count > 0))
                    System.Diagnostics.Debug.WriteLine("One or more OnNote events weren't paired with an OffNote event.");
            }

            TotalDurationSamples = NoteSegments.Max(segment => segment.StartSample + segment.DurationSamples);
        }

        private OffNoteVoiceMidiEvent CreateOffNoteFromOnNote(OnNoteVoiceMidiEvent onNote)
        {
            if (onNote.Velocity != 0)
                throw new Exception("The OnNoteVoiceMidiEvent used to create the artificial OffNoteVoiceMidiEvent MUST have a Velocity of 0.");

            return new OffNoteVoiceMidiEvent(onNote.DeltaTime, onNote.Channel, onNote.Note, onNote.Velocity);
        }
    }
}
