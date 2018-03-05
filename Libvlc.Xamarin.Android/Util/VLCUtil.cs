using System;
using System.Collections.Generic;
using Android.Annotation;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Java.Nio;
using Java.IO;
using Java.Lang;
using Console = System.Console;
using File = Java.IO.File;
using FileNotFoundException = Java.IO.FileNotFoundException;
using IOException = Java.IO.IOException;
using String = Java.Lang.String;
using StringBuilder = System.Text.StringBuilder;
using Uri =  Android.Net.Uri;
namespace Libvlc.Xamarin.Android.Util
{
    public  class VLCUtil
    {
        public static readonly string Tag = "VLC/LibVLC/Util";
       
        private static bool _isCompatible = false;

        public static string ErrorMessage { get; set; }
        private static readonly string[] CpuArchs = { "*Pre-v4", "*v4", "*v4T", "v5T", "v5TE", "v5TEJ", "v6", "v6KZ", "v6T2", "v6K", "v7", "*v6-M", "*v6S-M", "*v7E-M", "*v8" };
        private const string UriAuthorizedChars = "!'()*";

        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public static IList<string> AbiList21
        {
            get
            {
                var abis = Build.SupportedAbis;
                if (abis == null || abis.Count == 0)
                {
                    return AbiList;
                }
                return abis;
            }
        }

        [Obsolete]
        public static IList<string> AbiList => new List<string>{ Build.CpuAbi , Build.CpuAbi2 };

        public static  bool HasCompatibleCpu(Context context)
        {
            string line = "";
            float frequency = -1;
            bool hasNeon = false, hasFpu = false, hasArmV6 = false, hasPlaceHolder = false,
                hasArmV7 = false, hasMips = false, hasX86 = false, is64Bits = false, isIntel = false;
            float bogoMips = -1;

            int processors = 0;
            IList<string> abis;
            ElfData elf = null;

            bool elfHasX86 = false;
            bool elfHasArm = false;
            bool elfHasMips = false;
            bool elfIs64Bits = false;

            FileReader fileReader = null;
            BufferedReader br = null;

            if (ErrorMessage!= null || _isCompatible)
            {
                return _isCompatible; 
            }


         

            /* ABI */
          
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                abis = AbiList21; 
            }
            else
            {
                abis = AbiList; 
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
                        is64Bits = true;
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
                        is64Bits = true;
                        break;
                }                               
            }
            /* Elf */
  
           var lib = SearchLibrary(context.ApplicationInfo);
            if (lib != null && (elf = ReadLib(lib)) != null)
            {
                elfHasX86 = elf.EMachine == Em386 || elf.EMachine == EmX8664;
                elfHasArm = elf.EMachine == EmArm || elf.EMachine == EmAarch64;
                elfHasMips = elf.EMachine == EmMips;
                elfIs64Bits = elf.Is64Bits;

                Log.Info(Tag, "ELF ABI = " + (elfHasArm ? "arm" : elfHasX86 ? "x86" : "mips") + ", " + (elfIs64Bits ? "64bits" : "32bits"));
                Log.Info(Tag, "ELF arch = " + elf.AttArch);
                Log.Info(Tag, "ELF fpu = " + elf.AttFpu);
            }
            else
            {
                Log.Warn(Tag, "WARNING: Unable to read libvlcjni.so; cannot check device ABI!");
            }

       
            try
            {
                fileReader = new FileReader("/proc/cpuinfo");
                br = new BufferedReader(fileReader);
               // string line;
                while (!ReferenceEquals(line = br.ReadLine(), null))
                {
                    if (line.Contains("AArch64"))
                    {
                        hasArmV7 = true;
                        hasArmV6 = true; // Armv8 is backwards compatible to < v7
                    }
                    else if (line.Contains("ARMv7"))
                    {
                        hasArmV7 = true;
                        hasArmV6 = true; // Armv7 is backwards compatible to < v6
                    }
                    else if (line.Contains("ARMv6"))
                    {
                        hasArmV6 = true;
                    }
                    // "clflush size" is a x86-specific cpuinfo tag.
                    // (see kernel sources arch/x86/kernel/cpu/proc.c)
                    else if (line.Contains("clflush size"))
                    {
                        hasX86 = true;
                    }
                    else if (line.Contains("GenuineIntel"))
                    {
                        hasX86 = true;
                    }
                    else if (line.Contains("placeholder"))
                    {
                        hasPlaceHolder = true;
                    }
                    else if (line.Contains("CPU implementer") && line.Contains("0x69"))
                    {
                        isIntel = true;
                    }
                    // "microsecond timers" is specific to MIPS.
                    // see arch/mips/kernel/proc.c
                    else if (line.Contains("microsecond timers"))
                    {
                        hasMips = true;
                    }
                    if (line.Contains("neon") || line.Contains("asimd"))
                    {
                        hasNeon = true;
                    }
                    if (line.Contains("vfp") || (line.Contains("Features") && line.Contains("fp")))
                    {
                        hasFpu = true;
                    }
                    if (line.StartsWith("processor", StringComparison.Ordinal))
                    {
                        processors++;
                    }
                    if (bogoMips < 0 && line.ToLower().Contains("bogomips"))
                    {
                        string[] bogoParts = line.Split(':');
                        try
                        {
                            bogoMips = float.Parse(bogoParts[1].Trim());
                        }
                        catch (FormatException)
                        {
                            bogoMips = -1; // invalid bogomips
                        }
                    }

                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);

            }
            finally
            {
                if (br != null)
                {
                    try
                    {
                        br.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
                if (fileReader != null)
                {
                    try
                    {
                        fileReader.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            if (processors == 0)
                processors = 1; // possibly borked cpuinfo?
            _isCompatible = true;

            /* compare ELF with ABI/cpuinfo */
            if (elf != null)
            {
                // Enforce proper architecture to prevent problems
                if (elfHasX86 && !hasX86)
                {
                    //Some devices lie on their /proc/cpuinfo
                    // they seem to have a 'Hardware	: placeholder' property
                    if (hasPlaceHolder && isIntel)
                    {
                        Log.Debug(Tag, "Emulated armv7 detected, trying to launch x86 libraries");
                    }
                    else
                    {
                        ErrorMessage = "x86 build on non-x86 device";
                        _isCompatible = false;
                    }
                }
                else if (elfHasArm && !hasArmV6)
                {
                    ErrorMessage = "ARM build on non ARM device";
                    _isCompatible = false;
                }

                if (elfHasMips && !hasMips)
                {
                    ErrorMessage = "MIPS build on non-MIPS device";
                    _isCompatible = false;
                }
                else if (elfHasArm && hasMips)
                {
                    ErrorMessage = "ARM build on MIPS device";
                    _isCompatible = false;
                }

                if (elf.EMachine == EmArm && elf.AttArch.StartsWith("v7") && !hasArmV7)
                {
                    ErrorMessage = "ARMv7 build on non-ARMv7 device";
                    _isCompatible = false;
                }
                if (elf.EMachine == EmArm)
                {
                    if (elf.AttArch.StartsWith("v6") && !hasArmV6)
                    {
                        ErrorMessage = "ARMv6 build on non-ARMv6 device";
                        _isCompatible = false;
                    }
                    else if (elf.AttFpu && !hasFpu)
                    {
                        ErrorMessage = "FPU-enabled build on non-FPU device";
                        _isCompatible = false;
                    }
                }
                if (elfIs64Bits && !is64Bits)
                {
                    ErrorMessage = "64bits build on 32bits device";
                    _isCompatible = false;
                }
            }
       
            fileReader = null;
            br = null;
           
            try
            {
                fileReader = new FileReader("/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq");
                br = new BufferedReader(fileReader);
                line = br.ReadLine();
                if (line != null)
                {
                    frequency = float.Parse(line) / 1000.0f; // Convert to MHz
                }
            }
            catch (IOException)
            {
                Log.Warn(Tag, "Could not find maximum CPU frequency!");
            }
            catch (FormatException)
            {
                Log.Warn(Tag, "Could not parse maximum CPU frequency!");
                Log.Warn(Tag, "Failed to parse: " + line);
            }
            finally
            {
                if (br != null)
                {
                    try
                    {
                        br.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
                if (fileReader != null)
                {
                    try
                    {
                        fileReader.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            // Store into MachineSpecs
             MachineSpecs = new MachineSpecs();
            Log.Debug(Tag, "machineSpecs: hasArmV6: " + hasArmV6 + ", hasArmV7: " + hasArmV7 + ", hasX86: " + hasX86 + ", is64bits: " + is64Bits);
            MachineSpecs.HasArmV6 = hasArmV6;
            MachineSpecs.HasArmV7 = hasArmV7;
            MachineSpecs.HasFpu = hasFpu;
            MachineSpecs.HasMips = hasMips;
            MachineSpecs.HasNeon = hasNeon;
            MachineSpecs.HasX86 = hasX86;
            MachineSpecs.Is64Bits = is64Bits;
            MachineSpecs.BogoMips = bogoMips;
            MachineSpecs.Processors = processors;
            MachineSpecs.Frequency = frequency;
            return _isCompatible;
        }

        public static MachineSpecs MachineSpecs { get; set; }

        [TargetApi(Value = (int)BuildVersionCodes.Gingerbread)]
        private static File SearchLibrary(ApplicationInfo applicationInfo)
        {
            // Search for library path
            var libraryPaths = new string[1];
            libraryPaths[0] = applicationInfo.NativeLibraryDir;

            if (ReferenceEquals(libraryPaths[0], null))
            {
                Log.Error(Tag, "can't find library path");
                return null;
            }

            // Search for libvlcjni.so
            File lib;
            foreach (var libraryPath in libraryPaths)
            {
                lib = new File(libraryPath, "libvlcjni.so");
                if (lib.Exists() && lib.CanRead())
                {
                    return lib;
                }
            }
            Log.Error(Tag, "WARNING: Can't find shared library");
            return null;
        }

        private static ElfData ReadLib(File file)
        {
            RandomAccessFile @in = null;
            try
            {
                @in = new RandomAccessFile(file, "r");

                ElfData elf = new ElfData();
                if (!ReadHeader(@in, elf))
                {
                    return null;
                }

                switch (elf.EMachine)
                {
                    case Em386:
                    case EmMips:
                    case EmX8664:
                    case EmAarch64:
                        return elf;
                    case EmArm:
                        @in.Close();
                        @in = new RandomAccessFile(file, "r");
                        if (!ReadSection(@in, elf))
                        {
                            return null;
                        }
                        @in.Close();
                        @in = new RandomAccessFile(file, "r");
                        if (!ReadArmAttributes(@in, elf))
                        {
                            return null;
                        }
                        break;
                    default:
                        return null;
                }
                return elf;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            finally
            {
                try
                {
                    if (@in != null)
                    {
                        @in.Close();
                    }
                }
                catch (IOException)
                {
                }
            }
            return null;
        }

        private static bool ReadArmAttributes(RandomAccessFile @in, ElfData elf)
        {
            byte[] bytes = new byte[elf.ShSize];
            @in.Seek(elf.ShOffset);
            @in.ReadFully(bytes);

            // wrap bytes in a ByteBuffer to force endianess
            ByteBuffer buffer = ByteBuffer.Wrap(bytes);
            buffer.Order(elf.Order);

            //http://infocenter.arm.com/help/topic/com.arm.doc.ihi0044e/IHI0044E_aaelf.pdf
            //http://infocenter.arm.com/help/topic/com.arm.doc.ihi0045d/IHI0045D_ABI_addenda.pdf
            if (buffer.Get() != 'A') // format-version
            {
                return false;
            }

            // sub-sections loop
            while (buffer.Remaining() > 0)
            {
                int startSection = buffer.Position();
                int length = buffer.Int;
                string vendor = GetString(buffer);
                if (vendor.Equals("aeabi"))
                {
                    // tags loop
                    while (buffer.Position() < startSection + length)
                    {
                        int start = buffer.Position();
                        int tag = buffer.Get();
                        int size = buffer.Int;
                        // skip if not Tag_File, we don't care about others
                        if (tag != 1)
                        {
                            buffer.Position(start + size);
                            continue;
                        }

                        // attributes loop
                        while (buffer.Position() < start + size)
                        {
                            tag = GetUleb128(buffer);
                            if (tag == 6)
                            { // CPU_arch
                                int arch = GetUleb128(buffer);
                                elf.AttArch = CpuArchs[arch];
                            }
                            else if (tag == 27)
                            { // ABI_HardFP_use
                                GetUleb128(buffer);
                                elf.AttFpu = true;
                            }
                            else
                            {
                                // string for 4=CPU_raw_name / 5=CPU_name / 32=compatibility
                                // string for >32 && odd tags
                                // uleb128 for other
                                tag %= 128;
                                if (tag == 4 || tag == 5 || tag == 32 || (tag > 32 && (tag & 1) != 0))
                                {
                                    GetString(buffer);
                                }
                                else
                                {
                                    GetUleb128(buffer);
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return true;
        }
        private static int GetUleb128(ByteBuffer buffer)
        {
            int ret = 0;
            int c;
            do
            {
                ret <<= 7;
                c = buffer.Get();
                ret |= c & 0x7f;
            } while ((c & 0x80) > 0);

            return ret;
        }
        private static string GetString(ByteBuffer buffer)
        {
            StringBuilder sb = new StringBuilder(buffer.Limit());
            while (buffer.Remaining() > 0)
            {
                char c = (char)buffer.Get();
                if (c == (char)0)
                {
                    break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool ReadSection(RandomAccessFile @in, ElfData elf)
        {
            byte[] bytes = new byte[SectionHeaderSize];
            @in.Seek(elf.EShoff);

            for (int i = 0; i < elf.EShnum; ++i)
            {
                @in.ReadFully(bytes);

                // wrap bytes in a ByteBuffer to force endianess
                ByteBuffer buffer = ByteBuffer.Wrap(bytes);
                buffer.Order(elf.Order);

                int shType = buffer.GetInt(4); // Section type
                if (shType != ShtArmAttributes)
                {
                    continue;
                }

                elf.ShOffset = buffer.GetInt(16); // Section file offset
                elf.ShSize = buffer.GetInt(20); // Section size in bytes
                return true;
            }

            return false;
        }




        private const int Em386 = 3;
        private const int EmMips = 8;
        private const int EmArm = 40;
        private const int EmX8664 = 62;
        private const int EmAarch64 = 183;
        private const int ElfHeaderSize = 52;
        private const int SectionHeaderSize = 40;
        private const int ShtArmAttributes = 0x70000003;


        private static bool ReadHeader(RandomAccessFile @in, ElfData elf)
        {
            // http://www.sco.com/developers/gabi/1998-04-29/ch4.eheader.html
            byte[] bytes = new byte[ElfHeaderSize];
            @in.ReadFully(bytes);
            if (bytes[0] != 127 || bytes[1] != (sbyte)'E' || bytes[2] != (sbyte)'L' || bytes[3] != (sbyte)'F' || (bytes[4] != 1 && bytes[4] != 2))
            {
                Log.Error(Tag, "ELF header invalid");
                return false;
            }

            elf.Is64Bits = bytes[4] == 2;
            elf.Order = bytes[5] == 1 ? ByteOrder.LittleEndian : ByteOrder.BigEndian; // ELFDATA2MSB -  ELFDATA2LSB

            // wrap bytes in a ByteBuffer to force endianess
            ByteBuffer buffer = ByteBuffer.Wrap(bytes);
            buffer.Order(elf.Order);

            elf.EMachine = buffer.GetShort(18); // Architecture
            elf.EShoff = buffer.GetInt(32); // Section header table file offset
            elf.EShnum = buffer.GetShort(48); // Section header table entry count
            return true;
        }




        private class ElfData
        {
           public  ByteOrder Order;
            public bool Is64Bits;
            public int EMachine;
            public int EShoff;
            public int EShnum;
            public int ShOffset;
            public int ShSize;
            public string AttArch;
            public bool AttFpu;
        }

        /// <summary>
        /// VLC authorize only "-._~" in Mrl format, android Uri authorize "_-!.~'()*".
        /// Therefore, decode the characters authorized by Android Uri when creating an Uri from VLC.
        /// </summary>
        public static Uri UriFromMrl(String mrl)
        {
  
            char[] array = mrl.ToCharArray();

            StringBuilder sb = new StringBuilder(array.Length * 2);
            for (int i = 0; i < array.Length; ++i)
            {
  
                char c = array[i];
                if (c == '%')
                {
                    if (array.Length - i >= 3)
                    {
                        try
                        {
  
                            int hex = Convert.ToInt32(new string(array, i + 1, 2), 16);
                            if (UriAuthorizedChars.IndexOf((char)hex) != -1)
                            {
                                sb.Append((char)hex);
                                i += 2;
                                continue;
                            }
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }
                sb.Append(c);
            }

            return  Uri.Parse(sb.ToString());
        }


        public static string EncodeVLCUri(Uri uri)
        {
            return EncodeVLCString(uri.ToString());
        }

        /// <summary>
        /// VLC only acccepts "-._~" in Mrl format, android Uri accepts "_-!.~'()*".
        /// Therefore, encode the characters authorized by Android Uri when creating a mrl from an Uri.
        /// </summary>
      
        public static string EncodeVLCString(string mrl)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final char[] array = mrl.toCharArray();
            char[] array = mrl.ToCharArray();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final StringBuilder sb = new StringBuilder(array.length * 2);
            StringBuilder sb = new StringBuilder(array.Length * 2);

            foreach (char c in array)
            {
                if (UriAuthorizedChars.IndexOf(c) != -1)
                {
                    // Todo: check this one the original is        
                    sb.Append("%").Append(Integer.ToHexString(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }



    }
}