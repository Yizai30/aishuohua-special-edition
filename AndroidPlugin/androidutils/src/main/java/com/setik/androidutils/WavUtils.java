package com.setik.androidutils;

import android.media.SoundPool;
import android.util.Log;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.HashMap;

public class WavUtils {
    public static String CombinePCMFiles(String[] filelist) {
        Log.i("Unity", "merge " + filelist);
        if (filelist.length == 0) {
            return "";
        }
        if (filelist.length == 1) {
            return filelist[0];
        }
        String tmpout = filelist[0].replace(".pcm", "_temp.pcm");
        String tmpin = filelist[0];
        for (int i = 1; i < filelist.length; i++) {
            CombinePCMFile(tmpin, filelist[i], tmpout);
            tmpin = tmpout;
            tmpout = filelist[i].replace(".pcm", "_temp.pcm");
        }
        Log.i("Unity", "CombinePCMFiles: return " + tmpin);
        return tmpin;
    }

    public static void CombinePCMFile(String in1fn, String in2fn, String outfn) {
        Log.i("Unity", "CombinePCMFile: " + in1fn + " & " + in2fn + " -> " + outfn);
        FileInputStream in1 = null, in2 = null;
        FileOutputStream out = null;
        byte[] data = new byte[TRANSFER_BUFFER_SIZE];
        try {
            in1 = new FileInputStream(new File(in1fn));
            in2 = new FileInputStream(new File(in2fn));

            out = new FileOutputStream(new File(outfn));
            while (in1.read(data) != -1) {
                out.write(data);
            }
            while (in2.read(data) != -1) {
                out.write(data);
            }
            out.close();
            in1.close();
            in2.close();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public static void CombineWaveFile(String in1fn, String in2fn, String outfn) {
        Log.i("Unity", "CombineWaveFile: " + in1fn + " & " + in2fn + " -> " + outfn);
        FileInputStream in1 = null, in2 = null;
        FileOutputStream out = null;
        long totalAudioLen = 0;
        long totalDataLen = totalAudioLen + 36;
        long longSampleRate = 16000;
        int channels = 1;
        long byteRate = 16 * 16000 * channels / 8;

        byte[] data = new byte[TRANSFER_BUFFER_SIZE];

        try {
            in1 = new FileInputStream(new File(in1fn));
            in2 = new FileInputStream(new File(in2fn));

            out = new FileOutputStream(new File(outfn));

            totalAudioLen = in1.getChannel().size() + in2.getChannel().size();
            totalDataLen = totalAudioLen + 36;

            WriteWaveFileHeader(out, totalAudioLen, totalDataLen,
                    longSampleRate, channels, byteRate);

            while (in1.read(data) != -1) {
                out.write(data);

            }
            while (in2.read(data) != -1) {
                out.write(data);
            }

            out.close();
            in1.close();
            in2.close();

        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    static void WriteWaveFileHeader(FileOutputStream out, long totalAudioLen,
                                     long totalDataLen, long longSampleRate, int channels, long byteRate)
            throws IOException {

        byte[] header = new byte[44];

        header[0] = 'R';
        header[1] = 'I';
        header[2] = 'F';
        header[3] = 'F';
        header[4] = (byte)(totalDataLen & 0xff);
        header[5] = (byte)((totalDataLen >> 8) & 0xff);
        header[6] = (byte)((totalDataLen >> 16) & 0xff);
        header[7] = (byte)((totalDataLen >> 24) & 0xff);
        header[8] = 'W';
        header[9] = 'A';
        header[10] = 'V';
        header[11] = 'E';
        header[12] = 'f';
        header[13] = 'm';
        header[14] = 't';
        header[15] = ' ';
        header[16] = 16;
        header[17] = 0;
        header[18] = 0;
        header[19] = 0;
        header[20] = 1;
        header[21] = 0;
        header[22] = (byte) channels;
        header[23] = 0;
        header[24] = (byte)(longSampleRate & 0xff);
        header[25] = (byte)((longSampleRate >> 8) & 0xff);
        header[26] = (byte)((longSampleRate >> 16) & 0xff);
        header[27] = (byte)((longSampleRate >> 24) & 0xff);
        header[28] = (byte)(byteRate & 0xff);
        header[29] = (byte)((byteRate >> 8) & 0xff);
        header[30] = (byte)((byteRate >> 16) & 0xff);
        header[31] = (byte)((byteRate >> 24) & 0xff);
        header[32] = (byte)(2 * 16 / 8);
        header[33] = 0;
        header[34] = 16;
        header[35] = 0;
        header[36] = 'd';
        header[37] = 'a';
        header[38] = 't';
        header[39] = 'a';
        header[40] = (byte)(totalAudioLen & 0xff);
        header[41] = (byte)((totalAudioLen >> 8) & 0xff);
        header[42] = (byte)((totalAudioLen >> 16) & 0xff);
        header[43] = (byte)((totalAudioLen >> 24) & 0xff);

        out.write(header, 0, 44);
    }


    static public void PCMToWAV(File input, File output) throws IOException {
        final int inputSize = (int) input.length();
        int channelCount = 1;
        int sampleRate = 16000;
        int bitsPerSample = 16;

        try (OutputStream encoded = new FileOutputStream(output)) {
            // WAVE RIFF header
            writeToOutput(encoded, "RIFF"); // chunk id
            writeToOutput(encoded, 36 + inputSize); // chunk size
            writeToOutput(encoded, "WAVE"); // format

            // SUB CHUNK 1 (FORMAT)
            writeToOutput(encoded, "fmt "); // subchunk 1 id
            writeToOutput(encoded, 16); // subchunk 1 size
            writeToOutput(encoded, (short) 1); // audio format (1 = PCM)
            writeToOutput(encoded, (short) channelCount); // number of channelCount
            writeToOutput(encoded, sampleRate); // sample rate
            writeToOutput(encoded, sampleRate * channelCount * bitsPerSample / 8); // byte rate
            writeToOutput(encoded, (short) (channelCount * bitsPerSample / 8)); // block align
            writeToOutput(encoded, (short) bitsPerSample); // bits per sample

            // SUB CHUNK 2 (AUDIO DATA)
            writeToOutput(encoded, "data"); // subchunk 2 id
            writeToOutput(encoded, inputSize); // subchunk 2 size
            copy(new FileInputStream(input), encoded);
        }
    }

    /**
     * Size of buffer used for transfer, by default
     */
    private static final int TRANSFER_BUFFER_SIZE = 10 * 1024;

    /**
     * Writes string in big endian form to an output stream
     *
     * @param output stream
     * @param data   string
     * @throws IOException
     */
    public static void writeToOutput(OutputStream output, String data) throws IOException {
        for (int i = 0; i < data.length(); i++)
            output.write(data.charAt(i));
    }

    public static void writeToOutput(OutputStream output, int data) throws IOException {
        output.write(data >> 0);
        output.write(data >> 8);
        output.write(data >> 16);
        output.write(data >> 24);
    }

    public static void writeToOutput(OutputStream output, short data) throws IOException {
        output.write(data >> 0);
        output.write(data >> 8);
    }

    public static long copy(InputStream source, OutputStream output)
            throws IOException {
        return copy(source, output, TRANSFER_BUFFER_SIZE);
    }

    public static long copy(InputStream source, OutputStream output, int bufferSize) throws IOException {
        long read = 0L;
        byte[] buffer = new byte[bufferSize];
        for (int n; (n = source.read(buffer)) != -1; read += n) {
            output.write(buffer, 0, n);
        }
        return read;
    }

    static SoundPool pool = new  SoundPool.Builder().build();
    static HashMap<String, Integer> loadedFiles = new HashMap<>();
    public static void playWavFile(String path) {
        if (!loadedFiles.containsKey(path)) {
            int mid = pool.load(path, 0);
            pool.setOnLoadCompleteListener((SoundPool soundPool, int sampleId, int status) -> {
                    int streamId = soundPool.play(mid, 1, 1, 128, 0, 1);
                    if (streamId == 0) {
                        Log.e("Unity", "playWavFile failed: cannot be added to stream");
                    }
            });
        } else {
            pool.play(loadedFiles.get(path), 1, 1, 128, 0, 1);
        }
    }
}
