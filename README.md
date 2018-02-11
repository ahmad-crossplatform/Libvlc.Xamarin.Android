# Libvlc.Xamarin.Android

A project to port [libvlc for android](https://code.videolan.org/videolan/vlc-android/tree/master/libvlc/src/org/videolan/libvlc)
from Java to C# 



## Classes to be Implemented (In Util) 
1. **Util/Dumper** : Depends on 
  * Libvlc.LibVlc
  * Libvlc.Media
  * Libvlc.MediaPlayer

2. **Util/MediaBrowser** Depends on 
  * Libvlc.LibVlc
  * Libvlc.Media
  * Libvlc.MediaDiscoverer
  * Libvlc.MediaList

3. **Util/VLCUtil** : Completed however GetThumbnail is not implemented, as it depends  
  * Libvlc.LibVlc
  * Libvlc.Media


## Classes to be Implemented in Root Folder 

1. AWindow 
2. Dialog
3. ~~IVLCOut(Done)~~
4. LibVlc
5. Media
6. MediaDiscoverer
7. MediaList
8. MediaPlayer
9. ~~VLCEvent (Done)~~
10. VlCObject  (needs a dllImprt for NativeDetachEvents)

