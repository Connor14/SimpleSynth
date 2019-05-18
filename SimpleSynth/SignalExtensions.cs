using NWaves.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSynth
{
    public static class SignalExtensions
    {
        // Average two signals together
        public static void Combine(this DiscreteSignal mainSignal, DiscreteSignal signal)
        {
            if (mainSignal.Samples.Length != signal.Samples.Length)
            {
                throw new Exception();
            }

            for (int i = 0; i < mainSignal.Samples.Length; i++)
            {
                mainSignal.Samples[i] = (mainSignal.Samples[i] + signal.Samples[i]) / 2f;
            }
        }
    }
}
