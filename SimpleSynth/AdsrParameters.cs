using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSynth
{
    public class AdsrParameters
    {
        // Example timings:
        //          attackTime,     decayTime,  releaseTime
        // short:   .01,            .02,        .015
        // medium:  .05,            .1,         .075
        // long:    .1,             .2,         .15
        public static AdsrParameters Short = new AdsrParameters(0.01, 0.02, 0.015);
        public static AdsrParameters Default = new AdsrParameters(0.05, 0.1, 0.075);
        public static AdsrParameters Long = new AdsrParameters(0.1, 0.2, 0.15);

        public double AttackTime { get; set; }
        public double DecayTime { get; set; }
        public double ReleaseTime { get; set; }

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
