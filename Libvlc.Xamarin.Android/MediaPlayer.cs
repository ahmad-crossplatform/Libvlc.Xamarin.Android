using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Android.Annotation;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Util;
using Java.IO;
using Libvlc.Xamarin.Android.Interfaces;
using Libvlc.Xamarin.Android.Media;
using Libvlc.Xamarin.Android.Util;
using Uri = Android.Net.Uri;

namespace Libvlc.Xamarin.Android
{
    public class MediaPlayer : VLCObject<MediaPlayerEvent>
    {
        private static bool _isPlaying;
        private static bool _isPlayRequested;
        private static int _voutCount;
        private static string _audioPlugOutputDevice = "stereo";

        private readonly MediaPlayerAudioDeviceCallback _audioDeviceCallback;

        private readonly BroadcastReceiver _audioPlugReceiver;

        private readonly AWindow _window;
        private string _audioOutput = "android_audiotrack";
        private string _audioOutputDevice;

        private bool _audioPlugRegistered;
        private bool _isAudioDeviceFromUser;
        private bool _isAudioReset;
        private Media.Media _media;


        /// <summary>
        ///     Create an empty MediaPlayer
        /// </summary>
        /// <param name="libVLC"> a valid libVLC </param>
        public MediaPlayer(LibVlc libVLC) : base(libVLC)
        {
            _window = new AWindow(new AWindowSurfaceCallback(this));
            _audioPlugReceiver =
                AndroidUtil.IsLolliPopOrLater && !AndroidUtil.IsMarshMallowOrLater ? CreateAudioPlugReceiver() : null;

            _audioDeviceCallback =
                AndroidUtil.IsMarshMallowOrLater ? CreateAudioDeviceCallback() : null;


            nativeNewFromLibVlc(libVLC, _window);
        }

        /// <summary>
        ///     Create a MediaPlayer from a Media
        /// </summary>
        /// <param name="media"> a valid Media object </param>
        public MediaPlayer(Media.Media media)
        {
            if (media == null || media.IsReleased()) throw new ArgumentException("Media is null or released");
            _media = media;
            _media.Retain();
            _window = new AWindow(new AWindowSurfaceCallback(this));
            _audioPlugReceiver =
                AndroidUtil.IsLolliPopOrLater && !AndroidUtil.IsMarshMallowOrLater ? CreateAudioPlugReceiver() : null;

            _audioDeviceCallback =
                AndroidUtil.IsMarshMallowOrLater ? CreateAudioDeviceCallback() : null;


            nativeNewFromMedia(_media, _window);
        }

        /// <summary>
        ///     Get the IVLCVout helper.
        /// </summary>
        public virtual IVLCVOut VLCVout => _window;

        /// <summary>
        ///     Set a Media
        /// </summary>
        /// <param name="media"> a valid Media object </param>
        public virtual Media.Media Media
        {
            get
            {
                lock (this)
                {
                    _media?.Retain();
                    return _media;
                }
            }
            set
            {
                if (value != null)
                {
                    if (value.IsReleased()) throw new ArgumentException("Media is released");
                    value.SetDefaultMediaPlayerOptions();
                }

                nativeSetMedia(value);
                lock (this)
                {
                    _media?.Release();
                    value?.Retain();
                    _media = value;
                }
            }
        }

        /// <summary>
        ///     Get the current video scaling factor
        /// </summary>
        /// <returns>
        ///     the currently configured zoom factor, or 0. if the video is set to fit to the
        ///     output window/drawable automatically.
        /// </returns>
        public virtual float Scale
        {
            get => nativeGetScale();
            set => nativeSetScale(value);
        }


        /// <summary>
        ///     Get current video aspect ratio
        /// </summary>
        /// <returns> the video aspect ratio or NULL if unspecified </returns>
        public virtual string AspectRatio
        {
            get => nativeGetAspectRatio();
            set => nativeSetAspectRatio(value);
        }

        /// <summary>
        ///     Get the full description of available titles.
        /// </summary>
        /// <returns> the list of titles </returns>
        public virtual Title[] Titles => nativeGetTitles();

        /// <summary>
        ///     Get the number of available video tracks.
        /// </summary>
        public virtual int VideoTracksCount => nativeGetVideoTracksCount();

        /// <summary>
        ///     Get the list of available video tracks.
        /// </summary>
        public virtual TrackDescription[] VideoTracks => nativeGetVideoTracks();

        /// <summary>
        ///     Get the current video track.
        /// </summary>
        /// <returns> the video track ID or -1 if no active input </returns>
        public virtual int VideoTrack
        {
            get => nativeGetVideoTrack();
            set => SetVideoTrack(value);
        }

        /// <summary>
        ///     Set the enabled state of the video track
        /// </summary>
        /// <param name="enabled"> </param>
        public virtual bool VideoTrackEnabled
        {
            set
            {
                if (!value)
                {
                    VideoTrack = -1;
                }
                else if (VideoTrack == -1)
                {
                    var tracks = VideoTracks;

                    if (tracks != null)
                        foreach (var track in tracks)
                            if (track.id != -1)
                            {
                                VideoTrack = track.id;
                                break;
                            }
                }
            }
        }

        /// <summary>
        ///     Get the current video track
        /// </summary>
        public virtual VideoTrack CurrentVideoTrack
        {
            get
            {
                if (VideoTrack == -1) return null;

                var trackCount = _media.TrackCount;
                for (var i = 0; i < trackCount; ++i)
                {
                    var track = _media.GetTrack(i);
                    if (track.type == VideoTrack) return (VideoTrack) track;
                }

                return null;
            }
        }

        /// <summary>
        ///     Get the number of available audio tracks.
        /// </summary>
        public virtual int AudioTracksCount => nativeGetAudioTracksCount();

        /// <summary>
        ///     Get the list of available audio tracks.
        /// </summary>
        public virtual TrackDescription[] AudioTracks => nativeGetAudioTracks();

        /// <summary>
        ///     Get the current audio track.
        /// </summary>
        /// <returns> the audio track ID or -1 if no active input </returns>
        public virtual int AudioTrack => nativeGetAudioTrack();

        /// <summary>
        ///     Get the current audio delay.
        /// </summary>
        /// <returns> delay in microseconds. </returns>
        public virtual long AudioDelay => nativeGetAudioDelay();

        /// <summary>
        ///     Get the number of available spu (subtitle) tracks.
        /// </summary>
        public virtual int SpuTracksCount => nativeGetSpuTracksCount();

        /// <summary>
        ///     Get the list of available spu (subtitle) tracks.
        /// </summary>
        public virtual TrackDescription[] SpuTracks => nativeGetSpuTracks();


        /// <summary>
        ///     Get the current spu (subtitle) track.
        /// </summary>
        /// <returns> the spu (subtitle) track ID or -1 if no active input </returns>
        public virtual int SpuTrack => nativeGetSpuTrack();

        /// <summary>
        ///     Get the current spu (subtitle) delay.
        /// </summary>
        /// <returns> delay in microseconds. </returns>
        public virtual long SpuDelay => nativeGetSpuDelay();

        public virtual IListener<MediaPlayerEvent> EventListener
        {
            set
            {
                lock (this)
                {
                    SetEventListener(value);
                }
            }
        }

        private void UpdateAudioOutputDevice(long encodingFlags, string defaultDevice)
        {
            var newDeviceId = encodingFlags != 0 ? "encoded:" + encodingFlags : defaultDevice;
            if (newDeviceId.Equals(_audioPlugOutputDevice)) return;
            _audioPlugOutputDevice = newDeviceId;
            SetAudioOutputDeviceInternal(_audioPlugOutputDevice, false);
        }

        private static bool IsEncoded(int encoding)
        {
            switch (encoding)
            {
                //case AudioFormat.ENCODING_AC3:
                case 5:
                //case AudioFormat.ENCODING_E_AC3:
                case 6:
                case 14:
                case 7:
                //	case AudioFormat.ENCODING_DTS:
                case 8:
                    //	case AudioFormat.ENCODING_DTS_HD:
                    return true;
                default:
                    return false;
            }
        }


        private static long GetEncodingFlags(int[] encodings)
        {
            if (encodings == null) return 0;
            long encodingFlags = 0;
            foreach (var encoding in encodings)
                if (IsEncoded(encoding))
                    encodingFlags |= 1 << encoding;
            return encodingFlags;
        }


        [TargetApi(Value = (int) BuildVersionCodes.Lollipop)]
        private BroadcastReceiver CreateAudioPlugReceiver()
        {
            return new AudioPlugReceiver(this);
        }

        [TargetApi(Value = (int) BuildVersionCodes.Lollipop)]
        private void RegisterAudioPlugV21(bool register)
        {
            if (register)
            {
                var intentFilter = new IntentFilter(AudioManager.ActionHdmiAudioPlug);
                var stickyIntent = LibVlc.AppContext.RegisterReceiver(_audioPlugReceiver, intentFilter);
                if (stickyIntent != null) _audioPlugReceiver.OnReceive(LibVlc.AppContext, stickyIntent);
            }
            else
            {
                LibVlc.AppContext.UnregisterReceiver(_audioPlugReceiver);
            }
        }

        private MediaPlayerAudioDeviceCallback CreateAudioDeviceCallback()
        {
            return new MediaPlayerAudioDeviceCallback(this);
        }

        [TargetApi(Value = (int) BuildVersionCodes.M)]
        private void RegisterAudioPlugV23(bool register)
        {
            var am = (AudioManager) LibVlc.AppContext.GetSystemService(Context.AudioService);
            if (register)
            {
                _audioDeviceCallback.OnAudioDevicesAdded(am.GetDevices(GetDevicesTargets.Outputs));
                am.RegisterAudioDeviceCallback(_audioDeviceCallback, null);
            }
            else
            {
                am.UnregisterAudioDeviceCallback(_audioDeviceCallback);
            }
        }

        private void RegisterAudioPlug(bool register)
        {
            if (register == _audioPlugRegistered) return;
            if (_audioDeviceCallback != null)
                RegisterAudioPlugV23(register);
            else if (_audioPlugReceiver != null) RegisterAudioPlugV21(register);
            _audioPlugRegistered = register;
        }

        public virtual int SetRenderer(RendererItem item)
        {
            return nativeSetRenderer(item);
        }

        public virtual bool HasMedia()
        {
            lock (this)
            {
                return _media != null;
            }
        }


        /// <summary>
        ///     Play the media
        /// </summary>
        public virtual void Play()
        {
            lock (this)
            {
                if (!_isPlaying)
                {
                    /* HACK: stop() reset the audio output, so set it again before first play. */
                    if (_isAudioReset)
                    {
                        if (_audioOutput != null) nativeSetAudioOutput(_audioOutput);
                        if (_audioOutputDevice != null) nativeSetAudioOutputDevice(_audioOutputDevice);
                        _isAudioReset = false;
                    }

                    if (!_isAudioDeviceFromUser) RegisterAudioPlug(true);
                    _isPlayRequested = true;
                    if (_window.AreSurfacesWaiting()) return;
                }

                _isPlaying = true;
            }

            nativePlay();
        }

        /// <summary>
        ///     Stops the playing media
        /// </summary>
        public virtual void Stop()
        {
            lock (this)
            {
                _isPlayRequested = false;
                _isPlaying = false;
                _isAudioReset = true;
            }

            nativeStop();
        }


        /// <summary>
        ///     Set if, and how, the video title will be shown when media is played
        /// </summary>
        /// <param name="position"> see <seealso cref="Position" /> </param>
        /// <param name="timeout"> </param>
        public virtual void SetVideoTitleDisplay(int position, int timeout)
        {
            nativeSetVideoTitleDisplay(position, timeout);
        }


        /// <summary>
        ///     Update the video viewpoint information
        /// </summary>
        /// <param name="yaw"> View point yaw in degrees </param>
        /// <param name="pitch"> View point pitch in degrees </param>
        /// <param name="roll">  View point roll in degrees </param>
        /// <param name="fov"> Field of view in degrees (default 80.0f) </param>
        /// <param name="absolute">
        ///     if true replace the old viewpoint with the new one. If false,
        ///     increase/decrease it.
        /// </param>
        /// <returns> true on success. </returns>
        public virtual bool UpdateViewpoint(float yaw, float pitch, float roll, float fov, bool absolute)
        {
            return nativeUpdateViewpoint(yaw, pitch, roll, fov, absolute);
        }

        /// <summary>
        ///     Selects an audio output module.
        ///     Any change will take effect only after playback is stopped and
        ///     restarted. Audio output cannot be changed while playing.
        ///     By default, the "android_audiotrack" is selected. Starting Android 21, passthrough is
        ///     enabled for encodings supported by the device/audio system.
        ///     Calling this method will disable the encoding detection.
        /// </summary>
        /// <returns> true on success. </returns>
        public virtual bool SetAudioOutput(string aout)
        {
            var ret = nativeSetAudioOutput(aout);
            if (ret)
                lock (this)
                {
                    _audioOutput = aout;
                    /* The user forced an output, don't listen to audio plug events and let the user decide */
                    _isAudioDeviceFromUser = true;
                    RegisterAudioPlug(false);
                }

            return ret;
        }

        private bool SetAudioOutputDeviceInternal(string id, bool fromUser)
        {
            var ret = nativeSetAudioOutputDevice(id);
            if (ret)
                lock (this)
                {
                    _audioOutputDevice = id;
                    if (fromUser)
                    {
                        /* The user forced a device, don't listen to audio plug events and let the user decide */
                        _isAudioDeviceFromUser = true;
                        RegisterAudioPlug(false);
                    }
                }

            return ret;
        }

        /// <summary>
        ///     Configures an explicit audio output device.
        ///     Audio output will be moved to the device specified by the device identifier string.
        ///     Available devices for the "android_audiotrack" module (the default) are
        ///     "stereo": Up to 2 channels (compat mode).
        ///     "pcm": Up to 8 channels.
        ///     "encoded": Up to 8 channels, passthrough for every encodings if available.
        ///     "encoded:ENCODING_FLAGS_MASK": passthrough for every encodings specified by
        ///     ENCODING_FLAGS_MASK. This extra value is a long that contains binary-shifted
        ///     AudioFormat.ENCODING_* values.
        ///     Calling this method will disable the encoding detection (see <seealso cref="#setAudioOutput" />).
        /// </summary>
        /// <returns> true on success. </returns>
        public virtual bool SetAudioOutputDevice(string id)
        {
            return SetAudioOutputDeviceInternal(id, true);
        }

        /// <summary>
        ///     Get the full description of available chapters.
        /// </summary>
        /// <param name="title"> index of the title (if -1, use the current title) </param>
        /// <returns> the list of Chapters for the title </returns>
        public virtual Chapter[] GetChapters(int title)
        {
            return nativeGetChapters(title);
        }

        /// <summary>
        ///     Set the video track.
        /// </summary>
        /// <returns> true on success. </returns>
        public virtual bool SetVideoTrack(int index)
        {
            /* Don't activate a video track is surfaces are not ready */
            if (index == -1 || _window.AreViewsAttached() && !_window.AreSurfacesWaiting())
                return nativeSetVideoTrack(index);
            return false;
        }

        /// <summary>
        ///     Set the audio track.
        /// </summary>
        /// <returns> true on success. </returns>
        public virtual bool SetAudioTrack(int index)
        {
            return nativeSetAudioTrack(index);
        }

        /// <summary>
        ///     Set current audio delay. The audio delay will be reset to zero each time the media changes.
        /// </summary>
        /// <param name="delay"> in microseconds. </param>
        /// <returns> true on success. </returns>
        public virtual bool SetAudioDelay(long delay)
        {
            return nativeSetAudioDelay(delay);
        }

        /// <summary>
        ///     Set the spu (subtitle) track.
        /// </summary>
        /// <returns> true on success. </returns>
        public virtual bool SetSpuTrack(int index)
        {
            return nativeSetSpuTrack(index);
        }

        /// <summary>
        ///     Set current spu (subtitle) delay. The spu delay will be reset to zero each time the media changes.
        /// </summary>
        /// <param name="delay"> in microseconds. </param>
        /// <returns> true on success. </returns>
        public virtual bool SetSpuDelay(long delay)
        {
            return nativeSetSpuDelay(delay);
        }

	    /// <summary>
	    ///     Apply new equalizer settings to a media player.
	    ///     The equalizer is first created by invoking <seealso cref="Equalizer#create()" /> or
	    ///     <seealso cref="Equalizer#createFromPreset(int)" />}.
	    ///     It is possible to apply new equalizer settings to a media player whether the media
	    ///     player is currently playing media or not.
	    ///     Invoking this method will immediately apply the new equalizer settings to the audio
	    ///     output of the currently playing media if there is any.
	    ///     If there is no currently playing media, the new equalizer settings will be applied
	    ///     later if and when new media is played.
	    ///     Equalizer settings will automatically be applied to subsequently played media.
	    ///     To disable the equalizer for a media player invoke this method passing null.
	    /// </summary>
	    /// <returns> true on success. </returns>
	    public virtual bool SetEqualizer(Equalizer equalizer)
        {
            return nativeSetEqualizer(equalizer);
        }

        /// <summary>
        ///     Add a slave (or subtitle) to the current media player.
        /// </summary>
        /// <param name="type"> see <seealso cref="Slave.Type" /> </param>
        /// <param name="uri"> a valid RFC 2396 Uri </param>
        /// <param name="select"></param>
        /// <returns> true on success. </returns>
        public virtual bool AddSlave(int type, Uri uri, bool select)
        {
            return nativeAddSlave(type, VLCUtil.EncodeVLCUri(uri), select);
        }

        /// <summary>
        ///     Add a slave (or subtitle) to the current media player.
        /// </summary>
        /// <param name="type"> see <seealso cref="Slave.Type" /> </param>
        /// <param name="path"> a local path </param>
        /// <returns> true on success. </returns>
        public virtual bool AddSlave(int type, string path, bool select)
        {
            return AddSlave(type, Uri.FromFile(new File(path)), select);
        }

        /// <summary>
        ///     Sets the speed of playback (1 being normal speed, 2 being twice as fast)
        /// </summary>
        /// <param name="rate"> </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void setRate(float rate);

        /// <summary>
        ///     Get the current playback speed
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern float getRate();

        /// <summary>
        ///     Returns true if any media is playing
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern bool isPlaying();


        /// <summary>
        ///     Returns true if any media is seekable
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern bool isSeekable();

        /// <summary>
        ///     Pauses any playing media
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void pause();

        /// <summary>
        ///     Get player state.
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int getPlayerState();

        /// <summary>
        ///     Gets volume as integer
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int getVolume();

        /// <summary>
        ///     Sets volume as integer
        /// </summary>
        /// <param name="volume">: Volume level passed as integer </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int setVolume(int volume);

        /// <summary>
        ///     Gets the current movie time (in ms).
        /// </summary>
        /// <returns> the movie time (in ms), or -1 if there is no media. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern long getTime();

        /// <summary>
        ///     Sets the movie time (in ms), if any media is being played.
        /// </summary>
        /// <param name="time">: Time in ms. </param>
        /// <returns> the movie time (in ms), or -1 if there is no media. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern long setTime(long time);

        /// <summary>
        ///     Gets the movie position.
        /// </summary>
        /// <returns> the movie position, or -1 for any error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern float getPosition();

        /// <summary>
        ///     Sets the movie position.
        /// </summary>
        /// <param name="pos">: movie position. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void setPosition(float pos);

        /// <summary>
        ///     Gets current movie's length in ms.
        /// </summary>
        /// <returns> the movie length (in ms), or -1 if there is no media. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern long getLength();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int getTitle();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void setTitle(int title);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int getChapter();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int previousChapter();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern int nextChapter();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void setChapter(int chapter);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        public extern void navigate(int navigate);

        protected override MediaPlayerEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            {
                lock (this)
                {
                    switch (eventType)
                    {
                        case MediaPlayerEvent.MediaChanged:
                        case MediaPlayerEvent.Stopped:
                        case MediaPlayerEvent.EndReached:
                        case MediaPlayerEvent.EncounteredError:
                            _voutCount = 0;
                            Monitor.Pulse(this);
                            goto case MediaPlayerEvent.Opening;
                        case MediaPlayerEvent.Opening:
                        case MediaPlayerEvent.Buffering:
                            return new MediaPlayerEvent(eventType, argf1);
                        case MediaPlayerEvent.Playing:
                        case MediaPlayerEvent.Paused:
                            return new MediaPlayerEvent(eventType);
                        case MediaPlayerEvent.TimeChanged:
                            return new MediaPlayerEvent(eventType, arg1);
                        case MediaPlayerEvent.LengthChanged:
                            return new MediaPlayerEvent(eventType, arg1);
                        case MediaPlayerEvent.PositionChanged:
                            return new MediaPlayerEvent(eventType, argf1);
                        case MediaPlayerEvent.Vout:
                            _voutCount = (int) arg1;
                            Monitor.Pulse(this);
                            return new MediaPlayerEvent(eventType, arg1);
                        case MediaPlayerEvent.ESAdded:
                        case MediaPlayerEvent.ESDeleted:
                        case MediaPlayerEvent.ESSelected:
                            return new MediaPlayerEvent(eventType, arg1, arg2);
                        case MediaPlayerEvent.SeekableChanged:
                        case MediaPlayerEvent.PausableChanged:
                            return new MediaPlayerEvent(eventType, arg1);
                    }

                    return null;
                }
            }
        }

        protected override void OnReleaseNative()
        {
            RegisterAudioPlug(false);

            _media?.Release();
            _voutCount = 0;
            nativeRelease();
        }

        private static Title CreateTitleFromNative(long duration, string name, int flags)
        {
            return new Title(duration, name, flags);
        }

        private static Chapter CreateChapterFromNative(long timeOffset, long duration, string name)
        {
            // Used from JNI
            return new Chapter(timeOffset, duration, name);
        }

        private static TrackDescription CreateTrackDescriptionFromNative(int id, string name)
        {
            // Used from JNI
            return new TrackDescription(id, name);
        }


        /* JNI */
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromLibVlc(LibVlc libVLC, AWindow window);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeNewFromMedia(Media.Media media, AWindow window);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeRelease();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeSetMedia(Media.Media media);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativePlay();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeStop();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeSetRenderer(RendererItem item);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeSetVideoTitleDisplay(int position, int timeout);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern float nativeGetScale();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeSetScale(float scale);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern string nativeGetAspectRatio();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeSetAspectRatio(string aspect);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeUpdateViewpoint(float yaw, float pitch, float roll, float fov, bool absolute);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetAudioOutput(string aout);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetAudioOutputDevice(string id);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern Title[] nativeGetTitles();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern Chapter[] nativeGetChapters(int title);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetVideoTracksCount();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern TrackDescription[] nativeGetVideoTracks();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetVideoTrack();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetVideoTrack(int index);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetAudioTracksCount();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern TrackDescription[] nativeGetAudioTracks();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetAudioTrack();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetAudioTrack(int index);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern long nativeGetAudioDelay();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetAudioDelay(long delay);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetSpuTracksCount();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern TrackDescription[] nativeGetSpuTracks();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern int nativeGetSpuTrack();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetSpuTrack(int index);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern long nativeGetSpuDelay();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetSpuDelay(long delay);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeAddSlave(int type, string location, bool select);

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern bool nativeSetEqualizer(Equalizer equalizer);

        private class AWindowSurfaceCallback : ISurfaceCallback
        {
            private readonly MediaPlayer _mediaPlayer;

            public AWindowSurfaceCallback(MediaPlayer mediaPlayer)
            {
                _mediaPlayer = mediaPlayer;
            }

            public void OnSurfacesCreated(AWindow vout)
            {
                var play = false;
                var enableVideo = false;
                lock (this)
                {
                    if (!_isPlaying && _isPlayRequested)
                        play = true;
                    else if (_voutCount == 0) enableVideo = true;
                }

                if (play)
                    _mediaPlayer.Play();
                else if (enableVideo) _mediaPlayer.VideoTrackEnabled = true;
            }

            public void OnSurfacesDestroyed(AWindow vout)
            {
                var disableVideo = false;
                lock (this)
                {
                    if (_voutCount > 0) disableVideo = true;
                }

                if (disableVideo) _mediaPlayer.VideoTrackEnabled = false;
            }
        }

        private class AudioPlugReceiver : BroadcastReceiver
        {
            private readonly MediaPlayer _mediaPlayer;

            public AudioPlugReceiver(MediaPlayer mediaPlayer)
            {
                _mediaPlayer = mediaPlayer;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                var action = intent.Action;
                if (ReferenceEquals(action, null)) return;
                if (action.Equals(AudioManager.ActionHdmiAudioPlug, StringComparison.CurrentCultureIgnoreCase))
                {
                    var hasHdmi = intent.GetIntExtra(AudioManager.ExtraAudioPlugState, 0) == 1;
                    var encodingFlags =
                        !hasHdmi ? 0 : GetEncodingFlags(intent.GetIntArrayExtra(AudioManager.ExtraEncodings));
                    _mediaPlayer.UpdateAudioOutputDevice(encodingFlags, "stereo");
                }
            }
        }

        [TargetApi(Value = (int) BuildVersionCodes.M)]
        private class MediaPlayerAudioDeviceCallback : AudioDeviceCallback
        {
            private readonly MediaPlayer _mediaPlayer;
            private readonly SparseArray<long> _mEncodedDevices = new SparseArray<long>();

            public MediaPlayerAudioDeviceCallback(MediaPlayer mediaPlayer)
            {
                _mediaPlayer = mediaPlayer;
            }

            private void OnAudioDevicesChanged()
            {
                long encodingFlags = 0;
                for (var i = 0; i < _mEncodedDevices.Size(); ++i) encodingFlags |= _mEncodedDevices.ValueAt(i);

                _mediaPlayer.UpdateAudioOutputDevice(encodingFlags, "pcm");
            }

            public override void OnAudioDevicesAdded(AudioDeviceInfo[] addedDevices)
            {
                foreach (var info in addedDevices)
                {
                    if (!info.IsSink) continue;

                    var encodings = info.GetEncodings().Select(e => (int) e).ToArray();

                    var encodingFlags = GetEncodingFlags(encodings);
                    if (encodingFlags != 0) _mEncodedDevices.Put(info.Id, encodingFlags);
                }

                OnAudioDevicesChanged();
            }

            public override void OnAudioDevicesRemoved(AudioDeviceInfo[] removedDevices)
            {
                foreach (var info in removedDevices)
                {
                    if (!info.IsSink) continue;
                    _mEncodedDevices.Remove(info.Id);
                }

                OnAudioDevicesChanged();
            }
        }
    }
}