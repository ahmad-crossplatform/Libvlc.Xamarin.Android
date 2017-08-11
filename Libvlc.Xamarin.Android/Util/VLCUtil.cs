using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Nio;
using Java.IO;

namespace Libvlc.Xamarin.Android.Util
{
    public  class VLCUtil
    {
        public static readonly string Tag = "VLC/LibVLC/Util";
       
        private static bool _isCompatible = false;

        public static string ErrorMessage { get; }

    
        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public static IList<string> ABIList21
        {
            get
            {
                var abis = Build.SupportedAbis;
                if (abis == null || abis.Count == 0)
                {
                    return ABIList;
                }
                return abis;
            }
        }

        [Obsolete]
        public static IList<string> ABIList => new List<string>{ Build.CpuAbi , Build.CpuAbi2 };

        public static  bool HasCompatibleCPU(Context context)
        {
            if (ErrorMessage!= null || _isCompatible)
            {
                return _isCompatible; 
            }
            bool hasNeon = false, hasFpu = false, hasArmV6 = false, hasPlaceHolder = false,
                hasArmV7 = false, hasMips = false, hasX86 = false, is64bits = false, isIntel = false;

            float bogoMIPS = -1;
            int processors = 0;

            /* ABI */
            IList<string> abis;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                abis = ABIList21; 
            }
            else
            {
                abis = ABIList; 
            }

            foreach (var abi in abis)
            {
                switch (abi)
                {
                    case "x86":
                        hasX86 = true;
                        break;
                    case "x86_64":
                        hasX86 = true;
                        is64bits = true;
                        break;
                    case "armeabi-v7a":
                        hasArmV7 = true;
                        hasArmV6 = true; /* Armv7 is backwards compatible to < v6 */
                        break;
                    case "armeabi":
                        hasArmV6 = true;
                        break;
                    case "arm64-v8a":
                        hasNeon = true;
                        hasArmV6 = true;
                        hasArmV7 = true;
                        is64bits = true;
                        break;
                }                               
            }
            /* Elf */
            ElfData elf = null;
           bool elfHasX86 = false;
           bool elfHasArm = false;
           bool elfHasMips = false;
           bool elfIs64bits = false;

        }

        [TargetApi(Value = (int)BuildVersionCodes.Gingerbread)]
        private static File SearchLibrary(ApplicationInfo applicationInfo)
        {
            // Search for library path
            string[] libraryPaths;
            if ((applicationInfo.Flags & (ApplicationInfoFlags) Protection.FlagSystem) != 0) //todo: needs revision               
            {

                string property = System.GetProperty("java.library.path");
                libraryPaths = property.Split(":", true);
            }
            else
            {
                libraryPaths = new string[1];
                libraryPaths[0] = applicationInfo.nativeLibraryDir;
            }
            if (string.ReferenceEquals(libraryPaths[0], null))
            {
                Log.e(TAG, "can't find library path");
                return null;
            }

            // Search for libvlcjni.so
            File lib;
            foreach (string libraryPath in libraryPaths)
            {
                lib = new File(libraryPath, "libvlcjni.so");
                if (lib.exists() && lib.canRead())
                {
                    return lib;
                }
            }
            Log.e(TAG, "WARNING: Can't find shared library");
            return null;
        }


        private class ElfData
        {
            ByteOrder order;
            bool is64bits;
            int e_machine;
            int e_shoff;
            int e_shnum;
            int sh_offset;
            int sh_size;
            string att_arch;
            bool att_fpu;
        }


        public VLCUtil()
        {
            throw new NotImplementedException(); 
        }
    }
}