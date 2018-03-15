using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    public  class Equalizer
    {
        
        private long _instance; // Used from JNI

        private Equalizer()
        {
            NativeNew();
        }

        private Equalizer(int index)
        {
            NativeNewFromPreset(index);
        }

        ~Equalizer()
        {
            NativeRelease();
        }

        /// <summary>
        /// Create a new default equalizer, with all frequency values zeroed.
        /// The new equalizer can subsequently be applied to a media player by invoking
        /// <seealso cref="MediaPlayer#setEqualizer"/>.
        /// </summary>
        public static Equalizer Create()
        {
            return new Equalizer();
        }

        /// <summary>
        /// Create a new equalizer, with initial frequency values copied from an existing
        /// preset.
        /// The new equalizer can subsequently be applied to a media player by invoking
        /// <seealso cref="MediaPlayer#setEqualizer"/>.
        /// </summary>
        public static Equalizer CreateFromPreset(int index)
        {
            return new Equalizer(index);
        }
        /// <summary>
        /// Get the number of equalizer presets.
        /// </summary>
        public static int PresetCount => NativeGetPresetCount();

        /// <summary>
        /// Get the name of a particular equalizer preset.
        /// This name can be used, for example, to prepare a preset label or menu in a user
        /// interface.
        /// </summary>
        /// <param name="index"> index of the preset, counting from zero. </param>
        /// <returns> preset name, or NULL if there is no such preset </returns>

        public static string GetPresetName(int index)
        {
            return NativeGetPresetName(index);
        }

        /// <summary>
        /// Get the number of distinct frequency bands for an equalizer.
        /// </summary>
        public static int BandCount => NativeGetBandCount();

        /// <summary>
        /// Get a particular equalizer band frequency.
        /// This value can be used, for example, to create a label for an equalizer band control
        /// in a user interface.
        /// </summary>
        /// <param name="index"> index of the band, counting from zero. </param>
        /// <returns> equalizer band frequency (Hz), or -1 if there is no such band </returns>
        public static float GetBandFrequency(int index)
        {
            return NativeGetBandFrequency(index);
        }

        /// <summary>
        /// Get the current pre-amplification value from an equalizer.
        /// </summary>
        /// <returns> preamp value (Hz) </returns>
        public virtual float PreAmp => NativeGetPreAmp();

        /// <summary>
        /// Set a new pre-amplification value for an equalizer.
        /// The new equalizer settings are subsequently applied to a media player by invoking
        /// <seealso cref="MediaPlayer#setEqualizer"/>.
        /// The supplied amplification value will be clamped to the -20.0 to +20.0 range.
        /// </summary>
        /// <param name="preamp"> value (-20.0 to 20.0 Hz) </param>
        /// <returns> true on success. </returns>
        public virtual bool SetPreAmp(float preamp)
        {
            return NativeSetPreAmp(preamp);
        }

        /// <summary>
        /// Get the amplification value for a particular equalizer frequency band.
        /// </summary>
        /// <param name="index"> counting from zero, of the frequency band to get. </param>
        /// <returns> amplification value (Hz); NaN if there is no such frequency band. </returns>
        public virtual float GetAmp(int index)
        {
            return NativeGetAmp(index);
        }

        /// <summary>
        /// Set a new amplification value for a particular equalizer frequency band.
        /// The new equalizer settings are subsequently applied to a media player by invoking
        /// <seealso cref="MediaPlayer#setEqualizer"/>.
        /// The supplied amplification value will be clamped to the -20.0 to +20.0 range.
        /// </summary>
        /// <param name="index"> counting from zero, of the frequency band to set. </param>
        /// <param name="amp"> amplification value (-20.0 to 20.0 Hz).
        /// \return true on success. </param>
        public virtual bool SetAmp(int index, float amp)
        {
            return NativeSetAmp(index, amp);
        }

        [DllImport("unknown")]
        private static extern int NativeGetPresetCount();
        [DllImport("unknown")]
        private static extern string NativeGetPresetName(int index);
        [DllImport("unknown")]
        private static extern int NativeGetBandCount();
        [DllImport("unknown")]
        private static extern float NativeGetBandFrequency(int index);
        [DllImport("unknown")]
        private extern void NativeNew();
        [DllImport("unknown")]
        private extern void NativeNewFromPreset(int index);
        [DllImport("unknown")]
        private extern void NativeRelease();
        [DllImport("unknown")]
        private extern float NativeGetPreAmp();
        [DllImport("unknown")]
        private extern bool NativeSetPreAmp(float preamp);
        [DllImport("unknown")]
        private extern float NativeGetAmp(int index);
        [DllImport("unknown")]
        private extern bool NativeSetAmp(int index, float amp);

    }
}