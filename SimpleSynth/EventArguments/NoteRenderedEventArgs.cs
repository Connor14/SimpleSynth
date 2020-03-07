using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SimpleSynth.EventArguments
{
    public class NoteRenderedEventArguments : ProgressChangedEventArgs
    {
        public int Number { get; set; }
        public int Total { get; set; }
        public string Description { get; set; }

        public NoteRenderedEventArguments(int number, int total, string description) : base(number * 100 / total, null)
        {
            Number = number;
            Total = total;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} notes rendered", Number, Total, Description);
        }
    }
}
