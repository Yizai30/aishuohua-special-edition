package com.setik.androidutils;

import static android.os.Process.myPid;

import com.github.anrwatchdog.ANRError;
import com.github.anrwatchdog.ANRWatchDog;
import com.github.asvid.remotelogger.Config;
import com.github.asvid.remotelogger.RemoteLogger;
import android.Manifest;
import android.app.Activity;
import android.app.ActivityManager;
import android.app.Application;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.AudioFormat;
import android.media.AudioRecord;
import android.media.MediaRecorder;
import android.media.MediaScannerConnection;
import android.media.projection.MediaProjectionManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.provider.MediaStore;
import android.provider.Settings;
import android.util.Log;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.DrawableRes;
import androidx.annotation.RequiresApi;

//import com.hbisoft.hbrecorder_custom.HBRecorderListener;
import com.hbisoft.hbrecorder.HBRecorder;
import com.hbisoft.hbrecorder.HBRecorderListener;
import com.iflytek.cloud.ErrorCode;
import com.iflytek.cloud.InitListener;
import com.iflytek.cloud.RecognizerListener;
import com.iflytek.cloud.RecognizerResult;
import com.iflytek.cloud.SpeechConstant;
import com.iflytek.cloud.SpeechError;
import com.iflytek.cloud.SpeechRecognizer;
import com.iflytek.cloud.SpeechUtility;
import com.iflytek.cloud.util.ResourceUtil;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
//
//import logcatsender.LoggingService;

public class AndroidUtils extends UnityPlayerActivity implements HBRecorderListener {
    private static final int SCREEN_RECORD_REQUEST_CODE = 777;
    private static final int PERMISSION_REQ_ID_RECORD_AUDIO = 22;
    private static final int PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE = PERMISSION_REQ_ID_RECORD_AUDIO + 1;
    private String mGameObject = "AndroidUtils", saveFolder = "TellStory";
    public static String filename = "ScreenCapture.mp4";
    HBRecorder hbRecorder;
    ContentValues contentValues;
    Uri mUri;
    ContentResolver resolver;
    boolean customSetting = false;
    boolean successfullyGenerated = false;

    private String getCurrentProcessName(Context context) {
        int pid = android.os.Process.myPid();
        ActivityManager activityManager = (ActivityManager) context.getSystemService(Context.ACTIVITY_SERVICE);
        if (activityManager == null) {
            return null;
        }
        for (ActivityManager.RunningAppProcessInfo appProcess : activityManager.getRunningAppProcesses()) {
            if (appProcess.pid == pid) {
                return appProcess.processName;
            }
        }
        return null;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        hbRecorder = new HBRecorder(this, this);

        SpeechUtility.createUtility(this, SpeechConstant.APPID + "=024df79d");

        mIat = SpeechRecognizer.createRecognizer(this, code -> {
            Log.d("Unity", "SpeechRecognizer init() code = " + code);
            if (code != ErrorCode.SUCCESS) {
                Log.e("Unity", "SpeechRecognizer init failed");
            }
        });

        mIat.setParameter(SpeechConstant.CLOUD_GRAMMAR, null );
        mIat.setParameter(SpeechConstant.SUBJECT, null );
        mIat.setParameter(SpeechConstant.RESULT_TYPE, "plain");
        mIat.setParameter(SpeechConstant.ENGINE_TYPE, "cloud");
        mIat.setParameter(SpeechConstant.LANGUAGE, "zh_cn");
        mIat.setParameter(SpeechConstant.ACCENT, "mandarin");
        mIat.setParameter(SpeechConstant.VAD_BOS, "10000");
        mIat.setParameter(SpeechConstant.VAD_EOS, "1000");
        mIat.setParameter(SpeechConstant.ASR_PTT,"0");
        String dataLogPath = this.getExternalFilesDir(null).getAbsolutePath() + "/msc/";
        tempFilePath = this.getExternalFilesDir(null).getAbsolutePath() + "/msc/" + "iat.pcm";
        mIat.setParameter(SpeechConstant.AUDIO_FORMAT, "pcm");
        mIat.setParameter(SpeechConstant.ASR_AUDIO_PATH,
                getExternalFilesDir("msc").getAbsolutePath() + "/iat.pcm");
        File dataFolder = new File(dataLogPath);
        if (!dataFolder.exists()) {
            Log.i("Unity","创建文件夹：" + dataLogPath);
            boolean result = dataFolder.mkdirs();
            Log.i("Unity","创建文件夹" + (result ? "成功": "失败"));
        }

        VoiceFileManager.Init(this);

        mPresenter = new TellStoryVoicePresenter(this);
    }

    @Override
    public void HBRecorderOnStart() {
        Log.e("HBRecorder", "HBRecorderOnStart called");
        UnityPlayer.UnitySendMessage(this.mGameObject, "VideoRecorderCallback", "start_record");
    }

    @Override
    public synchronized void HBRecorderOnComplete() {
        Log.i("Unity", "HBRecorderOnComplete...");
        successfullyGenerated = true;
        this.notify();
        //UnityPlayer.UnitySendMessage(this.mGameObject, "VideoRecorderCallback", "stop_record");
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private void refreshGalleryFile() {
        MediaScannerConnection.scanFile(this,
                new String[]{hbRecorder.getFilePath()}, null,
                new MediaScannerConnection.OnScanCompletedListener() {
                    public void onScanCompleted(String path, Uri uri) {
                        Log.i("ExternalStorage", "Scanned " + path + ":");
                        Log.i("ExternalStorage", "-> uri=" + uri);
                    }
                });
    }

    private void updateGalleryUri() {
        contentValues.clear();
        contentValues.put(MediaStore.Video.Media.IS_PENDING, 0);
        getContentResolver().update(mUri, contentValues, null, null);
    }

    @Override
    public synchronized void HBRecorderOnError(int errorCode, String reason) {
        // Error 38 happens when
        // - the selected video encoder is not supported
        // - the output format is not supported
        // - if another app is using the microphone
        //It is best to use device default
        if (errorCode == 38) {
            showLongToast("Some settings are not supported by your device");
        } else {
            showLongToast("HBRecorderOnError - See Log");
        }
        Log.e("HBRecorderOnError", reason);
        successfullyGenerated = true;
        this.notify();
        UnityPlayer.UnitySendMessage(this.mGameObject, "VideoRecorderCallback", "init_record_error");
    }

    public void setUpSaveFolder(String folderName) {
        this.saveFolder = folderName;
    }

    public void setFilename(String _filename) {
//        filename = _filename;
//        SharedPreferences sp = getApplication().getSharedPreferences("pref", MODE_PRIVATE);
//        sp.edit().putString("videoFileName", _filename).commit();
        filename = _filename.replace(".mp4", "");
    }

    public void setupVideo(int width, int height, int bitRate, int fps, boolean audioEnabled, String encoder) {
        hbRecorder.enableCustomSettings();
        hbRecorder.setScreenDimensions(height, width);
        hbRecorder.setVideoFrameRate(fps);
        hbRecorder.setVideoBitrate(bitRate);
        hbRecorder.setVideoEncoder(encoder);
        hbRecorder.recordHDVideo(false);
//        hbRecorder.setAudioBitrate(128000);
//        hbRecorder.setAudioSamplingRate(44100);
        hbRecorder.isAudioEnabled(audioEnabled);
        //hbRecorder.isAudioEnabled(false); // Disable if remux on server
        customSetting = true;

        if (Build.VERSION.SDK_INT >= 23) {
            int REQUEST_CODE_PERMISSION_STORAGE = 100;
            String[] permissions = {
                    Manifest.permission.READ_EXTERNAL_STORAGE,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE
            };
            for (String str : permissions) {
                if (this.checkSelfPermission(str) != PackageManager.PERMISSION_GRANTED) {
                    this.requestPermissions(permissions, REQUEST_CODE_PERMISSION_STORAGE);
                    return;
                }
            }
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public void startRecording() {
        MediaProjectionManager mediaProjectionManager = (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);
        Intent permissionIntent = mediaProjectionManager != null ? mediaProjectionManager.createScreenCaptureIntent() : null;
        startActivityForResult(permissionIntent, SCREEN_RECORD_REQUEST_CODE);
    }

    public synchronized void stopRecording() {
        //if((audioManager.getMode()== AudioManager.MODE_IN_CALL)||(audioManager.getMode()== AudioManager.MODE_IN_COMMUNICATION)){
        //    audioManager.setMicrophoneMute(false);
        //}
        successfullyGenerated = false;
        hbRecorder.stopScreenRecording();
        Log.i("Unity", "stopRecording...");
        try {
            while (!successfullyGenerated) {this.wait();}
        } catch (Exception e) {
            Log.i("Unity", "wait failed: " + e.toString());
        }
        //UnityPlayer.UnitySendMessage(this.mGameObject, "VideoRecorderCallback", "stop_record");
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == SCREEN_RECORD_REQUEST_CODE) {
            if (resultCode == RESULT_OK) {
                setOutputPath();
                Log.i("Unity", "startRecording...");
                //Start screen recording
                //UnityPlayer.UnitySendMessage(mGameObject, "VideoRecorderCallback", "start_record");
                hbRecorder.startScreenRecording(data, resultCode);
            }
        }
    }

    //For Android 10> we will pass a Uri to HBRecorder
    //This is not necessary - You can still use getExternalStoragePublicDirectory
    //But then you will have to add android:requestLegacyExternalStorage="true" in your Manifest
    //IT IS IMPORTANT TO SET THE FILE NAME THE SAME AS THE NAME YOU USE FOR TITLE AND DISPLAY_NAME
    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    private void setOutputPath() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            resolver = getContentResolver();
            contentValues = new ContentValues();
            contentValues.put(MediaStore.Video.Media.RELATIVE_PATH, "Movies/" + saveFolder);
            contentValues.put(MediaStore.Video.Media.TITLE, filename);
            contentValues.put(MediaStore.MediaColumns.DISPLAY_NAME, filename);
            contentValues.put(MediaStore.MediaColumns.MIME_TYPE, "video/mp4");
            mUri = resolver.insert(MediaStore.Video.Media.EXTERNAL_CONTENT_URI, contentValues);
            //FILE NAME SHOULD BE THE SAME
            hbRecorder.setFileName(filename);
            hbRecorder.setOutputUri(mUri);
        } else {
            createFolder();
            hbRecorder.setOutputPath(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MOVIES) + "/" + saveFolder);
            hbRecorder.setFileName(filename);
            //hbRecorder.setOutputPath(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM) + "/" + saveFolder);
        }
        Log.i("Unity", "using filename " + filename + " for recorded files");
    }

    //Create Folder
    //Only call this on Android 9 and lower (getExternalStoragePublicDirectory is deprecated)
    //This can still be used on Android 10> but you will have to add android:requestLegacyExternalStorage="true" in your Manifest
    private void createFolder() {
        File f1 = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MOVIES), saveFolder);
        //File f1 = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), saveFolder);
        if (!f1.exists()) {
            if (f1.mkdirs()) {
                Log.i("Folder ", "created");
            }
        }
    }

    //Generate a timestamp to be used as a file name
    private String generateFileName() {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd-HH-mm-ss", Locale.getDefault());
        Date curDate = new Date(System.currentTimeMillis());
        return formatter.format(curDate).replace(" ", "");
    }

    private static void showLongToast(final String msg) {
        Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(), msg, Toast.LENGTH_LONG).show();
    }

    private byte[] drawable2ByteArray(@DrawableRes int drawableId) {
        Bitmap icon = BitmapFactory.decodeResource(getResources(), drawableId);
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        icon.compress(Bitmap.CompressFormat.PNG, 100, stream);
        return stream.toByteArray();
    }

    @RequiresApi(api = Build.VERSION_CODES.M)
    public void requestPermission(String permissionStr) {
        if ((!hasPermission(permissionStr)) && (android.os.Build.VERSION.SDK_INT >= 23)) {
            UnityPlayer.currentActivity.requestPermissions(new String[]{permissionStr}, 0);
        }
    }

    public boolean hasPermission(String permissionStr) {
        if (android.os.Build.VERSION.SDK_INT < 23)
            return true;
        Context context = UnityPlayer.currentActivity.getApplicationContext();
        return context.checkCallingOrSelfPermission(permissionStr) == PackageManager.PERMISSION_GRANTED;
    }

    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        switch (requestCode) {
            case 0:
                if (grantResults[0] == 0) {
                    UnityPlayer.UnitySendMessage(mGameObject, "OnAllow", "");
                } else if (android.os.Build.VERSION.SDK_INT >= 23) {
                    if (shouldShowRequestPermissionRationale(permissions[0])) {
                        UnityPlayer.UnitySendMessage(mGameObject, "OnDeny", "");
                    } else {
                        UnityPlayer.UnitySendMessage(mGameObject, "OnDenyAndNeverAskAgain", "");
                    }
                }
                break;
        }
    }


    private static SpeechRecognizer mIat;
    private static String tempFilePath;

    public static void recognizeInit() {
        String androidId = Settings.Secure.getString(UnityPlayer.currentActivity.getContentResolver(),
                Settings.Secure.ANDROID_ID);
        String logpath = UnityPlayer.currentActivity.getExternalFilesDir("Logs").getAbsolutePath() + "/" + androidId + "_" + myPid() + ".txt";
        Log.d("Unity", "logpath: " + logpath);
        new RemoteLogger().initialize(new Config(
                "43.129.88.236", // your server IP, will be displayed in CMD after server is started
                1234, // optional port
                UnityPlayer.currentActivity.getApplicationInfo().packageName, // package name, required for proper LogCat filterin)
                androidId,
                logpath
        ));



        new ANRWatchDog().setANRListener(new ANRWatchDog.ANRListener() {
            @Override
            public void onAppNotResponding(ANRError error) {
                // Save ANR log to file
                try {
                    File file = new File(UnityPlayer.currentActivity.getExternalFilesDir("Logs")
                            .getAbsolutePath() + "/anr_" + androidId + "_" + myPid() + ".txt");
                    if (!file.exists()) {
                        try {
                            file.createNewFile();
                        } catch (IOException e) {
                            throw new RuntimeException(e);
                        }
                    }
                    PrintWriter pw = new PrintWriter(new FileWriter(file));
                    error.printStackTrace(pw);
                    pw.flush();
                    pw.close();
                    // TODO: network
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }).setANRInterceptor(new ANRWatchDog.ANRInterceptor() {
            @Override
            public long intercept(long duration) {
                long ret = 15000 - duration;
                if(ret > 0) {
                    Log.e("ANR", "Intercepted ANR that is too short (" +
                            duration + " ms), postponing for " + ret + " ms.");
                }
                return ret;
            }
        }).start();

    }

//    static class NlpListener  { //implements VoiceModuleNlpListener
//        public static String results = "";
//        public static String resultPath = "";
//        //Override
//        public void recognitionVoiceText(String text, String text2) {
//            Log.d("Unity", "语音文本返回=" + text);
//            results += text + "。";
//        }
//
//        //@Override
//        public void audioSourcePath(String path) {
//            Log.d("Unity", "语音源文件路径=" + new File(path).getParent());
//            //resultPath = new File(path).getParent();
//            if (!resultPath.contains(path)) {
//                resultPath += path +  ";";
//            }
//        }
//    };
//
//    public static NlpListener nlpListener = new NlpListener();
    private static TellStoryVoicePresenter mPresenter;
    public static String recognizerResult, recognizerResultPath;
    public static boolean isRecognizing = false;
    public static void startRecognize() {
        isRecognizing = true;
        int ret = mIat.startListening(mRecognizerListener);
        if (ret != ErrorCode.SUCCESS) {
            Log.e("Unity", "识别失败,错误码: " + ret + " (https://www.xfyun.cn/document/error-code)");
        }
        recognizerResult = "";recognizerResultPath = "";
        UnityPlayer.currentActivity.runOnUiThread(()->{
            mPresenter.showAnimaMic();
        });

    }

//    public static String stopRecognizeOld() {
//        //service.getVoiceService().stopVoiceNlpRecognize();
//        try {
//            String originalPaths = nlpListener.resultPath;
//            Log.d("Unity", "转码到wav文件：" + nlpListener.resultPath);
//                String[] fileNames = nlpListener.resultPath.split(";");
//                String pcmFile = WavUtils.CombinePCMFiles(fileNames);
//                if (pcmFile != "") {
//                    String wavFile = pcmFile.replace(".pcm", "_" + System.currentTimeMillis() + ".wav");
//                    WavUtils.PCMToWAV(new File(pcmFile), new File(wavFile));
//                    nlpListener.resultPath = wavFile;
//                } else {
//                    nlpListener.resultPath = "";
//                }
//            return nlpListener.results + "|" + nlpListener.resultPath + "|" + originalPaths;
//        } catch (IOException e) {
//            Log.d("Unity", "转码失败");
//            e.printStackTrace();
//        }
//        return nlpListener.results + "|";
//    }

    public static String stopRecognize() {
        //service.getVoiceService().stopVoiceNlpRecognize();
        isRecognizing = false;

        mIat.stopListening();
        UnityPlayer.currentActivity.runOnUiThread(()->{
            mPresenter.closeAnimaMic();
        });


        try {
            String originalPaths = recognizerResultPath;
            recognizerResultPath = "";
            Log.d("Unity", "转码到wav文件：" + originalPaths);
            String[] fileNames = originalPaths.split(";");
            for (int i=0;i < fileNames.length; i++) {
                if (fileNames[i] == "") continue;
                String pcmFile = fileNames[i];
                String wavFile = pcmFile.replace(".pcm", "_" + System.currentTimeMillis() + ".wav");
                WavUtils.PCMToWAV(new File(pcmFile), new File(wavFile));
                if (i > 0) {
                    recognizerResultPath += ";";
                }
                recognizerResultPath += wavFile;

            }
            return recognizerResult + "|" + recognizerResultPath + "|" + originalPaths;
        } catch (IOException e) {
            Log.d("Unity", "转码失败");
            e.printStackTrace();
        }
        return recognizerResult + "|";
    }

    static AudioRecord audioRecord;
    // 采样率，现在能够保证在所有设备上使用的采样率是44100Hz, 但是其他的采样率（22050, 16000, 11025）在一些设备上也可以使用。
    static final int SAMPLE_RATE_INHZ = 44100;
    // 声道数。CHANNEL_IN_MONO and CHANNEL_IN_STEREO. 其中CHANNEL_IN_MONO是可以保证在所有设备能够使用的。
    static final int CHANNEL_CONFIG = AudioFormat.CHANNEL_IN_STEREO;
    // 返回的音频数据的格式。 ENCODING_PCM_8BIT, ENCODING_PCM_16BIT, and ENCODING_PCM_FLOAT.
    static final int AUDIO_FORMAT = AudioFormat.ENCODING_PCM_16BIT;
    /**
     * 录音的工作线程
     */
    static Thread recordingAudioThread;
    static boolean isRecording = false;//mark if is recording
    static String audioCacheFilePath;
    /**
     * 开始录音，返回临时缓存文件（.pcm）的文件路径
     */
    public static void startRecordAudio(String audioCacheFilePath) {
        AndroidUtils.audioCacheFilePath = audioCacheFilePath;
        try{
            // 获取最小录音缓存大小，
            int minBufferSize = AudioRecord.getMinBufferSize(SAMPLE_RATE_INHZ, CHANNEL_CONFIG, AUDIO_FORMAT);
            audioRecord = new AudioRecord(MediaRecorder.AudioSource.MIC, SAMPLE_RATE_INHZ, CHANNEL_CONFIG, AUDIO_FORMAT, minBufferSize);
            // 开始录音
            isRecording = true;
            audioRecord.startRecording();

            // 创建数据流，将缓存导入数据流
            recordingAudioThread = new Thread(new Runnable() {
                @Override
                public void run() {
                    File file = new File(audioCacheFilePath);
                    File folder = new File(file.getParent());
                    if (!folder.exists()) {
                        folder.mkdirs();
                    }
                    Log.i("Unity", "audio cache pcm file path:" + audioCacheFilePath);
                    /*
                     *  以防万一，看一下这个文件是不是存在，如果存在的话，先删除掉
                     */
                    if (file.exists()) {
                        file.delete();
                    }

                    try {
                        file.createNewFile();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }

                    FileOutputStream fos = null;
                    try {
                        fos = new FileOutputStream(file);
                    } catch (FileNotFoundException e) {
                        e.printStackTrace();
                        Log.e("Unity", "临时缓存文件未找到");
                    }
                    if (fos == null) {
                        return;
                    }

                    byte[] data = new byte[minBufferSize];
                    int read;
                    if (fos != null) {
                        while (isRecording && !recordingAudioThread.isInterrupted()) {
                            read = audioRecord.read(data, 0, minBufferSize);
                            if (AudioRecord.ERROR_INVALID_OPERATION != read) {
                                try {
                                    fos.write(data);
                                    Log.i("audioRecordTest", "写录音数据->" + read);
                                } catch (IOException e) {
                                    e.printStackTrace();
                                }
                            }
                        }
                    }

                    try {
                        // 关闭数据流
                        fos.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            });
            recordingAudioThread.start();
            subtitleListener.results = "";
            //service.getVoiceService().startVoiceNlpRecognize(subtitleListener);
            subtitleListener.beginTime = System.currentTimeMillis();
        }
        catch(IllegalStateException e){
            Log.w("Unity","需要获取录音权限！");
            checkIfNeedRequestRunningPermission();
        }
        catch(SecurityException e){
            Log.w("Unity","需要获取录音权限！");
            checkIfNeedRequestRunningPermission();
        }
    }

    /**
     * 停止录音
     */
    public static String stopRecordAudio(String outputPath){
        try {
            //service.getVoiceService().stopVoiceNlpRecognize();
            isRecording = false;
            if (audioRecord != null) {
                audioRecord.stop();
                audioRecord.release();
                audioRecord = null;
                recordingAudioThread.interrupt();
                recordingAudioThread = null;
            }
            PcmToWavUtil ptwUtil = new PcmToWavUtil();
            ptwUtil.pcmToWav(AndroidUtils.audioCacheFilePath, outputPath,false);
        }
        catch (Exception e){
            Log.w("Unity", e.getLocalizedMessage());
        }
        Log.w("Unity","{" + subtitleListener.results + "}");
        return "{" + subtitleListener.results + "}";
    }

    static void checkIfNeedRequestRunningPermission() {
        //TODO
    }


    static class SubtitleListener  { //implements VoiceModuleNlpListener
        public static long beginTime;
        public static String results = "";

        //@Override
        public void recognitionVoiceText(String text, String text2) {
            Log.d("Unity", "语音文本返回=" + text);
            results += "\"" +  (System.currentTimeMillis() - beginTime) + "\": \"" + text + "。\",";
        }

        //@Override
        public void audioSourcePath(String path) {}
    };


    public static SubtitleListener subtitleListener = new SubtitleListener();


    private static RecognizerListener mRecognizerListener = new RecognizerListener() {

        @Override
        public void onVolumeChanged(int volume, byte[] data) {}

        private StringBuffer tempResult = new StringBuffer();
        @Override
        public void onResult(final RecognizerResult result, boolean isLast) {
            if (null != result) {
                tempResult.append(result.getResultString());
            }
            if (isLast) {
                String resultText = tempResult.toString();
                tempResult = new StringBuffer();
                File pcmFile = new File(tempFilePath);
                Log.d("Unity", "onResult tempFilePath: " + tempFilePath);
                Log.d("Unity", "onResult size: " + pcmFile.length());
                recognizerResult += resultText + "。";
                Log.d("Unity", "onResult getResultString: " + resultText);
                recognizerResultPath += VoiceFileManager.addVoiceFile(tempFilePath) + ";";
                if (mPresenter != null) UnityPlayer.currentActivity.runOnUiThread(()->{
                    mPresenter.display(resultText);
                });
            }

            if (isLast && isRecognizing) {
                Log.e("Unity", "reopen");
                mIat.stopListening();
                int ret = mIat.startListening(mRecognizerListener);
                if (ret != ErrorCode.SUCCESS) {
                    Log.e("Unity", "识别失败,错误码: " + ret + " (https://www.xfyun.cn/document/error-code)");
                }
            }
        }

        @Override
        public void onError(SpeechError speechError) {
            Log.e("Unity", "onError: " + speechError.toString());
        }
        @Override
        public void onEndOfSpeech() {}
        @Override
        public void onBeginOfSpeech() {}
        @Override
        public void onEvent(int eventType, int arg1, int arg2, Bundle obj) {
        }
    };
}
