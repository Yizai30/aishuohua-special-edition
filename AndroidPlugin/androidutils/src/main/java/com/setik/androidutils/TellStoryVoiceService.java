package com.setik.androidutils;


public class TellStoryVoiceService { //implements VoiceServiceImpl, VoiceCallBackImpl {
    private TellStoryVoiceNLPUtil nlpUtil;
    //private VoiceModuleNlpListener nlpListener;

//    @Override
//    public void startModuleActivity(Context mContext, int requestCode) {
//        Intent mIntent = new Intent(mContext, DeepSoftVoiceHomeActivity.class);
//        mContext.startActivity(mIntent);
//    }
//    @Override
//    public void initVoiceService(Context mContext) {
//        nlpUtil = new TellStoryVoiceNLPUtil(mContext, this);
//    }

//
//    @Override
//    public void stopAIUI() {
//        nlpUtil.stopVoiceNlp();
//    }
//
//    @Override
//    public void stopVoiceListener() {
//        nlpUtil.stopVoiceListener();
//    }
//
//    @Override
//    public void startVoiceListener() {
//        nlpUtil.startVoiceListener();
//    }
//
//    @Override
//    public void setVoiceText(String text, String text2) {
//        if (nlpListener != null) {
//            nlpListener.recognitionVoiceText(text, text2);
//        }
//    }
//
//    @Override
//    public void setAudioSourcePath(String path) {
//        if (nlpListener != null) {
//            nlpListener.audioSourcePath(path);
//        }
//    }
//
//    @Override
//    public void setVoiceModuleNlpListener(VoiceModuleNlpListener nlpListener) {
//        this.nlpListener = nlpListener;
//    }
//
//    @Override
//    public void startVoiceNlpRecognize(VoiceModuleNlpListener nlpListener) {
//        this.nlpListener = nlpListener;
//        if (nlpUtil != null) {
//            nlpUtil.startVoiceNlp();
//        } else {
//            Logger.LogE("请初始化语音服务");
//        }
//
//    }
//
//    @Override
//    public void stopVoiceNlpRecognize() {
//        if (nlpUtil != null) {
//            nlpUtil.stopVoiceNlp();
//        } else {
//            Logger.LogE("请初始化语音服务");
//        }
//    }
}
