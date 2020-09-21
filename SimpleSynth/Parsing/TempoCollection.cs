using MidiSharp.Events.Meta;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSynth.Parsing
{
    public class TempoCollection
    {
        public int TicksPerBeatOrFrame { get; } // ticks per beat. Comes from MIDI file.

        // Key represents the tick where the tempo event was found (time)
        public SortedList<long, TempoMetaMidiEvent> TempoEvents { get; } = new SortedList<long, TempoMetaMidiEvent>();

        public TempoCollection(int ticksPerBeatOrFrame)
        {
            TicksPerBeatOrFrame = ticksPerBeatOrFrame;
        }

        public void AddTempoEvent(long time, TempoMetaMidiEvent tempoEvent)
        {
            if (TempoEvents.ContainsKey(time))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("TempoEvent at time {0} already exists.", time));
                return;
            }

            TempoEvents.Add(time, tempoEvent);
        }

        /// <summary>
        /// Gets the number of microseconds / tick based on the current tempo at the provided time 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public double GetMicrosecondsPerTickForTime(long time)
        {
            // Get the LAST tempo event that comes at or before the time (in ticks)
            var tempoEventForOnNote = TempoEvents.Last(tempoEvent => tempoEvent.Key <= time).Value;

            return GetMicrosecondsPerTick(tempoEventForOnNote);
        }

        private double GetMicrosecondsPerTick(TempoMetaMidiEvent tempoEvent)
        {
            int microsecondsPerBeat = tempoEvent.Value; // microseconds / beat

            return (double)microsecondsPerBeat / TicksPerBeatOrFrame; // returns microseconds / tick
        }

        /// <summary>
        /// Gets the number of samples that have elapsed up until the provided time while also taking tempo changes into account.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int GetTotalElapsedSamplesForTime(long time)
        {
            if (time == 0) return 0;

            int elapsedSamples = 0;

            // Applicable tempo events in reverse order
            var applicableTempoEvents = TempoEvents.Where(tempoEvent => tempoEvent.Key <= time).Reverse().ToList();

            foreach (var tempoEventPair in applicableTempoEvents)
            {
                // Calculate the amount of time the note was in the tempo zone
                long timeInTempoZone = time - tempoEventPair.Key; // time - time of tempo event

                double microsecondsPerTick = GetMicrosecondsPerTick(tempoEventPair.Value);

                // Calculated the number of samples that the note spent in this tempo zone
                elapsedSamples += Conversions.ConvertTicksToSamples(microsecondsPerTick, timeInTempoZone);

                // Reduce my time by the amount of time I was in the tempo zone so that I take off that "chunk" of the timeline
                time -= timeInTempoZone;
            }

            return elapsedSamples;
        }
    }
}
