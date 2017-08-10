using System.Collections.Generic;

namespace Libvlc.Xamarin.Android.Util
{
    namespace org.videolan.libvlc.util
    {
        public class Extensions
        {
            public static readonly HashSet<string> Video = new HashSet<string>();
            public static readonly HashSet<string> Audio = new HashSet<string>();
            public static readonly HashSet<string> Subtitles = new HashSet<string>();
            public static readonly HashSet<string> Playlist = new HashSet<string>();

            static Extensions()
            {
                var videoExtensions = new[]
                {
                    ".3g2", ".3gp", ".3gp2", ".3gpp", ".amv", ".asf", ".avi", ".divx", ".drc", ".dv", ".f4v", ".flv",
                    ".gvi", ".gxf", ".ismv", ".iso", ".m1v", ".m2v", ".m2t", ".m2ts", ".m4v", ".mkv", ".mov", ".mp2",
                    ".mp2v", ".mp4", ".mp4v", ".mpe", ".mpeg", ".mpeg1", ".mpeg2", ".mpeg4", ".mpg", ".mpv2", ".mts",
                    ".mtv", ".mxf", ".mxg", ".nsv", ".nut", ".nuv", ".ogm", ".ogv", ".ogx", ".ps", ".rec", ".rm",
                    ".rmvb", ".tod", ".ts", ".tts", ".vob", ".vro", ".webm", ".wm", ".wmv", ".wtv", ".xesc"
                };

                var audioExtensions = new[]
                {
                    ".3ga", ".a52", ".aac", ".ac3", ".adt", ".adts", ".aif", ".aifc", ".aiff", ".amr", ".aob", ".ape",
                    ".awb", ".caf", ".dts", ".flac", ".it", ".m4a", ".m4b", ".m4p", ".mid", ".mka", ".mlp", ".mod",
                    ".mpa", ".mp1", ".mp2", ".mp3", ".mpc", ".mpga", ".oga", ".ogg", ".oma", ".opus", ".ra", ".ram",
                    ".rmi", ".s3m", ".spx", ".tta", ".voc", ".vqf", ".w64", ".wav", ".wma", ".wv", ".xa", ".xm"
                };

                var subtitlesExtensions = new[]
                {
                    ".idx", ".sub", ".srt", ".ssa", ".ass", ".smi", ".utf", ".utf8", ".utf-8", ".rt", ".aqt", ".txt",
                    ".usf", ".jss", ".cdg", ".psb", ".mpsub", ".mpl2", ".pjs", ".dks", ".stl", ".vtt"
                };

                var playlistExtensions = new[] {".m3u", ".asx", ".b4s", ".pls", ".xspf"};

                Video.UnionWith(videoExtensions);
                Audio.UnionWith(audioExtensions);
                Subtitles.UnionWith(subtitlesExtensions);
                Playlist.UnionWith(playlistExtensions);
            }
        }
    }
}