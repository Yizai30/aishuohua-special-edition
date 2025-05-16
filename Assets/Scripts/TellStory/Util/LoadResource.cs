using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using AnimGenerator;
public class LoadResource : MonoBehaviour
{

    public Interpreter interpreter;
    
    public  IEnumerator LoadAudioFile(string fullpath)
    {

        Debug.Log("正在加载 " + fullpath);

        if (!System.IO.File.Exists(fullpath))
        {
            Debug.Log("文件不存在: " + fullpath);
            yield break;
        }

        AudioClip temp = null;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + fullpath, AudioType.WAV))
        {
            www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                temp = DownloadHandlerAudioClip.GetContent(www);
            }
        }
        interpreter.setAudioClip(temp, fullpath);
        yield break;
        //changeFunction.Invoke(temp);
    }



}
