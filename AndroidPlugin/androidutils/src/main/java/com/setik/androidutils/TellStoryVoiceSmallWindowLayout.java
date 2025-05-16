package com.setik.androidutils;

import android.content.Context;
import android.graphics.drawable.AnimationDrawable;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

public class TellStoryVoiceSmallWindowLayout extends LinearLayout {
    public TextView mText;
    public TellStoryVoiceSmallWindowLayout(Context context) {
        super(context);
        setOrientation(LinearLayout.VERTICAL);// 水平排列
        this.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT,
                LayoutParams.WRAP_CONTENT));//设置宽高
        View view = LayoutInflater.from(context).inflate(
                com.hbisoft.hbrecorder.R.layout.voice_small_window_float, null);
        mText = view.findViewById(com.hbisoft.hbrecorder.R.id.speak_text);
        this.addView(view);
    }
    public void showVoiceText() {
        mText.setVisibility(VISIBLE);
    }
    public void hideVoiceText() {
        mText.setVisibility(GONE);
    }
    public void setVoiceText(String text) {
        mText.setText(text);
    }
}

