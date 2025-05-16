package com.setik.androidutils;

import static android.Manifest.permission.RECORD_AUDIO;
import static android.Manifest.permission.WRITE_EXTERNAL_STORAGE;
import static android.media.MediaFormat.MIMETYPE_AUDIO_AAC;
import static android.media.MediaFormat.MIMETYPE_VIDEO_AVC;
import static android.os.Build.VERSION_CODES.M;

import android.annotation.TargetApi;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.ActivityNotFoundException;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.graphics.Point;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.MediaCodecInfo;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Looper;
import android.os.StrictMode;
import android.util.Log;
import android.util.Range;
import android.widget.ArrayAdapter;
import android.widget.SpinnerAdapter;
import android.widget.Toast;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentActivity;

import com.hbisoft.hbrecorder.R;
import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import screenrecorder.AudioEncodeConfig;
import screenrecorder.PermissionRequestActivity;
import screenrecorder.ScreenRecorder;
import screenrecorder.Utils;
import screenrecorder.VideoEncodeConfig;

public class ScreenRecord {
    public static void startScreenRecord() {
        if (hasPermissions()) {
            if (mMediaProjection == null) {
                requestMediaProjection();
            } else {
                startCapturing(mMediaProjection);
            }
        } else if (Build.VERSION.SDK_INT >= M) {
            requestPermissions();
        }
    }

    public static void stopScreenRecord() {
        stopRecordingAndOpenFile(UnityPlayer.currentActivity);
    }

    private static final int REQUEST_PERMISSIONS = 2;
    @TargetApi(M)
    private static void requestPermissions() {
        String[] permissions = new String[]{WRITE_EXTERNAL_STORAGE, RECORD_AUDIO};
        boolean showRationale = false;
        for (String perm : permissions) {
            showRationale |= UnityPlayer.currentActivity.shouldShowRequestPermissionRationale(perm);
        }
        if (!showRationale) {
            UnityPlayer.currentActivity.requestPermissions(permissions, REQUEST_PERMISSIONS);
            return;
        }
        new AlertDialog.Builder(UnityPlayer.currentActivity)
                .setMessage("Using your mic to record audio and your sd card to save video file")
                .setCancelable(false)
                .setPositiveButton(android.R.string.ok, (dialog, which) ->
                        UnityPlayer.currentActivity.requestPermissions(permissions, REQUEST_PERMISSIONS))
                .setNegativeButton(android.R.string.cancel, null)
                .create()
                .show();
    }



//    @Override
//    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
//        if (requestCode == REQUEST_MEDIA_PROJECTION) {
//            // NOTE: Should pass this result data into a Service to run ScreenRecorder.
//            // The following codes are merely exemplary.
//
//            MediaProjection mediaProjection = mMediaProjectionManager.getMediaProjection(resultCode, data);
//            if (mediaProjection == null) {
//                Log.e("@@", "media projection is null");
//                return;
//            }
//
//            mMediaProjection = mediaProjection;
//            mMediaProjection.registerCallback(mProjectionCallback, new Handler());
//            startCapturing(mediaProjection);
//        }
//    }



    static final MediaProjection[] mediaProjection = new MediaProjection[1];
    private static void requestMediaProjection() {
        final MediaProjectionManager mediaProjectionManager =
                UnityPlayer.currentActivity.getSystemService(MediaProjectionManager.class);
        UnityPlayer.currentActivity.startActivity(new Intent(UnityPlayer.currentActivity, PermissionRequestActivity.class));
    }

    public static MediaProjection.Callback mProjectionCallback = new MediaProjection.Callback() {
        @Override
        public void onStop() {
            if (mRecorder != null) {
                stopRecorder();
            }
        }
    };

    public static MediaProjectionManager mMediaProjectionManager;
    public static MediaProjection mMediaProjection;


    private static void toast(String message, Object... args) {

        int length_toast = Locale.getDefault().getCountry().equals("BR") ? Toast.LENGTH_LONG : Toast.LENGTH_SHORT;
        // In Brazilian Portuguese this may take longer to read

        Toast toast = Toast.makeText(UnityPlayer.currentActivity,
                (args.length == 0) ? message : String.format(Locale.US, message, args),
                length_toast);
        if (Looper.myLooper() != Looper.getMainLooper()) {
            UnityPlayer.currentActivity.runOnUiThread(toast::show);
        } else {
            toast.show();
        }
    }
    private static ScreenRecorder mRecorder;
    private static File getSavingDir() {
        return new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MOVIES),
                "Screenshots");
    }

    private static void stopRecorder() {
        if (mRecorder != null) {
            mRecorder.quit();
        }
        mRecorder = null;
        try {
            UnityPlayer.currentActivity.unregisterReceiver(mStopActionReceiver);
        } catch (Exception e) {
            //ignored
        }
    }

    private static BroadcastReceiver mStopActionReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (ACTION_STOP.equals(intent.getAction())) {
                stopRecordingAndOpenFile(context);
            }
        }
    };

    private static void stopRecordingAndOpenFile(Context context) {
        File file = new File(mRecorder.getSavedPath());
        stopRecorder();
        Toast.makeText(context, "Recorder stopped!  Saved file" + file, Toast.LENGTH_LONG).show();
        StrictMode.VmPolicy vmPolicy = StrictMode.getVmPolicy();
        try {
            // disable detecting FileUriExposure on public file
            StrictMode.setVmPolicy(new StrictMode.VmPolicy.Builder().build());
            viewResult(file);
        } finally {
            StrictMode.setVmPolicy(vmPolicy);
        }
    }

    private static void viewResult(File file) {
        Intent view = new Intent(Intent.ACTION_VIEW);
        view.addCategory(Intent.CATEGORY_DEFAULT);
        view.setDataAndType(Uri.fromFile(file), VIDEO_AVC);
        view.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        try {
            UnityPlayer.currentActivity.startActivity(view);
        } catch (ActivityNotFoundException e) {
            // no activity can open this video
        }
    }

    private static void cancelRecorder() {
        if (mRecorder == null) return;
        Toast.makeText(UnityPlayer.currentActivity, "Permission denied! Screen recorder is cancel", Toast.LENGTH_SHORT).show();
        stopRecorder();
    }

    private static boolean hasPermissions() {
        PackageManager pm = UnityPlayer.currentActivity.getPackageManager();
        String packageName = UnityPlayer.currentActivity.getPackageName();
        int granted = pm.checkPermission(RECORD_AUDIO, packageName)
                | pm.checkPermission(WRITE_EXTERNAL_STORAGE, packageName);
        return granted == PackageManager.PERMISSION_GRANTED;
    }


    public static void startCapturing(MediaProjection mediaProjection) {
        VideoEncodeConfig video = createVideoConfig();
        AudioEncodeConfig audio = createAudioConfig(); // audio can be null
        if (video == null) {
            toast("Create ScreenRecorder failure");
            return;
        }

        File dir = getSavingDir();
        if (!dir.exists() && !dir.mkdirs()) {
            cancelRecorder();
            return;
        }
        SimpleDateFormat format = new SimpleDateFormat("yyyyMMdd-HHmmss", Locale.US);
        final File file = new File(dir, "Screenshots-" + format.format(new Date())
                + "-" + video.width + "x" + video.height + ".mp4");
        Log.d("@@", "Create recorder with :" + video + " \n " + audio + "\n " + file);
        mRecorder = newRecorder(mediaProjection, video, audio, file);
        if (hasPermissions()) {
            startRecorder();
        } else {
            cancelRecorder();
        }
    }

    private static void startRecorder() {
        if (mRecorder == null) return;
        mRecorder.start();
        UnityPlayer.currentActivity.registerReceiver(mStopActionReceiver, new IntentFilter(ACTION_STOP));
        UnityPlayer.currentActivity.moveTaskToBack(true);
    }

    private static VirtualDisplay mVirtualDisplay;
    private static VirtualDisplay getOrCreateVirtualDisplay(MediaProjection mediaProjection, VideoEncodeConfig config) {
        if (mVirtualDisplay == null) {
            mVirtualDisplay = mediaProjection.createVirtualDisplay("ScreenRecorder-display0",
                    config.width, config.height, 1 /*dpi*/,
                    DisplayManager.VIRTUAL_DISPLAY_FLAG_PUBLIC,
                    null /*surface*/, null, null);
        } else {
            // resize if size not matched
            Point size = new Point();
            mVirtualDisplay.getDisplay().getSize(size);
            if (size.x != config.width || size.y != config.height) {
                mVirtualDisplay.resize(config.width, config.height, 1);
            }
        }
        return mVirtualDisplay;
    }


    private static ScreenRecorder newRecorder(MediaProjection mediaProjection, VideoEncodeConfig video,
                                       AudioEncodeConfig audio, File output) {
        final VirtualDisplay display = getOrCreateVirtualDisplay(mediaProjection, video);
        ScreenRecorder r = new ScreenRecorder(video, audio, display, output.getAbsolutePath());
        r.setCallback(new ScreenRecorder.Callback() {
            long startTime = 0;

            @Override
            public void onStop(Throwable error) {
                UnityPlayer.currentActivity.runOnUiThread(() -> stopRecorder());
                if (error != null) {
                    toast("Recorder error ! See logcat for more details");
                    error.printStackTrace();
                    output.delete();
                } else {
                    Intent intent = new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE)
                            .addCategory(Intent.CATEGORY_DEFAULT)
                            .setData(Uri.fromFile(output));
                    UnityPlayer.currentActivity.sendBroadcast(intent);
                }
            }

            @Override
            public void onStart() {
//                mNotifications.recording(0);
            }

            @Override
            public void onRecording(long presentationTimeUs) {
                if (startTime <= 0) {
                    startTime = presentationTimeUs;
                }
                long time = (presentationTimeUs - startTime) / 1000;
//                mNotifications.recording(time);
            }
        });
        return r;
    }
    static final String VIDEO_AVC = MIMETYPE_VIDEO_AVC; // H.264 Advanced Video Coding
    static final String AUDIO_AAC = MIMETYPE_AUDIO_AAC; // H.264 Advanced Audio Coding

    private static int abitrate;
    private static void resetAudioBitrateAdapter(MediaCodecInfo.CodecCapabilities capabilities) {
        Range<Integer> bitrateRange = capabilities.getAudioCapabilities().getBitrateRange();
        int lower = Math.max(bitrateRange.getLower() / 1000, 80);
        int upper = bitrateRange.getUpper() / 1000;
        List<Integer> rates = new ArrayList<>();
        for (int rate = lower; rate < upper; rate += lower) {
            rates.add(rate);
        }
        rates.add(upper);
        abitrate = rates.get((rates.size() / 2));
    }

    private static String acodec;
    private static AudioEncodeConfig createAudioConfig() {
        try {
            Utils.findEncodersByTypeAsync(AUDIO_AAC, infos -> {
                if (infos.length > 0) {
                    acodec = codecInfoNames(infos)[0];
                    MediaCodecInfo.CodecCapabilities capabilities = infos[0].getCapabilitiesForType(AUDIO_AAC);
                    resetAudioBitrateAdapter(capabilities);

                }
            });
        } catch (Exception e) {
            Log.e("Unity", e.getMessage() + "\n" + e);
        }
        if (acodec == null) {
            Log.e("Unity", "acodec not found.");
            return null;
        }
        int bitrate = abitrate;
        int samplerate = 44100;
        int channelCount = 2;
        int profile = 0;

        return new AudioEncodeConfig(acodec, AUDIO_AAC, bitrate, samplerate, channelCount, profile);
    }

    private static String vprofile;
    private static void resetAvcProfileLevelAdapter(MediaCodecInfo.CodecCapabilities capabilities) {
        MediaCodecInfo.CodecProfileLevel[] profiles = capabilities.profileLevels;
        if (profiles == null || profiles.length == 0) {
            return;
        }
        vprofile = Utils.avcProfileLevelToString(profiles[0]);
    }

    private static String vcodec;
    private static VideoEncodeConfig createVideoConfig() {
        try {
            Utils.findEncodersByTypeAsync(VIDEO_AVC, infos -> {
                if (infos.length > 0) {
                    vcodec = codecInfoNames(infos)[0];
                    MediaCodecInfo codec = infos[0];
                    MediaCodecInfo.CodecCapabilities capabilities = codec.getCapabilitiesForType(VIDEO_AVC);
                    resetAvcProfileLevelAdapter(capabilities);
                }
            });
        } catch (Exception e) {
            Log.e("Unity", e.getMessage() + "\n" + e);
        }

        if (vcodec == null) {
            Log.e("Unity", "vcodec not found.");
            return null;
        }
        int[] selectedWithHeight = {1280, 720}; //getSelectedWithHeight();
        boolean isLandscape = false;
        int width = selectedWithHeight[isLandscape ? 0 : 1];
        int height = selectedWithHeight[isLandscape ? 1 : 0];
        int framerate = 30;
        int iframe = 1;
        int bitrate = 800 * 1000;
        MediaCodecInfo.CodecProfileLevel profileLevel = screenrecorder.Utils.toProfileLevel(vprofile);
        return new VideoEncodeConfig(width, height, bitrate,
                framerate, iframe, vcodec, VIDEO_AVC, profileLevel);
    }

    static final String ACTION_STOP = "";

    private static String[] codecInfoNames(MediaCodecInfo[] codecInfos) {
        String[] names = new String[codecInfos.length];
        for (int i = 0; i < codecInfos.length; i++) {
            names[i] = codecInfos[i].getName();
        }
        return names;
    }
}
