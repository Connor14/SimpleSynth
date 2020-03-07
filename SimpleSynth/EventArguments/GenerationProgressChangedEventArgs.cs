using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SimpleSynth.EventArguments
{
    public class GenerationProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public int Step { get; set; }
        public int TotalSteps { get; set; }

        public string Description { get; set; }
        public TimeSpan Duration { get; set; }

        public GenerationProgressChangedEventArgs(int step, int totalSteps, string description, TimeSpan duration) : base(step * 100 / totalSteps, null)
        {
            Step = step;
            TotalSteps = totalSteps;
            Description = description;
            Duration = duration;
        }

        public override string ToString()
        {
            return string.Format("Step {0}/{1} completed in {2}: {3}", Step, TotalSteps, Duration, Description);
        }
    }
}
