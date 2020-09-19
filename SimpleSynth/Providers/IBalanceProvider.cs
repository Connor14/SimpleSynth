using SimpleSynth.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Provides amplitude balancing information for synthesizer implementations.
    /// </summary>
    public interface IBalanceProvider
    {
        /// <summary>
        /// Returns an amplitude multiplier for the given NoteSegment.
        /// </summary>
        /// <param name="noteSegment"></param>
        /// <returns></returns>
        float GetMultiplier(NoteSegment noteSegment);
    }
}
