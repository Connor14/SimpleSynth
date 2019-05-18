using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSynth
{
    public class MidiSegmenter
    {
        public MidiSequence Sequence;

        // the total number of ticks in this sequence
        private long _tickCount;
        public long TickCount
        {
            get
            {
                return _tickCount;
            }
        }

        // the duration of the MIDI in seconds
        public double DurationSeconds
        {
            get
            {
                int ticksPerBeat = Sequence.Division; // 192 ticks/beat
                int microsecondsPerBeat = TempoMicroSecondsPerBeat; // 600000 uS / beat

                double microsecondsPerTick = (double)microsecondsPerBeat / ticksPerBeat;
                long totalMicroseconds = (long)(TickCount * microsecondsPerTick);

                return (double)totalMicroseconds / 1000000;
            }
        }

        // the duratioon of the MIDI in samples
        public int DurationSamples
        {
            get
            {
                return (int)(SynthConsts.SampleRate * DurationSeconds);
            }
        }


        private const int microsecondsPerMinute = 60000000; // the number of microseconds per minute (just a useful number)
        private int _tempo = -1; // the MIDI tempo in microseconds / quarter note

        // same as _tempo
        public int TempoMicroSecondsPerBeat
        {
            get
            {
                return _tempo;
            }
        }

        // The Tempo in BPM
        public int TempoBeatsPerMinute
        {
            get
            {
                return microsecondsPerMinute / _tempo;
            }
        }

        // All of the note data including their start times, durations, etc
        private List<NoteSegment> _segments;
        public List<NoteSegment> Segments
        {
            get
            {
                return _segments;
            }
        }

        public MidiSegmenter(MidiSequence sequence)
        {
            this.Sequence = sequence;

            _segments = GetSegments();
        }

        private List<NoteSegment> GetSegments()
        {
            List<NoteSegment> allSegments = new List<NoteSegment>();

            // find all segments for each track
            // apparently Abundant Music MIDIs only have 1 track. It has separate channels.
            foreach (MidiTrack track in Sequence.Tracks)
            {
                if (track.Events.Count == 0)
                {
                    continue;
                }

                Queue<MidiEvent> eventQueue = new Queue<MidiEvent>(track.Events.ToList()); // the events in sequential order
                List<NoteSegment> segments = new List<NoteSegment>(); // the segments for this track

                long currentTick = 0; // the current position in the track
                while (eventQueue.Count > 0)
                {
                    List<MidiEvent> concurrentEvents = new List<MidiEvent>();

                    MidiEvent e1 = eventQueue.Dequeue(); // grab the next event that has a non-zero delta time (unless it's the first event, of course)
                    currentTick += e1.DeltaTime; // adjust the current time
                    concurrentEvents.Add(e1); // add the event to the list

                    // while the DeltaTimes are 0 (meaning these are concurrent events), add them to our concurrent list.
                    while (eventQueue.Count > 0 && eventQueue.Peek().DeltaTime == 0)
                    {
                        concurrentEvents.Add(eventQueue.Dequeue());
                    }

                    // we have all of the concurrent events for this tick
                    // generate or complete the segments for each concurrent event
                    foreach (MidiEvent midiEvent in concurrentEvents)
                    {
                        Type midiEventType = midiEvent.GetType();

                        if (midiEventType == typeof(OnNoteVoiceMidiEvent))
                        {
                            OnNoteVoiceMidiEvent e = (OnNoteVoiceMidiEvent)midiEvent;
                            segments.Add(new NoteSegment(this, e.Channel, e.Note, currentTick));
                        }
                        else if (midiEventType == typeof(OffNoteVoiceMidiEvent))
                        {
                            OffNoteVoiceMidiEvent e = (OffNoteVoiceMidiEvent)midiEvent;
                            segments.Where(s => !s.Complete && s.Channel == e.Channel && s.Note == e.Note).First().SetEndingTick(currentTick);
                        }
                        else if (midiEventType == typeof(TempoMetaMidiEvent))
                        {
                            TempoMetaMidiEvent e = (TempoMetaMidiEvent)midiEvent;

                            // use the first tempo marking we find
                            if (_tempo == -1)
                            {
                                _tempo = e.Value;
                            }
                        }
                    }

                }

                // Theoretically, the last event is a NoteOff event, so it has no duration. Therefore, our currentTick count is our total stick count
                if (currentTick > TickCount)
                {
                    _tickCount = currentTick;
                }

                // now we have all of our segments for the track. These should all be completed (Complete == true)
                allSegments.AddRange(segments);
            }

            return allSegments;
        }

    }
}
