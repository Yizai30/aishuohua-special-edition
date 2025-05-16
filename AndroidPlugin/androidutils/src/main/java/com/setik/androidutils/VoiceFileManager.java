package com.setik.androidutils;

import android.app.Activity;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class VoiceFileManager {
    public static String currentFolderPath = "";
    private static int counter = 0;
    public static void Init(Activity activity) {
        for (int i = 1; i < 1e6; i++) {
            currentFolderPath = activity.getExternalFilesDir(null).getAbsolutePath() + "/wave" + i + "/";
            File dir = new File(currentFolderPath);
            if (!dir.exists()) {
                dir.mkdirs();
                Log.i("Unity", "using folder: " + currentFolderPath);
                break;
            }
        }
    }

    public static String addVoiceFile(String originalPath) {
        if (currentFolderPath == "") return "";
        counter += 1;
        String destPath = currentFolderPath + "voice" + counter + ".pcm";
        try (
                InputStream in = new FileInputStream(originalPath);
                OutputStream out = new FileOutputStream(destPath);
        ) {
            byte[] buffer = new byte[4096];
            int length;
            while ((length = in.read(buffer)) > 0) {
                out.write(buffer, 0, length);
            }
            Log.i("Unity", "voice file copied success");
            return destPath;
        } catch (IOException e) {
            Log.e("Unity", "fail to copy file to " + destPath);
            return "";
        }
    }
}
