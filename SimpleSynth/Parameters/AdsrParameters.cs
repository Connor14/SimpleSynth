namespace SimpleSynth.Parameters
{
    public class AdsrParameters
    {
        // A good reference: https://blog.landr.com/adsr-envelopes-infographic/

        // Example timings:
        //          attackTime,     decayTime,  releaseTime
        // short:   .01,            .02,        .015
        // medium:  .05,            .1,         .075
        // long:    .1,             .2,         .15
        public static AdsrParameters Short => new AdsrParameters(0.01, 0.02, 1, 0.015, 1.5);
        public static AdsrParameters Default => new AdsrParameters(0.05, 0.1, 1, 0.075, 1.5);
        public static AdsrParameters Long => new AdsrParameters(0.1, 0.2, 1, 0.15, 1.5);

        /// <summary>
        /// The length of the Attack phase in seconds
        /// </summary>
        public double AttackDuration { get; set; }

        /// <summary>
        /// The length of the Decay phase in seconds
        /// </summary>
        public double DecayDuration { get; set; }

        /// <summary>
        /// The fractional length of the Sustain phase relative to the original (full) length of the Sustain phase. 
        /// 
        /// <see cref="GetSustainDurationSeconds(double)"/> for more information.
        /// 
        /// Suggested Minimum of 0 and Maximum of 1 (neither is a hard limit)
        /// </summary>
        public double SustainDurationFraction { get; set; }

        /// <summary>
        /// The length of the Release phase in seconds
        /// </summary>
        public double ReleaseDuration { get; set; }

        /// <summary>
        /// The maximum amplitude that the Attack phase will reach
        /// </summary>
        public double AttackAmplitude { get; set; }

        public AdsrParameters(double attackDuration = .05, double decayDuration = .1, double sustainDurationFraction = 1, double releaseDuration = .075, double attackAmplitude = 1.5)
        {
            AttackDuration = attackDuration;
            DecayDuration = decayDuration;
            SustainDurationFraction = sustainDurationFraction;
            ReleaseDuration = releaseDuration;

            AttackAmplitude = attackAmplitude;
        }

        /// <summary>
        /// Calculate the length of the Sustain phase of the ADSR envelope after taking the durations of Attack, Decay, and Release into account.
        /// Then multiply the length of the sustain by the SustainDurationRatio to make the Sustain phase shorter (or longer, technically) relative to the original Sustain duration
        /// </summary>
        /// <param name="totalDurationSeconds"></param>
        /// <returns></returns>
        public double GetSustainDurationSeconds(double totalDurationSeconds)
        {
            // Calculate the sustain duration
            return (totalDurationSeconds - (AttackDuration + DecayDuration + ReleaseDuration)) * SustainDurationFraction;
        }
    }
}
