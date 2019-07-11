using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;
using NWaves.Audio;
using NWaves.Filters;
using NWaves.Signals;
using SimpleSynth.Notes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSynth.Synths
{
    public class HarmonicSynth : MidiSynth
    {
        private IEnumerable<double> _defaultHarmonics { get; set; }

        // cannot re-assign otherwise it breaks the pass-by-reference
        public ConcurrentDictionary<int, HarmonicParameters> HarmonicParameters { get; private set; } = new ConcurrentDictionary<int, HarmonicParameters>();

        public HarmonicSynth(Stream midiStream, AdsrParameters adsrParameters, IEnumerable<double> defaultHarmonics) : base(midiStream, adsrParameters)
        {
            this._defaultHarmonics = defaultHarmonics;

            this.Segments = GetSegments(); // this MUST be called here and NOT in the base class because HarmonicCount and AllHarmonics need to be initialized
        }

        public HarmonicSynth(Stream midiStream, AdsrParameters adsrParameters, int defaultHarmonicCount = 10)
            : this(midiStream, adsrParameters, Enumerable.Range(1, defaultHarmonicCount).Select(x => (double)x))
        {
        }

        // implementation is used by the base class
        protected override List<NoteSegment> GetSegments()
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

                        // add new harmonic channel if it doesn't exist.
                        if(midiEventType.IsSubclassOf(typeof(VoiceMidiEvent)))
                        {
                            VoiceMidiEvent e = (VoiceMidiEvent)midiEvent;

                            if (!HarmonicParameters.ContainsKey(e.Channel))
                            {
                                // use ToList to create a NEW list because every HarmonicChannel is referring to _defaultHarmonics.
                                // If we don't make a NEW list, then if we edit the harmonics of one channel, they all change
                                HarmonicParameters[e.Channel] = new HarmonicParameters(e.Channel, _defaultHarmonics.ToList());
                            }
                        }

                        if (midiEventType == typeof(OnNoteVoiceMidiEvent))
                        {
                            OnNoteVoiceMidiEvent e = (OnNoteVoiceMidiEvent)midiEvent;
                            segments.Add(new HarmonicNote(this, e.Channel, e.Note, currentTick));
                        }
                        else if (midiEventType == typeof(OffNoteVoiceMidiEvent))
                        {
                            OffNoteVoiceMidiEvent e = (OffNoteVoiceMidiEvent)midiEvent;
                            segments.Where(s => !s.Complete && s.Channel == e.Channel && s.Note == e.Note).First().SetEndingTick(currentTick);
                        }
                        else if (midiEventType == typeof(TempoMetaMidiEvent)) // this may not necessarily come before every OnNoteVoice event
                        {
                            TempoMetaMidiEvent e = (TempoMetaMidiEvent)midiEvent;

                            // use the first tempo marking we find
                            if (this.TempoMicroSecondsPerBeat == -1)
                            {
                                this.TempoMicroSecondsPerBeat = e.Value;
                            }
                        }
                        else if (midiEventType == typeof(ProgramChangeVoiceMidiEvent))
                        {
                            ProgramChangeVoiceMidiEvent e = (ProgramChangeVoiceMidiEvent)midiEvent;

                            HarmonicParameters[e.Channel].SetInstrument(e.Number);
                        }
                    }

                }

                // Theoretically, the last event is a NoteOff event, so it has no duration. Therefore, our currentTick count is our total stick count
                if (currentTick > TickCount)
                {
                    this.TickCount = currentTick;
                }

                // now we have all of our segments for the track. These should all be completed (Complete == true)
                allSegments.AddRange(segments);
            }

            return allSegments;
        }
    }
}
