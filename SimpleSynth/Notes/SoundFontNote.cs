using NWaves.Signals;
using SimpleSynth.Synths;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NAudio.SoundFont;
using NWaves.Effects;

namespace SimpleSynth.Notes
{
    public class SoundFontNote : NoteSegment
    {
        public SoundFontNote(SoundFontSynth synth, byte channel, byte note, long startTick) : base(synth, channel, note, startTick)
        {

        }

        public override DiscreteSignal GetSignalMix()
        {
            SoundFontSynth soundFontSynth = (SoundFontSynth)Synth;
            SampleHeader sampleHeader = soundFontSynth.SoundFontParameters[this.Channel].SampleHeader;

            double soundFontFrequency = SynthUtils.NoteToFrequency(sampleHeader.OriginalPitch);
            double frequency = SynthUtils.NoteToFrequency(this.Note);

            var pitchShift = new PitchShiftEffect(frequency / soundFontFrequency);

            //https://stackoverflow.com/questions/4635769/how-do-i-convert-an-array-of-floats-to-a-byte-and-back
            // create a second float array and copy the bytes into it...
            int count = (int)(sampleHeader.EndLoop - sampleHeader.StartLoop);

            // a dirty way to make sure count is divisible by 4
            while(count % 4 != 0)
            {
                count--;
            }

            float[] sample = new float[count / 4];
            Buffer.BlockCopy(soundFontSynth.SoundFont.SampleData, (int)sampleHeader.StartLoop, sample, 0, count);

            float[] audio = new float[DurationSamples];

            int sampleIndex = 0;
            for (int i = 0; i < audio.Length; i++)
            {
                if (sampleIndex >= sample.Length)
                {
                    sampleIndex = 0;
                }

                audio[i] = sample[sampleIndex];

                sampleIndex++;
            }

            DiscreteSignal mainSignal = new DiscreteSignal((int)sampleHeader.SampleRate, audio);

            // nothing special about .9f. Just needs to be less than 1
            mainSignal.NormalizeAmplitude(.9f); // might help make a nicer ADSR effect because we don't normalize amplitudes AFTER ADSR until the final WAV is made. Just a theory.

            //if (Synth.AdsrParameters != null)
            //{
            //    DiscreteSignal adsr = GetAdsrEnvelope(frequency, Synth.AdsrParameters);

            //    for (int i = 0; i < mainSignal.Samples.Length; i++)
            //    {
            //        mainSignal[i] = mainSignal.Samples[i] * adsr.Samples[i] * .5f;
            //    }
            //}

            return mainSignal;
        }
    }
}
