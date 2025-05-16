using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecordButton: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<string> OnRecordFinish;
    public Action<bool> OnPressStateChanged;
    public bool IsRecording = false;

    string RecordResult = "";

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_ANDROID
        IsRecording = true;
        OnPressStateChanged(IsRecording);
        AndroidUtils.ShowToast("开始录音，松开时停止");
        AndroidUtils.startRecognize();
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(StopRecord());
    }

    bool ProcessingRecordResult = false;
    IEnumerator StopRecord()
    {
#if UNITY_ANDROID
        if (ProcessingRecordResult) yield break;
        ProcessingRecordResult = true;
        yield return new WaitForSeconds(1);
        string returnedValue = AndroidUtils.stopRecognize();
        IsRecording = false;
        OnPressStateChanged(IsRecording);
        ProcessingRecordResult = false;
        var values = returnedValue.Split('|');
        if (values[0].Length > 0 && values[1].Length > 0)
        {
            RecordResult = values[0].Replace("。", "");
            OnRecordFinish(RecordResult);
        }
        else
        {
            AndroidUtils.ShowToast("识别失败，请重试");
        }
#endif
        yield return null;
    }
}

