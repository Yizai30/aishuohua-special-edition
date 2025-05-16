package com.setik.androidutils;

import android.content.Context;
import android.graphics.PixelFormat;
import android.media.AudioManager;
import android.os.Build;
import android.util.Log;
import android.view.Gravity;
import android.view.WindowManager;

import androidx.appcompat.app.AppCompatActivity;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class TellStoryVoicePresenter extends AppCompatActivity {
    private static String TAG = "Unity";
    private Context mContext;
    private TellStoryVoiceSmallWindowLayout mVoiceFloat;
    private WindowManager mWindowManager;
    private WindowManager.LayoutParams mLayout;

    public TellStoryVoicePresenter(Context mContext) {
        this.mContext = mContext;
        createWindowManager();
        initVoice();
    }

    private void createWindowManager() {
        // 取得系统窗体
        mWindowManager = (WindowManager) mContext.getSystemService(Context.WINDOW_SERVICE);
        // 窗体的布局样式
        mLayout = new WindowManager.LayoutParams();
        // 设置窗体显示类型——TYPE_SYSTEM_ALERT(系统提示)
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {//8.0新特性
            mLayout.type = WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY;
        } else {
            mLayout.type = WindowManager.LayoutParams.TYPE_SYSTEM_ALERT;
        }
        // 设置窗体焦点及触摸：
        // FLAG_NOT_FOCUSABLE(不能获得按键输入焦点)
        mLayout.flags = WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
        // 设置显示的模式
        mLayout.format = PixelFormat.TRANSLUCENT;
        // 设置对齐的方法
        mLayout.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
        mLayout.width = WindowManager.LayoutParams.WRAP_CONTENT;
        mLayout.height = WindowManager.LayoutParams.WRAP_CONTENT;
    }

    public void showAnimaMic() {
        mWindowManager.addView(mVoiceFloat, mLayout);
    }

    public void closeAnimaMic() {
        mWindowManager.removeView(mVoiceFloat);
    }

    private void initVoice() {
        mVoiceFloat = new TellStoryVoiceSmallWindowLayout(mContext);
    }

    public void display(String result) {
        Log.i(TAG, result);
        try {
            mVoiceFloat.showVoiceText();
            mVoiceFloat.setVoiceText(result);
            startDismissVoiceTextTimer();
        } catch (Exception e) {
            Log.i(TAG, "返回数据出错：" + e.toString());
        }
    }

//    private void processEvent(AIUIEvent event, String result) {
//        String stream_id = event.data.getString("stream_id");
//        String DataLogPath = UnityPlayer.currentActivity.getExternalFilesDir(null).getAbsolutePath() + "/CacheData/";
//        Logger.LogI("stream_id：" + stream_id);
//        Logger.LogI("文件夹路径：" + DataLogPath + "index.txt");
//        Logger.LogI("文件夹内容：" + FileUtils.readTxtFile(DataLogPath + "index.txt"));
//
//        int index = Integer.parseInt(FileUtils.readTxtFile(DataLogPath + "index.txt").trim()) - 1;
//
//        mVoiceCallBack.setVoiceText(result, "");
//        String audioPath = DataLogPath + "wake" + index + "/" + stream_id + ".pcm";
//        Logger.LogI("音频文件夹路径" + audioPath);
//        mVoiceCallBack.setAudioSourcePath(audioPath);
//
//    }

    protected static Timer DISMISS_CONTROL_VIEW_TIMER;
    protected DismissVoiceTimerTask mDismissVoiceTimerTask;

    public void startDismissVoiceTextTimer() {
        cancelDismissVoiceTextTimer();
        DISMISS_CONTROL_VIEW_TIMER = new Timer();
        mDismissVoiceTimerTask = new DismissVoiceTimerTask();
        DISMISS_CONTROL_VIEW_TIMER.schedule(mDismissVoiceTimerTask, 3000);
    }

    public void cancelDismissVoiceTextTimer() {
        if (DISMISS_CONTROL_VIEW_TIMER != null) {
            DISMISS_CONTROL_VIEW_TIMER.cancel();
        }
        if (mDismissVoiceTimerTask != null) {
            mDismissVoiceTimerTask.cancel();
        }
    }


    public class DismissVoiceTimerTask extends TimerTask {
        @Override
        public void run() {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    mVoiceFloat.hideVoiceText();
                }
            });
            cancelDismissVoiceTextTimer();
        }
    }
}
//
//public class VoicePresenter extends AppCompatActivity implements WakeUpCallBack, TTsSpeakCallBack {
//
//
//    private void initVoice() {
//        mVoiceFloat = new VoiceSmallWindowLayout(mContext);
//    }
//
//    public void nlpResult(String json, AIUIEvent event) {
//        try {
//            mFromJsonResult = json;
//            Logger.LogI("nlp data：" + json);
//            mJsonRootVo = jsonBinder.fromJson(json, AIUIResultVO.class);
//            //显示文本信息相关
//            mVoiceFloat.showVoiceText();
//            mVoiceFloat.setVoiceText(mJsonRootVo.getText());
//            String serviceName = mJsonRootVo.getService();
//            startDismissVoiceTextTimer();
//            processEvent(event);
//
//            if (serviceName != null) {
//                if (mJsonRootVo.getService().equals(VoiceConfig.MUSICPLAYER_SMARTHOME)) {
//                    mJsonRootBeanObj = jsonBinder.fromJson(json, AIUIResultOb.class);
//                } else {
//                    mJsonRootBean = jsonBinder.fromJson(json, AIUIResultAr.class);
//                }
//                serviceCategory(serviceName);
//
//            } else {
//                executeVoiceCmd();
//                mVoiceCallBack.sendVoiceCmd(mJsonRootVo.getText(), VoiceTag.INIT_TAG_NOSERVICE);
//            }
//        } catch (Exception e) {
//            mVoiceCallBack.resultError(e.toString());
//            Logger.LogI("返回数据出错：" + e.toString());
//
//        }
//
//    }
//
//    private void processEvent(AIUIEvent event) {
////        String stream_id = event.data.getString("stream_id");
////        Logger.LogI("文件夹内容" + FileUtils.readTxtFile(DPConstants.DATA_CACHE_PATH + "index.txt"));
////        int index = Integer.parseInt(FileUtils.readTxtFile(DPConstants.DATA_CACHE_PATH + "index.txt").trim()) - 1;
////        if (mJsonRootVo.getAnswer() != null) {
////            mVoiceCallBack.setVoiceText(mJsonRootVo.getText(), mJsonRootVo.getAnswer().getText());
////        } else {
////            mVoiceCallBack.setVoiceText(mJsonRootVo.getText(), "");
////        }
////        String audioPath = DPConstants.DATA_CACHE_PATH + "wake" + index + "/" + stream_id + ".pcm";
////        Logger.LogI("音频文件夹路径" + audioPath);
//        mVoiceCallBack.setAudioSourcePath("");
//
//    }
//
//
//
//    protected static Timer DISMISS_CONTROL_VIEW_TIMER;
//    protected com.deepsoft.student.voice.home.presenter.VoicePresenter.DismissVoiceTimerTask mDismissVoiceTimerTask;
//
//    public void startDismissVoiceTextTimer() {
//        cancelDismissVoiceTextTimer();
//        DISMISS_CONTROL_VIEW_TIMER = new Timer();
//        mDismissVoiceTimerTask = new DismissVoiceTimerTask();
//        DISMISS_CONTROL_VIEW_TIMER.schedule(mDismissVoiceTimerTask, 5000);
//    }
//
//    public void cancelDismissVoiceTextTimer() {
//        if (DISMISS_CONTROL_VIEW_TIMER != null) {
//            DISMISS_CONTROL_VIEW_TIMER.cancel();
//        }
//        if (mDismissVoiceTimerTask != null) {
//            mDismissVoiceTimerTask.cancel();
//        }
//    }
//
//    @Override
//    public void wakeUpSuccess() {
//        mVoiceCallBack.wakeUpSuccess();
//
//    }
//
//    @Override
//    public void wakeUpError(String error) {
//        mVoiceCallBack.wakeUpError(error);
//
//    }
//
//    @Override
//    public void onSpeakBegin() {
//        mVoiceCallBack.onSpeakBegin();
//
//    }
//
//    @Override
//    public void onSpeakPaused() {
//        mVoiceCallBack.onSpeakPaused();
//
//    }
//
//    @Override
//    public void onSpeakResumed() {
//        mVoiceCallBack.onSpeakResumed();
//
//    }
//
//    @Override
//    public void onSpeakProgress(int percent, int beginPos, int endPos) {
//        mVoiceCallBack.onSpeakProgress(percent, beginPos, endPos);
//
//    }
//
//    @Override
//    public void onSpeakCompleted(String des) {
//        mVoiceCallBack.onSpeakEnd(des);
//    }
//
//    public class DismissVoiceTimerTask extends TimerTask {
//        @Override
//        public void run() {
//            mVoiceCallBack.hideVoiceText();
//
//            runOnUiThread(new Runnable() {
//                @Override
//                public void run() {
//                    mVoiceFloat.hideVoiceText();
//                }
//            });
//
//            cancelDismissVoiceTextTimer();
//
//        }
//    }
//
//    /**
//     * 设置WindowManager
//     */
//    private void createWindowManager() {
//        // 取得系统窗体
//        mWindowManager = (WindowManager) mContext.getSystemService(Context.WINDOW_SERVICE);
//        // 窗体的布局样式
//        mLayout = new WindowManager.LayoutParams();
//        // 设置窗体显示类型——TYPE_SYSTEM_ALERT(系统提示)
//        if (Build.VERSION.SDK_INT >= 26) {//8.0新特性
//            mLayout.type = WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY;
//        } else {
//            mLayout.type = WindowManager.LayoutParams.TYPE_SYSTEM_ALERT;
//        }
//        // 设置窗体焦点及触摸：
//        // FLAG_NOT_FOCUSABLE(不能获得按键输入焦点)
//        mLayout.flags = WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
//        // 设置显示的模式
//        mLayout.format = PixelFormat.TRANSLUCENT;
//        // 设置对齐的方法
////        mLayout.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
//        mLayout.gravity = Gravity.BOTTOM | Gravity.LEFT;
//        // 设置窗体宽度和高度
//        mLayout.width = WindowManager.LayoutParams.WRAP_CONTENT;
//        mLayout.height = WindowManager.LayoutParams.WRAP_CONTENT;
//    }
//
//    public void showAnimaMic() {
//        mVoiceTTsUtils.ttsSpeakBegin(RandomUntil.getWakeLanguage());
//        mVoiceFloat.startAnimationMic();
//        mWindowManager.addView(mVoiceFloat, mLayout);
//
//    }
//
//    public void closeAnimaMic() {
//        mVoiceTTsUtils.ttsSpeakBegin(RandomUntil.getCloseWakeLanguage());
//        mVoiceWakeUtils.startListening();
//        mVoiceFloat.closeAnimationMic();
//        mWindowManager.removeView(mVoiceFloat);
//    }
//
//    public void stopVoiceListener() {
//        mVoiceWakeUtils.stopListening();
//        Logger.LogE("stopListening");
//    }
//
//    public void startVoiceListener() {
//        mVoiceWakeUtils.startListening();
//
//    }
//
//}
