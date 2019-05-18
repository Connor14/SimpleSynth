using MidiSharp;
using NWaves.Audio;
using NWaves.Signals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleSynth
{
    public static class MidiSynth
    {
        public static MemoryStream GenerateWAV(Stream midiStream, SignalType[] signalTypes, bool includeAdsr)
        {
            MidiSequence sequence = MidiSequence.Open(midiStream);

            MidiSegmenter segmenter = new MidiSegmenter(sequence);
            List<NoteSegment> allSegments = segmenter.Segments;

            int totalSamples = segmenter.DurationSamples;
            float[] samples = new float[totalSamples];
            foreach (NoteSegment segment in segmenter.Segments)
            {
                // Exclude percussion because it makes the signal dirty
                if (segment.Channel == (byte)SpecialChannel.Percussion)
                {
                    continue;
                }

                long startSample = segment.StartSample;

                DiscreteSignal segmentSignal = segment.GetSignalMix(signalTypes, includeAdsr);
                for (long i = 0; i < segmentSignal.Samples.Length; i++)
                {
                    samples[startSample + i] = (samples[startSample + i] + segmentSignal.Samples[i]) / 2f; // average the data
                }

            }

            DiscreteSignal signal = new DiscreteSignal(44100, samples);

            // Amplify based on the maximum / minimum amplitues so that the new max/min are 1
            float currentMax = signal.Samples.Max();
            float currentMin = signal.Samples.Min();

            float greater = Math.Max(currentMax, Math.Abs(currentMin));
            float multiplier = 1f / greater;

            signal.Amplify(multiplier);

            //var filter = new CombFeedforwardFilter(25);
            //signal = filter.ApplyTo(signal);

            //var frequency = 800/*Hz*/;
            //var notchFilter = new NotchFilter(frequency / signal.SamplingRate);
            //signal = notchFilter.ApplyTo(signal);

            //var maFilter = new MovingAverageFilter();
            //signal = maFilter.ApplyTo(signal);

            MemoryStream output = null;
            using(MemoryStream stream = new MemoryStream())
            {
                var waveFile = new WaveFile(signal, 32);
                waveFile.SaveTo(stream);
                output = new MemoryStream(stream.ToArray());
            }

            return output;
        }
    }
}
