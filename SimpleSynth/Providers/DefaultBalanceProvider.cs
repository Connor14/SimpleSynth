using SimpleSynth.Parsing;
using SimpleSynth.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth.Providers
{
    /// <summary>
    /// Provides amplitude balancing information for synthesizer implementations.
    /// </summary>
    public class DefaultBalanceProvider : IBalanceProvider
    {
        public const float DefaultMultiplier = 1.0f;

        // Frequency, Amplitude Multiplier
        // Should be sorted in lowest to highest frequency order
        // Examples:
        // frequency <= 160 => 2.0f
        // frequency <= 2500 => 1.0f
        // else 0.5f
        public IReadOnlyDictionary<double, float> FrequencyLevels { get; }

        public IReadOnlyDictionary<PercussionType, float> PercussionLevels { get; }

        public DefaultBalanceProvider(SortedList<double, float> frequencyLevels = null, Dictionary<PercussionType, float> percussionLevels = null)
        {
            FrequencyLevels = frequencyLevels ?? new SortedList<double, float>();
            PercussionLevels = percussionLevels ?? new Dictionary<PercussionType, float>()
            {
                { PercussionType.Bass, 5.0f },
                { PercussionType.Snare, 3.0f },
                { PercussionType.Crash, 3.0f },
                { PercussionType.Ride, 3.0f },
            };
        }

        /// <inheritdoc />
        public float GetMultiplier(NoteSegment noteSegment)
        {
            if (noteSegment is PercussionNoteSegment percussionNoteSegment)
            {
                return GetPercussionMultiplier(percussionNoteSegment.PercussionType);
            }
            else if (noteSegment is MelodicNoteSegment melodicNoteSegment)
            {
                return GetFrequencyMultiplier(melodicNoteSegment.Frequency);
            }

            return DefaultMultiplier; // Default to a "non-modified" multiplier
        }

        private float GetFrequencyMultiplier(double frequency)
        {
            if (FrequencyLevels is null || FrequencyLevels.Count == 0)
                return DefaultMultiplier;

            foreach (var thresholdLevel in FrequencyLevels)
            {
                if (frequency <= thresholdLevel.Key)
                    return thresholdLevel.Value;
            }

            return DefaultMultiplier; // Default to a "non-modified" multiplier
        }

        private float GetPercussionMultiplier(PercussionType percussionType)
        {
            if (PercussionLevels is null || PercussionLevels.Count == 0)
                return DefaultMultiplier;

            if (PercussionLevels.TryGetValue(percussionType, out float multiplier))
                return multiplier;

            return DefaultMultiplier; // Default to a "non-modified" multiplier
        }
    }
}
