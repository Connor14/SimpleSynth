using NWaves.Signals;
using System;
using System.Linq;

namespace SimpleSynth.Extensions
{
    public static class DiscreteSignalExtensions
    {
        /// <summary>
        /// Averages two signals together.
        /// </summary>
        /// <param name="mainSignal"></param>
        /// <param name="signal"></param>
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

        /// <summary>
        /// Subtracts the "signal" amplitudes from the "mainSignal" amplitudes
        /// </summary>
        /// <param name="mainSignal"></param>
        /// <param name="signal"></param>
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

        /// <summary>
        /// Applies an ADSR envelope to mainSignal
        /// </summary>
        /// <param name="mainSignal"></param>
        /// <param name="adsrSignal"></param>
        public static void ApplyAdsr(this DiscreteSignal mainSignal, DiscreteSignal adsrSignal)
        {
            if (mainSignal.Samples.Length != adsrSignal.Samples.Length)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < mainSignal.Samples.Length; i++)
            {
                mainSignal[i] = mainSignal.Samples[i] * adsrSignal.Samples[i];
            }
        }

        // Make the maximum amplitudes equal to the given maximum amplitude
        // Theoretically, float values can range from [-1, 1] though at one point (for some reason) I was under the impressing that 1 would clip to 0
        // I think that was according to this: https://stackoverflow.com/questions/12112945/audio-units-samples-value-range
        public static void ScaleAmplitude(this DiscreteSignal mainSignal, float maximumAmplitude = 1f)
        {
            if (maximumAmplitude < -1 || maximumAmplitude > 1)
            {
                throw new ArgumentOutOfRangeException("Amplitude must be in range [-1f, 1f].");
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

            // Check the new minimums and maximums. This might reveal issues with the floating point math
            float postAmplifyMax = mainSignal.Samples.Max();
            float postAmplifyMin = mainSignal.Samples.Min();

            if(postAmplifyMin < -1 || postAmplifyMax > 1)
            {
                throw new Exception(
                    string.Format(
                        "Maximum or minimum amplitude after amplification was not in the valid range of [-1, 1]. Maximum amplitude: {0}; Minimum amplitude: {1}", 
                        postAmplifyMax, 
                        postAmplifyMin
                    )
                );
            }
        }
    }
}
