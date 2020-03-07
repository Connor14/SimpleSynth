using NWaves.Signals;
using System;
using System.Linq;

namespace SimpleSynth.Extensions
{
    public static class DiscreteSignalExtensions
    {
        // Average two signals together
        public static void CombineAverage(this DiscreteSignal mainSignal, DiscreteSignal signal)
        {
            if (mainSignal.Samples.Length != signal.Samples.Length)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < mainSignal.Samples.Length; i++)
            {
                mainSignal.Samples[i] = (mainSignal.Samples[i] + signal.Samples[i]) / 2f;
            }
        }

        /// <summary>
        /// Add the amplitudes of each signal together.
        /// 
        /// Similar to Superimpose but doesn't create a copy of a signal
        /// </summary>
        /// <param name="mainSignal"></param>
        /// <param name="signal"></param>
        public static void CombineAdd(this DiscreteSignal mainSignal, DiscreteSignal signal)
        {
            if (mainSignal.Samples.Length != signal.Samples.Length)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < mainSignal.Samples.Length; i++)
            {
                mainSignal.Samples[i] = mainSignal.Samples[i] + signal.Samples[i];
            }
        }

        // Subtract the "signal" amplitudes from the "mainSignal" amplitudes
        public static void CombineSubtract(this DiscreteSignal mainSignal, DiscreteSignal signal)
        {
            if (mainSignal.Samples.Length != signal.Samples.Length)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < mainSignal.Samples.Length; i++)
            {
                mainSignal.Samples[i] = mainSignal.Samples[i] - signal.Samples[i];
            }
        }

        // Make the maximum amplitudes equal to the given maximum amplitude
        // todo: is Normalize the correct terminology?

        // According to https://stackoverflow.com/questions/12112945/audio-units-samples-value-range
        // float values can range from [-1, 1). 1 will clip to 0.
        public static void NormalizeAmplitude(this DiscreteSignal mainSignal, float maximumAmplitude = .9f)
        {
            if(maximumAmplitude < -1 || maximumAmplitude >= 1)
            {
                throw new ArgumentException("Amplitude must be in range [-1f, 1f)");
            }

            if (mainSignal.Samples.Length == 0)
            {
                throw new ArgumentException("Signal has no samples.");
            }

            // Amplify based on the maximum / minimum amplitues so that the new max/min are 1
            float currentMax = mainSignal.Samples.Max();
            float currentMin = mainSignal.Samples.Min();

            float greater = Math.Max(currentMax, Math.Abs(currentMin));
            float multiplier = maximumAmplitude / greater;

            mainSignal.Amplify(multiplier);
        }
    }
}
