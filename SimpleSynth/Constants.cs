namespace SimpleSynth
{
    public static class Constants
    {
        /// <summary>
        /// Samples per second
        /// </summary>
        public const int SAMPLE_RATE = 44100;

        /// <summary>
        /// The number of samples for percussion instruments
        /// </summary>
        public const int PERCUSSION_SAMPLE_COUNT = 19404; // 440 ms. Based on the default duration in the Kicker plugin for LMMS

        /// <summary>
        /// The number of microseconds per second
        /// </summary>
        public const int MICROSECONDS_PER_SECOND = 1_000_000;

        /// <summary>
        /// The number of microseconds per minute
        /// </summary>
        public const int MICROSECONDS_PER_MINUTE = 60_000_000;
    }
}
