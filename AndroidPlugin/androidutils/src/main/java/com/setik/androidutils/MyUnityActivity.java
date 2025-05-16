package com.setik.androidutils;

import com.unity3d.player.UnityPlayerActivity;
import android.os.Bundle;
import android.util.Log;

public class MyUnityActivity extends UnityPlayerActivity {
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.e("OverrideActivity", "onCreate called!");
    }

    @Override
    public void onPause()
    {
        super.onPause();
        Log.e("MyUnityActivity", "onPause called!");
    }

    @Override
    public void onStop()
    {
        super.onStop();
        Log.e("MyUnityActivity", "onStop called!");
    }

    public void onBackPressed()
    {
    }
}