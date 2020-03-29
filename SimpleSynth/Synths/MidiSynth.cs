using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice.Note;
using NWaves.Audio;
using NWaves.Signals;
using SimpleSynth.EventArguments;
using SimpleSynth.Extensions;
using SimpleSynth.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSynth.Synths
{
    public abstract class MidiSynth
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public MidiSequence MidiFile { get; private set; }
        public double MicrosecondsPerTick { get; private set; }
        public int TotalDurationSamples { get; private set; }
        public List<NoteSegment> NoteSegments { get; private set; }

        public MidiSynth(Stream midiStream)
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

                    if(midiEvent is OnNoteVoiceMidiEvent onNote)
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

        public async Task<MemoryStream> GenerateWAVAsync()
        {
            return await Task.Run(() => { return GenerateWAV(); });
        }

        public MemoryStream GenerateWAV()
        {
            stopwatch.Restart();

            // unique NoteSegments
            Dictionary<Tuple<int, byte, int, byte>, NoteSegment> noteSegmentsToRender = new Dictionary<Tuple<int, byte, int, byte>, NoteSegment>();

            // find unique note segments
            foreach (var segment in NoteSegments)
            {
                // If the dictionary does not contain the segment's identifierr, add the segment
                if (!noteSegmentsToRender.ContainsKey(segment.Identifier))
                {
                    noteSegmentsToRender[segment.Identifier] = segment; // cache the first of a repeated signal
                }
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(1, 5, "Duplicate note consolidation", stopwatch.Elapsed));
            stopwatch.Restart();

            int notesRendered = 0;
            int totalToRender = noteSegmentsToRender.Count;

            // unique DiscreteSignals
            ConcurrentDictionary<Tuple<int, byte, int, byte>, DiscreteSignal> signalCache = new ConcurrentDictionary<Tuple<int, byte, int, byte>, DiscreteSignal>();

            // generate unique signals in parallel
            Parallel.ForEach(noteSegmentsToRender, segment =>
            {
                signalCache[segment.Value.Identifier] = Render(segment.Value);

                Interlocked.Increment(ref notesRendered);
                OnProgressChanged(new NoteRenderedEventArguments(notesRendered, totalToRender, "Note rendered in parallel"));
            });

            OnProgressChanged(new GenerationProgressChangedEventArgs(2, 5, "Unique note rendering", stopwatch.Elapsed));
            stopwatch.Restart();

            float[] samples = new float[TotalDurationSamples];

            // assemble the final wav
            foreach (NoteSegment segment in NoteSegments)
            {
                DiscreteSignal segmentSignal = signalCache[segment.Identifier];

                long startSample = segment.StartSample;

                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    samples[startSample + i] = samples[startSample + i] + segmentSignal.Samples[i]; // add the samples together
                }
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(3, 5, "Wave assembly", stopwatch.Elapsed));
            stopwatch.Restart();

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            signal.ScaleAmplitude(1f); // adjust the samples to fit between [-1, 1]

            OnProgressChanged(new GenerationProgressChangedEventArgs(4, 5, "Wave normalization", stopwatch.Elapsed));
            stopwatch.Restart();

            MemoryStream output;
            using (MemoryStream stream = new MemoryStream())
            {
                var waveFile = new WaveFile(signal, 32);
                waveFile.SaveTo(stream);
                output = new MemoryStream(stream.ToArray());
            }

            OnProgressChanged(new GenerationProgressChangedEventArgs(5, 5, "MemoryStream writing", stopwatch.Elapsed));
            stopwatch.Stop();

            return output;
        }

        /// <summary>
        /// The logic to turn NoteSegments into DiscreteSignals
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        protected abstract DiscreteSignal Render(NoteSegment segment);

        public event EventHandler ProgressChanged;

        protected virtual void OnProgressChanged(EventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
