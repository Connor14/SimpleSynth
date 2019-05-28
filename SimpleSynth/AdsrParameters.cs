using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth
{
    public class AdsrParameters
    {
        public double AttackTime { get; set; }
        public double DecayTime { get; set; }
        public double ReleaseTime { get; set; }

        // Example timings:
        //          attackTime,     decayTime,  releaseTime
        // short:   .01,            .02,        .015
        // medium:  .05,            .1,         .075
        // long:    .1,             .2,         .15
        public AdsrParameters(double attackTime = .05, double decayTime = .1, double releaseTime = .075)
        {
            AttackTime = attackTime;
            DecayTime = decayTime;
            ReleaseTime = releaseTime;
        }

        public double GetSustainTime(double DurationSeconds)
        {
            return DurationSeconds - (AttackTime + DecayTime + ReleaseTime);
        }

    }
}
