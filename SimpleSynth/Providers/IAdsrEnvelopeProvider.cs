using NWaves.Signals;
using SimpleSynth.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Provides ADSR envelopes for synthesizer implementations.
    /// </summary>
    public interface IAdsrEnvelopeProvider
    {
        /// <summary>
        /// Creates an ADSR envelope based on the given NoteSegment.
        /// </summary>
        /// <param name="noteSegment"></param>
        /// <returns></returns>
        DiscreteSignal CreateEnvelope(NoteSegment noteSegment);
    }
}
