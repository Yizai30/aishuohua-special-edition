package com.setik.androidutils;

import android.content.Context;
import android.content.res.AssetManager;
import android.text.TextUtils;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;

public class TellStoryVoiceNLPUtil {
    private static String TAG = "Unity";

    //private AIUIAgent mAIUIAgent = null;
    //private int mAIUIState = AIUIConstant.STATE_IDLE;
    private Context mContext;
    private boolean mIsWakeupEnable = false;
    //private VoiceCallBackImpl mVoiceBack;
    private TellStoryVoicePresenter mPresenter;


    public TellStoryVoiceNLPUtil(Context mContext) {// VoiceCallBackImpl mCallBack
        this.mContext = mContext;
        //this.mVoiceBack = mCallBack;
        mPresenter = new TellStoryVoicePresenter(mContext); //, mVoiceBack
        createAgent();
    }

//    private AIUIListener mAIUIListener = new AIUIListener() {
//        @Override
//        public void onEvent(AIUIEvent event) {
//            Log.i(TAG, "on event: " + event.eventType);
//
//            switch (event.eventType) {
//                case AIUIConstant.EVENT_CONNECTED_TO_SERVER:
//                    Log.i(TAG, "已连接服务器");
//                    break;
//
//                case AIUIConstant.EVENT_SERVER_DISCONNECTED:
//                    Log.i(TAG, "与服务器断连");
//                    break;
//
//                case AIUIConstant.EVENT_WAKEUP:
//                    Log.e(TAG, "进入识别状态");
//
//                    break;
//
//                case AIUIConstant.EVENT_RESULT: {
//                    try {
//                        // Force update timer for parameter "interact_timeout"
//                        AIUIMessage wakeupMsg = new AIUIMessage(AIUIConstant.CMD_RESULT_VALIDATION_ACK, 0, 0, "", null);
//                        mAIUIAgent.sendMessage(wakeupMsg);
//                        // Process recognition result
//                        JSONObject bizParamJson = new JSONObject(event.info);
//                        JSONObject data = bizParamJson.getJSONArray("data").getJSONObject(0);
//                        JSONObject params = data.getJSONObject("params");
//                        JSONObject content = data.getJSONArray("content").getJSONObject(0);
//                        String sub = params.optString("sub");
//
//                        if (content.has("cnt_id") && !"tts".equals(sub)) {
//                            String cnt_id = content.getString("cnt_id");
//
//                            String cntStr = new String(event.data.getByteArray(cnt_id), "utf-8");
//
//                            // 获取该路会话的id，将其提供给支持人员，有助于问题排查
//                            // 也可以从Json结果中看到
//                            String tag = event.data.getString("tag");
//                            String stream_id = event.data.getString("stream_id");
//
//                            Logger.LogD(TAG, "tag=" + tag);
//                            Logger.LogD(TAG, "stream_id0=" + stream_id);
//                            Logger.LogD(TAG, cntStr);
//
//                            // 获取从数据发送完到获取结果的耗时，单位：ms
//                            // 也可以通过键名"bos_rslt"获取从开始发送数据到获取结果的耗时
//                            long eosRsltTime = event.data.getLong("eos_rslt", -1);
//                            Logger.LogD(TAG, eosRsltTime + "ms");
//
//                            if (TextUtils.isEmpty(cntStr)) {
//                                Logger.LogD(TAG, eosRsltTime + "ms");
//                                return;
//                            }
//
//                            JSONObject cntJson = new JSONObject(cntStr);
//                            String result = "";
//                            try {
//                                JSONArray arr = cntJson.getJSONObject("text").getJSONArray("ws");
//                                for (int i = 0; i < arr.length(); i++) {
//                                    result += arr.getJSONObject(i).getJSONArray("cw").getJSONObject(0).getString("w");
//                                }
//                            } catch (Exception e) {
//                                Logger.LogD(TAG, "parse err: " + e.toString());
//                            }
//                            if (result != "") {
//                                Log.i(TAG, result);
//                                mPresenter.nlpResult(result, event);
//                            }
////                            if ("nlp".equals(sub)) {
////                                // 解析得到语义结果
////                                String resultStr = cntJson.optString("intent");
////                                if (resultStr.equals("{}")) {
////                                    Log.i(TAG, resultStr);
////                                    Logger.LogD(TAG, "stream_id1=" + stream_id);
////                                } else {
////                                    Logger.LogD(TAG, "stream_id2=" + stream_id);
////                                }
////                            }
//                        }
//                    } catch (Throwable e) {
//                        e.printStackTrace();
//
//                        Logger.LogE(TAG, e.getLocalizedMessage());
//
//                    }
//
//                }
//                break;
//
//                case AIUIConstant.EVENT_ERROR: {
//                    Logger.LogE(TAG, "错误: " + event.arg1 + "\n" + event.info);
//                    Log.e(TAG, "on event: " + "录音出错" + event.eventType);
//
//                }
//                break;
//
//                case AIUIConstant.EVENT_VAD: {
//                    if (AIUIConstant.VAD_BOS == event.arg1) {
//                        Logger.LogD("找到vad_bos");
//                    } else if (AIUIConstant.VAD_EOS == event.arg1) {
//                        Logger.LogD("找到vad_eos");
//                    } else {
//                        Logger.LogD("" + event.arg2);
//                    }
//                }
//                break;
//
//                case AIUIConstant.EVENT_START_RECORD: {
//                    Log.i(TAG, "开始录音");
//                    mPresenter.showAnimaMic();
//                }
//                break;
//
//                case AIUIConstant.EVENT_STOP_RECORD: {
//                    Log.i(TAG, "停止录音");
//                    mPresenter.closeAnimaMic();
//                }
//                break;
//
//                case AIUIConstant.EVENT_STATE: {    // 状态事件
//                    mAIUIState = event.arg1;
//                    if (AIUIConstant.STATE_IDLE == mAIUIState) {
//                    } else if (AIUIConstant.STATE_READY == mAIUIState) {
//                        stopVoiceNlp();
//                    } else if (AIUIConstant.STATE_WORKING == mAIUIState) {
//                    }
//                }
//                break;
//
//                case AIUIConstant.EVENT_CMD_RETURN: {
//                    if (AIUIConstant.CMD_SYNC == event.arg1) {    // 数据同步的返回
//                        int dtype = event.data.getInt("sync_dtype", -1);
//                        switch (dtype) {
//                            case AIUIConstant.SYNC_DATA_SCHEMA: {
//
//                            }
//                            break;
//                        }
//                    } else if (AIUIConstant.CMD_QUERY_SYNC_STATUS == event.arg1) {    // 数据同步状态查询的返回
//                        // 获取同步类型
//                        int syncType = event.data.getInt("sync_dtype", -1);
//                        if (AIUIConstant.SYNC_DATA_QUERY == syncType) {
//                            // 若是同步数据查询，则获取查询结果，结果中error字段为0则表示上传数据打包成功，否则为错误码
//                            String result = event.data.getString("result");
//
//                            Logger.LogD(result);
//                        }
//                    }
//                }
//                break;
//
//                default:
//                    break;
//            }
//        }
//
//    };

    private String getAIUIParams() {
        String params = "";

        AssetManager assetManager = mContext.getResources().getAssets();
        try {
            InputStream ins = assetManager.open("cfg/aiui_phone.cfg");
            byte[] buffer = new byte[ins.available()];

            ins.read(buffer);
            ins.close();

            params = new String(buffer);
            JSONObject paramsJson = new JSONObject(params);
            // Set new vad_eos
            paramsJson.getJSONObject("vad").put("vad_eos", "1000");
            paramsJson.getJSONObject("interact").put("interact_timeout", "180000");
            paramsJson.getJSONObject("interact").put("result_timeout", "1000");
            params = paramsJson.toString();
        } catch (IOException e) {
            e.printStackTrace();
        } catch (JSONException e) {
            e.printStackTrace();
        }
        return params;
    }

    public void createAgent() {
//        if (null == mAIUIAgent) {
//            Log.i(TAG, "create aiui agent");
//
//            //为每一个设备设置对应唯一的SN（最好使用设备硬件信息(mac地址，设备序列号等）生成），以便正确统计装机量，避免刷机或者应用卸载重装导致装机量重复计数
//            String deviceId = VoiceDeviceUtils.getDeviceId(mContext);
//            Log.i(TAG, "device id : " + deviceId);
//            AIUISetting.setSystemInfo(AIUIConstant.KEY_SERIAL_NUM, deviceId);
//
//            Log.i(TAG,"AIUI路径=="+AIUISetting.getAIUIDir());
//            String DataLogPath = UnityPlayer.currentActivity.getExternalFilesDir(null).getAbsolutePath() + "/CacheData/";
//            AIUISetting.setDataLogDir(DataLogPath);
//            Log.i(TAG,"AIUI路径数据保存路径=="+DataLogPath);
//            File dataFolder = new File(DataLogPath);
//            if (!dataFolder.exists()) {
//                Log.i(TAG,"创建文件夹：" + DataLogPath);
//                boolean result = dataFolder.mkdirs();
//                Log.i(TAG,"创建文件夹" + (result ? "成功": "失败"));
//            } else {
//                Log.i(TAG,"文件夹已存在：" + DataLogPath);
//            }
//            AIUISetting.setLocationEnable(true);
//            AIUISetting.setSaveDataLog(true);
//            mAIUIAgent = AIUIAgent.createAgent(mContext, getAIUIParams(), mAIUIListener);
//        }
//        if (null == mAIUIAgent) {
//            final String strErrorTip = "创建AIUIAgent失败！";
//            Logger.LogD(TAG, strErrorTip);
//        } else {
//            Logger.LogD(TAG, "AIUIAgent已创建");
//
//        }
    }

    public void startVoiceNlp() {
//        if (null == mAIUIAgent) {
//            Logger.LogD("AIUIAgent为空，请先创建");
//            return;
//        }
//
//        Logger.LogD(TAG, "start voice nlp");
//        // 先发送唤醒消息，改变AIUI内部状态，只有唤醒状态才能接收语音输入
//        // 默认为oneshot模式，即一次唤醒后就进入休眠。可以修改aiui_phone.cfg中speech参数的interact_mode为continuous以支持持续交互
//        if (!mIsWakeupEnable) {
//            AIUIMessage wakeupMsg = new AIUIMessage(AIUIConstant.CMD_WAKEUP, 0, 0, "", null);
//            mAIUIAgent.sendMessage(wakeupMsg);
//        }
//
//        // 打开AIUI内部录音机，开始录音。若要使用上传的个性化资源增强识别效果，则在参数中添加pers_param设置
//        // 个性化资源使用方法可参见http://doc.xfyun.cn/aiui_mobile/的用户个性化章节
//        // 在输入参数中设置tag，则对应结果中也将携带该tag，可用于关联输入输出
//        String params = "sample_rate=16000,data_type=audio,pers_param={\"uid\":\"\"},tag=audio-tag";
//        AIUIMessage startRecord = new AIUIMessage(AIUIConstant.CMD_START_RECORD, 0, 0, params, null);
//
//        mAIUIAgent.sendMessage(startRecord);
    }

    public void stopVoiceNlp() {
//        if (null == mAIUIAgent) {
//            Logger.LogD("AIUIAgent 为空，请先创建");
//            return;
//        }
//
//        Log.i(TAG, "stop voice nlp");
//        // 停止录音
//        String params = "sample_rate=16000,data_type=audio";
//        AIUIMessage stopRecord = new AIUIMessage(AIUIConstant.CMD_STOP_RECORD, 0, 0, params, null);
//
//        mAIUIAgent.sendMessage(stopRecord);
    }

//    public void stopVoiceListener() {
//        mPresenter.stopVoiceListener();
//    }
//
//    public void startVoiceListener() {
//        mPresenter.startVoiceListener();
//    }
}
