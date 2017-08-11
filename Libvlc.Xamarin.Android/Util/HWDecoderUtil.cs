using Java.Lang;
using Java.Util;

namespace Libvlc.Xamarin.Android.Util
{
    public class HwDecoderUtil
    {
        public enum AudioOutput
        {
            Opensles,
            Audiotrack,
            All
        }

        public enum Decoder
        {
            Unknown,
            None,
            OMX,
            MediaCodec,
            All
        }

        /// <summary>
        ///     FIXME: Theses cpu crash in MediaCodec. We need to get hands on these devices in order to debug it.
        /// </summary>
        private static readonly DecoderBySOC[] BlacklistedDecoderBySOCList =
        {
            new DecoderBySOC("ro.product.board", "MSM8225", Decoder.None), //Samsung Galaxy Core
            new DecoderBySOC("ro.product.board", "hawaii", Decoder.None) // Samsung Galaxy Ace 4
        };

        private static readonly DecoderBySOC[] DecoderBySOCList =
        {
            /*
             *  Put first devices you want to blacklist
             *  because theses devices can match the next rules.
             */
            new DecoderBySOC("ro.product.brand", "SEMC", Decoder.None), // Xperia S
            new DecoderBySOC("ro.board.platform", "msm7627", Decoder.None), // QCOM S1

            /*
             * Even if omap, old Amazon devices don't work with OMX, so either use MediaCodec or SW.
             */
            new DecoderBySOC("ro.product.brand", "Amazon", Decoder.MediaCodec),

            /*
             * Devices working on OMX
             */
            new DecoderBySOC("ro.board.platform", "omap3", Decoder.OMX), // Omap 3
            new DecoderBySOC("ro.board.platform", "rockchip", Decoder.OMX), // Rockchip RK29
            new DecoderBySOC("ro.board.platform", "rk29", Decoder.OMX), // Rockchip RK29
            new DecoderBySOC("ro.board.platform", "msm7630", Decoder.OMX), // QCOM S2
            new DecoderBySOC("ro.board.platform", "s5pc", Decoder.OMX), // Exynos 3
            new DecoderBySOC("ro.board.platform", "montblanc", Decoder.OMX), // Montblanc
            new DecoderBySOC("ro.board.platform", "exdroid", Decoder.OMX), // Allwinner A31
            new DecoderBySOC("ro.board.platform", "sun6i", Decoder.OMX), // Allwinner A31

            /*
             * Devices working only on MediaCodec
             */
            new DecoderBySOC("ro.board.platform", "exynos4", Decoder.MediaCodec), // Exynos 4 (Samsung Galaxy S2/S3)

            /*
             * Devices working on MediaCodec and OMX
             */
            new DecoderBySOC("ro.board.platform", "omap4", Decoder.All), // Omap 4
            new DecoderBySOC("ro.board.platform", "tegra", Decoder.All), // Tegra 2 & 3
            new DecoderBySOC("ro.board.platform", "tegra3", Decoder.All), // Tegra 3
            new DecoderBySOC("ro.board.platform", "msm8660", Decoder.All), // QCOM S3
            new DecoderBySOC("ro.board.platform", "exynos5", Decoder.All), // Exynos 5 (Samsung Galaxy S4)
            new DecoderBySOC("ro.board.platform", "rk30", Decoder.All), // Rockchip RK30
            new DecoderBySOC("ro.board.platform", "rk31", Decoder.All), // Rockchip RK31
            new DecoderBySOC("ro.board.platform", "mv88de3100", Decoder.All), // Marvell ARMADA 1500

            new DecoderBySOC("ro.hardware", "mt83", Decoder.All) //MTK
        };

        private static readonly AudioOutputBySOC[] AudioOutputBySOCList =
        {
            /* getPlaybackHeadPosition returns an invalid position on Fire OS,
             * thus Audiotrack is not usable */
            new AudioOutputBySOC("ro.product.brand", "Amazon", AudioOutput.Opensles),
            new AudioOutputBySOC("ro.product.manufacturer", "Amazon", AudioOutput.Opensles)
        };

        private static readonly HashMap SystemPropertyMap = new HashMap();


        /// <summary>
        ///     The hardware decoder known to work for the running device.
        ///     (Always return Dec.ALL after Android 4.3)
        /// </summary>
        /// <returns></returns>
        public static Decoder GetDecoderFromDevice()
        {
            /*
             * Try first blacklisted decoders (for all android versions)
             */
            foreach (var decoderBySOC in BlacklistedDecoderBySOCList)
            {
                var prop = GetSystemPropertyCached(decoderBySOC.Key);
                if (string.IsNullOrEmpty(prop)) continue;
                if (prop.Contains(decoderBySOC.Value))
                    return decoderBySOC.Decoder;
            }
            /*
             * Always try MediaCodec after JellyBean MR2,
             * Try OMX or MediaCodec after HoneyComb depending on device properties.
             * Otherwise, use software decoder by default.
             */
            if (AndroidUtil.IsHoneycombMr2OrLater)
                return Decoder.All;
            if (AndroidUtil.IsHoneycombOrLater)
                foreach (var decoderBySOC in DecoderBySOCList)
                {
                    var prop = GetSystemPropertyCached(decoderBySOC.Key);
                    if (string.IsNullOrEmpty(prop)) continue;
                    if (prop.Contains(decoderBySOC.Value))
                        return decoderBySOC.Decoder;
                }
            return Decoder.Unknown;
        }

        /// <summary>
        ///     Return the audio output known to work for the running device
        ///     (By default, returns ALL, i.e AudioTrack + OpenSles)
        /// </summary>
        /// <returns></returns>
        public static AudioOutput GetAudioOutputFromDevice()
        {
            foreach (var audioOutputBySOC in AudioOutputBySOCList)
            {
                var prop = GetSystemPropertyCached(audioOutputBySOC.Key);
                if (string.IsNullOrEmpty(prop)) continue;
                if (prop.Contains(audioOutputBySOC.Value))
                    return audioOutputBySOC.AudioOutput;
            }
            return AudioOutput.All;
        }

        private static string GetSystemPropertyCached(string key)
        {
            var prop = SystemPropertyMap.Get(key).ToString();
            if (!string.IsNullOrEmpty(prop)) return prop;
            prop = GetSystemProperty(key, "none");
            SystemPropertyMap.Put(key, prop);
            return prop;
        }

        private static string GetSystemProperty(string key, string def)
        {
            try
            {
                var cl = ClassLoader.SystemClassLoader;
                var systemProperties = cl.LoadClass("android.os.SystemProperties");
                var paramTypes = new[] {Class.FromType(typeof(string)), Class.FromType(typeof(string))};
                var get = systemProperties.GetMethod("get", paramTypes);
                var parameters = new Object[] {key, def};
                return get.Invoke(systemProperties, parameters).ToString();
            }
            catch (Exception e)
            {
                return def;
            }
        }

        private class DecoderBySOC
        {
            public readonly Decoder Decoder;
            public readonly string Key;
            public readonly string Value;

            public DecoderBySOC(string key, string value, Decoder decoder)
            {
                Key = key;
                Value = value;
                Decoder = decoder;
            }
        }


        private class AudioOutputBySOC
        {
            public readonly AudioOutput AudioOutput;
            public readonly string Key;
            public readonly string Value;

            public AudioOutputBySOC(string key, string value, AudioOutput audioOutput)
            {
                Key = key;
                Value = value;
                AudioOutput = audioOutput;
            }
        }
    }
}