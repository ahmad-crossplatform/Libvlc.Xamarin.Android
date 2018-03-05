using System;
using System.Runtime.InteropServices;
using System.Text;
using Java.IO;
using Libvlc.Xamarin.Android.Interfaces;
using Libvlc.Xamarin.Android.Util;
using String = Java.Lang.String;
using Uri = Android.Net.Uri;

namespace Libvlc.Xamarin.Android.Media
{
    public class Media : VLCObject<MediaEvent>
    {
        private const string Tag = "LibVLC/Media";
        private const int ParseStatusInit = 0x00;
        private const int ParseStatusParsing = 0x01;
        private const int ParseStatusParsed = 0x02;
        private readonly string[] _nativeMetas = new string[Meta.MAX];

        private readonly Uri _uri;
        private bool _codecOptionSet;
        private long _duration = -1;
        private Track[] _nativeTracks;
        private int _parseStatus = ParseStatusInit;
        private int _state = -1;
        private MediaList _subItems;
        private int _type = -1;

	    /// <summary>
	    ///     Create a Media from libVLC and a local path starting with '/'.
	    /// </summary>
	    /// <param name="libVlc"> a valid libVLC </param>
	    /// <param name="path"> an absolute local path </param>
	    public Media(LibVlc libVlc, string path) : base(libVlc)
        {
            nativeNewFromPath(libVlc, path);
            _uri = VLCUtil.UriFromMrl(nativeGetMrl());
        }

	    /// <summary>
	    ///     Create a Media from libVLC and a Uri
	    /// </summary>
	    /// <param name="libVLC"> a valid libVLC </param>
	    /// <param name="uri"> a valid RFC 2396 Uri </param>
	    public Media(LibVlc libVLC, Uri uri) : base(libVLC)
        {
            nativeNewFromLocation(libVLC, VLCUtil.EncodeVLCUri(uri));
            _uri = uri;
        }

	    /// <summary>
	    ///     Create a Media from libVLC and a FileDescriptor
	    /// </summary>
	    /// <param name="libVLC"> a valid LibVLC </param>
	    /// <param name="fd"> file descriptor object </param>
	    public Media(LibVlc libVLC, FileDescriptor fd) : base(libVLC)
        {
            nativeNewFromFd(libVLC, fd);
            _uri = VLCUtil.UriFromMrl(nativeGetMrl());
        }

        //todo: find what is wrong with this constructor check the commented base
        /// <param name="ml"> Should not be released and locked </param>
        /// <param name="index"> index of the Media from the MediaList </param>
        protected internal Media(MediaList ml, int index) //: base(ml)
        {
            if (ml == null || ml.IsReleased()) throw new ArgumentException("MediaList is null or released");
            if (!ml.Locked) throw new InvalidOperationException("MediaList should be locked");
            nativeNewFromMediaList(ml, index);
            _uri = VLCUtil.UriFromMrl(nativeGetMrl());
        }

        public virtual IMediaEventListener MediaEventListener
        {
            set => SetEventListener(value);
        }

	    /// <summary>
	    ///     Get the MRL associated with the Media.
	    /// </summary>
	    public virtual Uri Uri
        {
            get
            {
                lock (this)
                {
                    return _uri;
                }
            }
        }

	    /// <summary>
	    ///     Get the duration of the media.
	    /// </summary>
	    public virtual long Duration
        {
            get
            {
                lock (this)
                {
                    if (_duration != -1) return _duration;
                    if (IsReleased()) return 0;
                }

                var duration = nativeGetDuration();
                lock (this)
                {
                    _duration = duration;
                    return _duration;
                }
            }
        }

	    /// <summary>
	    ///     Get the state of the media.
	    /// </summary>
	    /// <seealso cref= State
	    /// </seealso>
	    public virtual int State
        {
            get
            {
                lock (this)
                {
                    if (_state != -1) return _state;
                    if (IsReleased()) return Android.Media.State.Error;
                }

                var state = nativeGetState();
                lock (this)
                {
                    _state = state;
                    return _state;
                }
            }
        }

	    /// <summary>
	    ///     Returns true if the media is parsed This Media should be alive (not released).
	    /// </summary>
	    public virtual bool Parsed
        {
            get
            {
                lock (this)
                {
                    return (_parseStatus & ParseStatusParsed) != 0;
                }
            }
        }

	    /// <summary>
	    ///     Get the type of the media
	    /// </summary>
	    /// <seealso cref=
	    /// <seealso cref="Type" />
	    /// </seealso>
	    public virtual int Type
        {
            get
            {
                lock (this)
                {
                    if (_type != -1) return _type;
                    if (IsReleased()) return Android.Media.Type.Unknown;
                }

                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final int type = nativeGetType();
                var type = nativeGetType();
                lock (this)
                {
                    _type = type;
                    return _type;
                }
            }
        }


        private Track[] Tracks
        {
            get
            {
                lock (this)
                {
                    if (_nativeTracks != null) return _nativeTracks;
                    if (IsReleased()) return null;
                }

                var tracks = nativeGetTracks();
                lock (this)
                {
                    _nativeTracks = tracks;
                    return _nativeTracks;
                }
            }
        }

	    /// <summary>
	    ///     Get the Track count.
	    /// </summary>
	    public virtual int TrackCount
        {
            get
            {
                var tracks = Tracks;
                return tracks != null ? tracks.Length : 0;
            }
        }


        private static string MediaCodecModule => AndroidUtil.IsLolliPopOrLater ? "mediacodec_ndk" : "mediacodec_jni";

	    /// <summary>
	    ///     Get a media's slave list
	    ///     The list will contain slaves parsed by VLC or previously added by
	    ///     <seealso cref="#addSlave(Slave)" />. The typical use case of this function is to save
	    ///     a list of slave in a database for a later use.
	    /// </summary>
	    public virtual Slave[] Slaves => nativeGetSlaves();

	    /// <summary>
	    ///     Get the stats related to the playing media
	    /// </summary>
	    public virtual Stats Stats => nativeGetStats();


        private static Track CreateAudioTrackFromNative(string codec, string originalCodec, int id, int profile,
            int level, int bitrate, string language, string description, int channels, int rate)
        {
            // Used from JNI
            return new AudioTrack(codec, originalCodec, id, profile, level, bitrate, language, description, channels,
                rate);
        }

        private static Track CreateVideoTrackFromNative(string codec, string originalCodec, int id, int profile,
            int level, int bitrate, string language, string description, int height, int width, int sarNum, int sarDen,
            int frameRateNum, int frameRateDen, int orientation, int projection)
        {
            return new VideoTrack(codec, originalCodec, id, profile, level, bitrate, language, description, height,
                width, sarNum, sarDen, frameRateNum, frameRateDen, orientation, projection);
        }


        private static Track CreateSubtitleTrackFromNative(string codec, string originalCodec, int id, int profile,
            int level, int bitrate, string language, string description, string encoding)
        {
            // Used from JNI
            return new SubtitleTrack(codec, originalCodec, id, profile, level, bitrate, language, description,
                encoding);
        }


        private static Track CreateUnknownTrackFromNative(string codec, string originalCodec, int id, int profile,
            int level, int bitrate, string language, string description)
        {
            // Used from JNI
            return new UnknownTrack(codec, originalCodec, id, profile, level, bitrate, language, description);
        }


        private static Slave CreateSlaveFromNative(int type, int priority, string uri)
        {
            // Used from JNI
            return new Slave(type, priority, uri);
        }

        private static Stats CreateStatsFromNative(int readBytes, float inputBitrate, int demuxReadBytes,
            float demuxBitrates, int demuxCorrupted, int demuxDiscontinuity, int decodedVideo, int decodedAudio,
            int displayedPictures, int lostPictures, int playedAbuffers, int lostAbuffers, int sentPackets,
            int sentBytes, float sendBitrate)
        {
            // Used from JNI
            return new Stats(readBytes, inputBitrate, demuxReadBytes, demuxBitrates, demuxCorrupted, demuxDiscontinuity,
                decodedVideo, decodedAudio, displayedPictures, lostPictures, playedAbuffers, lostAbuffers, sentPackets,
                sentBytes, sendBitrate);
        }

	    /// <summary>
	    ///     Get the subItems MediaList associated with the Media. This Media should be alive (not released).
	    /// </summary>
	    /// <returns> subItems as a MediaList. This MediaList should be released with <seealso cref="#release()" />. </returns>
	    public virtual MediaList SubItems()
        {
            lock (this)
            {
                if (_subItems != null)
                {
                    _subItems.Retain();
                    return _subItems;
                }
            }

            var subItems = new MediaList(this);
            lock (this)
            {
                _subItems = subItems;
                _subItems.Retain();
                return _subItems;
            }
        }

        private void PostParse()
        {
            lock (this)
            {
                // fetch if parsed and not fetched
                if ((_parseStatus & ParseStatusParsed) != 0) return;
                _parseStatus &= ~ParseStatusParsing;
                _parseStatus |= ParseStatusParsed;
                _nativeTracks = null;
                _duration = -1;
                _state = -1;
                _type = -1;
            }
        }

	    /// <summary>
	    ///     Parse the media synchronously with a flag. This Media should be alive (not released).
	    /// </summary>
	    /// <param name="flags"> see <seealso cref="Android.Media.Parse" /> </param>
	    /// <returns> true in case of success, false otherwise. </returns>
	    public virtual bool Parse(int flags)
        {
            var parse = false;
            lock (this)
            {
                if ((_parseStatus & (ParseStatusParsed | ParseStatusParsing)) == 0)
                {
                    _parseStatus |= ParseStatusParsing;
                    parse = true;
                }
            }

            if (parse && nativeParse(flags))
            {
                PostParse();
                return true;
            }

            return false;
        }

	    /// <summary>
	    ///     Parse the media and local art synchronously. This Media should be alive (not released).
	    /// </summary>
	    /// <returns> true in case of success, false otherwise. </returns>
	    public virtual bool Parse()
        {
            return Parse(Android.Media.Parse.FetchLocal);
        }

	    /// <summary>
	    ///     Parse the media asynchronously with a flag. This Media should be alive (not released).
	    ///     To track when this is over you can listen to <seealso cref="Event#ParsedChanged" />
	    ///     event (only if this methods returned true).
	    /// </summary>
	    /// <param name="flags"> see <seealso cref="Android.Media.Parse" /> </param>
	    /// <param name="timeout">
	    ///     maximum time allowed to preparse the media. If -1, the
	    ///     default "preparse-timeout" option will be used as a timeout. If 0, it will
	    ///     wait indefinitely. If > 0, the timeout will be used (in milliseconds).
	    /// </param>
	    /// <returns> true in case of success, false otherwise. </returns>
	    public virtual bool ParseAsync(int flags, int timeout)
        {
            var parse = false;
            lock (this)
            {
                if ((_parseStatus & (ParseStatusParsed | ParseStatusParsing)) == 0)
                {
                    _parseStatus |= ParseStatusParsing;
                    parse = true;
                }
            }

            return parse && nativeParseAsync(flags, timeout);
        }

        public virtual bool ParseAsync(int flags)
        {
            return ParseAsync(flags, -1);
        }

	    /// <summary>
	    ///     Parse the media and local art asynchronously. This Media should be alive (not released).
	    /// </summary>
	    /// <seealso cref= # parseAsync( int
	    /// )
	    /// </seealso>
	    public virtual bool ParseAsync()
        {
            return ParseAsync(Android.Media.Parse.FetchLocal);
        }

	    /// <summary>
	    ///     Get a Track
	    ///     The Track can be casted to <seealso cref="AudioTrack" />, <seealso cref="VideoTrack" /> or
	    ///     <seealso cref="SubtitleTrack" /> in function of the <seealso cref="Track.Type" />.
	    /// </summary>
	    /// <param name="idx"> index of the track </param>
	    /// <returns> Track or null if not idx is not valid </returns>
	    /// <seealso cref= # getTrackCount
	    /// (
	    /// )
	    /// </seealso>
	    public virtual Track GetTrack(int idx)
        {
            var tracks = Tracks;
            if (tracks == null || idx < 0 || idx >= tracks.Length) return null;
            return tracks[idx];
        }

	    /// <summary>
	    ///     Get a Meta.
	    /// </summary>
	    /// <param name="id"> see <seealso cref="Meta" /> </param>
	    /// <returns> meta or null if not found </returns>
	    public virtual string GetMeta(int id)
        {
            if (id < 0 || id >= Meta.MAX) return null;

            lock (this)
            {
                if (_nativeMetas[id] != null) return _nativeMetas[id];
                if (IsReleased()) return null;
            }

            var meta = nativeGetMeta(id);
            lock (this)
            {
                _nativeMetas[id] = meta;
                return meta;
            }
        }

	    /// <summary>
	    ///     Add or remove hw acceleration media options
	    /// </summary>
	    /// <param name="enabled"> if true, hw decoder will be used </param>
	    /// <param name="force"> force hw acceleration even for unknown devices </param>
	    public virtual void SetHwDecoderEnabled(bool enabled, bool force)
        {
            var decoder = enabled ? HwDecoderUtil.GetDecoderFromDevice() : HwDecoderUtil.Decoder.None;

            /* Unknown device but the user asked for hardware acceleration */
            if (decoder == HwDecoderUtil.Decoder.Unknown && force) decoder = HwDecoderUtil.Decoder.All;

            if (decoder == HwDecoderUtil.Decoder.None || decoder == HwDecoderUtil.Decoder.Unknown)
            {
                AddOption(":codec=all");
                return;
            }

            /*
             * Set higher caching values if using iomx decoding, since some omx
             * decoders have a very high latency, and if the preroll data isn't
             * enough to make the decoder output a frame, the playback timing gets
             * started too soon, and every decoded frame appears to be too late.
             * On Nexus One, the decoder latency seems to be 25 input packets
             * for 320x170 H.264, a few packets less on higher resolutions.
             * On Nexus S, the decoder latency seems to be about 7 packets.
             */
            AddOption(":file-caching=1500");
            AddOption(":network-caching=1500");

            var sb = new StringBuilder(":codec=");
            if (decoder == HwDecoderUtil.Decoder.MediaCodec || decoder == HwDecoderUtil.Decoder.All)
                sb.Append(MediaCodecModule).Append(",");
            if (force && (decoder == HwDecoderUtil.Decoder.OMX || decoder == HwDecoderUtil.Decoder.All))
                sb.Append("iomx,");
            sb.Append("all");

            AddOption(sb.ToString());
        }

	    /// <summary>
	    ///     Enable HWDecoder options if not already set
	    /// </summary>
	    protected internal virtual void SetDefaultMediaPlayerOptions()
        {
            bool codecOptionSet;
            lock (this)
            {
                codecOptionSet = _codecOptionSet;
                _codecOptionSet = true;
            }

            if (!codecOptionSet) SetHwDecoderEnabled(true, false);

            /* dvdnav need to be explicitly forced for network playbacks */
            if (_uri != null && _uri.Scheme != null &&
                !_uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase) && _uri.LastPathSegment != null &&
                _uri.LastPathSegment.ToLower().EndsWith(".iso", StringComparison.Ordinal))
                AddOption(":demux=dvdnav,any");
        }

	    /// <summary>
	    ///     Add an option to this Media. This Media should be alive (not released).
	    /// </summary>
	    /// <param name="option"> ":option" or ":option=value" </param>
	    public virtual void AddOption(string option)
        {
            lock (this)
            {
                if (!_codecOptionSet && option.StartsWith(":codec=", StringComparison.Ordinal)) _codecOptionSet = true;
            }

            nativeAddOption(option);
        }


	    /// <summary>
	    ///     Add a slave to the current media.
	    ///     A slave is an external input source that may contains an additional subtitle
	    ///     track (like a .srt) or an additional audio track (like a .ac3).
	    ///     This function must be called before the media is parsed (via <seealso cref="#parseAsync(int)" />} or
	    ///     before the media is played (via <seealso cref="MediaPlayer#play()" />)
	    /// </summary>
	    public virtual void AddSlave(Slave slave)
        {
            nativeAddSlave(slave.type, slave.priority, slave.uri);
        }

	    /// <summary>
	    ///     Clear all slaves previously added by <seealso cref="#addSlave(Slave)" /> or internally.
	    /// </summary>
	    public virtual void ClearSlaves()
        {
            nativeClearSlaves();
        }


        /* JNI */
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromPath(LibVlc libVLC, string path);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromLocation(LibVlc libVLC, string location);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromFd(LibVlc libVLC, FileDescriptor fd);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromMediaList(MediaList ml, int index);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeRelease();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeParseAsync(int flags, int timeout);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeParse(int flags);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern String nativeGetMrl();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetState();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern string nativeGetMeta(int id);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern Track[] nativeGetTracks();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern long nativeGetDuration();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetType();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeAddOption(string option);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeAddSlave(int type, int priority, string uri);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeClearSlaves();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern Slave[] nativeGetSlaves();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern Stats nativeGetStats();


        protected internal override MediaEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            lock (this)
            {
                switch (eventType)
                {
                    case MediaEvent.MetaChanged:
                        // either we update all metas (if first call) or we update a specific meta
                        var id = (int) arg1;
                        if (id >= 0 && id < Meta.MAX) _nativeMetas[id] = null;
                        return new MediaEvent(eventType, arg1);
                    case MediaEvent.DurationChanged:
                        _duration = -1;
                        break;
                    case MediaEvent.ParsedChanged:
                        PostParse();
                        return new MediaEvent(eventType, arg1);
                    case MediaEvent.StateChanged:
                        _state = -1;
                        break;
                }

                return new MediaEvent(eventType);
            }
        }

        protected internal override void OnReleaseNative()
        {
            if (_subItems != null) _subItems.Release();
            nativeRelease();
        }
    }
}