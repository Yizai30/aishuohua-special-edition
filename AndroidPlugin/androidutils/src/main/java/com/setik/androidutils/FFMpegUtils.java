package com.setik.androidutils;

import android.util.Log;

//import com.arthenica.ffmpegkit.FFmpegKit;
//import com.arthenica.ffmpegkit.FFmpegSession;
//import com.arthenica.ffmpegkit.ReturnCode;

public class FFMpegUtils {
//    public static void encodeAndScale(String inputPath, String outputPath) {
//        Log.d("Unity", String.format("Encoding file " + inputPath + " to " + outputPath));
//        //String params = "-c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -movflags faststart";
//        String params = "-c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -movflags faststart -c:a aac -ac 2 -b:a 128k -vf scale=\"1280:720\"";
//        String executedCommand = String.format("-i %s %s %s -y", inputPath, params, outputPath);
//        Log.d("Unity", "Executed command: " + executedCommand);
//        FFmpegSession session = FFmpegKit.execute(executedCommand);
//        if (ReturnCode.isSuccess(session.getReturnCode())) {
//            Log.d("Unity", "Command success");
//        } else if (ReturnCode.isCancel(session.getReturnCode())) {
//        } else {
//            Log.d("Unity", String.format("Command failed with state %s and rc %s.%s", session.getState(), session.getReturnCode(), session.getFailStackTrace()));
//        }
//    }
}
