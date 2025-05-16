package screenrecorder;

import android.app.Activity;
import android.content.Intent;
import android.media.projection.MediaProjectionManager;
import android.os.Bundle;
import android.os.Handler;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentActivity;

import com.setik.androidutils.ScreenRecord;

public class PermissionRequestActivity extends FragmentActivity {
    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        final MediaProjectionManager mediaProjectionManager =
                getSystemService(MediaProjectionManager.class);
        ActivityResultLauncher<Intent> startMediaProjection = registerForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                result -> {
                    if (result.getResultCode() == Activity.RESULT_OK) {
                        ScreenRecord.mMediaProjection = mediaProjectionManager.getMediaProjection(result.getResultCode(), result.getData());
                        ScreenRecord.mMediaProjection.registerCallback(ScreenRecord.mProjectionCallback, new Handler());
                        ScreenRecord.startCapturing(ScreenRecord.mMediaProjection);
                    }
                }
        );
    }
}