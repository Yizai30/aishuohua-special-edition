using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Dropdown bgmDropDown;

    public string wavFileName = "music/summer.wav"; // Replace with the name of your WAV file

    public BgmAsset bgmAsset;

    public AudioSource audioSource_bgm;

    public AudioSource audioSource_re;

    private void Start()
    {
        initDropDown();
    }

    enum AudioResourceType
    {
        STREAMASSET,
        PERSISTENTASSET

    }

    private void initDropDown()
    {
        // 创建一个包含选项的列表
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        for (int i = 0; i < bgmAsset.bgmList.Count; i++)
        {
            options.Add(new Dropdown.OptionData(bgmAsset.bgmList[i].bgmName));
        }
        // 添加选项


        bgmDropDown.options = options;

        bgmDropDown.onValueChanged.AddListener(OnDropdownValueChanged);

        wavFileName = bgmAsset.bgmList[0].bgmPath;
    }

    private void OnDropdownValueChanged(int index)
    {
        // index 参数表示选择的项的索引
        string selectedOption = bgmDropDown.options[index].text; // 获取选择的项的文本内容
        Debug.Log("你选择了：" + selectedOption);

        BgmElementAsset bgmElementAsset = bgmAsset.bgmList[index];
        wavFileName = bgmElementAsset.bgmPath;
    }

    public void Playbgm()
    {
        Debug.Log("---------播放bgm-------");
        string bgmPath= System.IO.Path.Combine(Application.streamingAssetsPath, wavFileName);
        StartCoroutine(LoadWavAndPlay(bgmPath,audioSource_bgm,AudioResourceType.STREAMASSET));
    }

    public void StopPlaybgm()
    {
        if(audioSource_bgm != null)
        {
            audioSource_bgm.Stop();
        }
    }

    public void Playre(string rePath)
    {
        Debug.Log("---------播放录制音频-------");
        StartCoroutine(LoadWavAndPlay(rePath, audioSource_re,AudioResourceType.PERSISTENTASSET));
    }
    public void StopPlayre()
    {
        if (audioSource_re != null)
        {
            audioSource_re.Stop();
        }
    }

    private IEnumerator LoadWavAndPlay(string path,AudioSource audioSource, AudioResourceType resourceType)
    {
        //string path = System.IO.Path.Combine(Application.streamingAssetsPath, wavFileName);

        // Handle Android platform separately using UnityWebRequest
        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("jar:file://" + path, AudioType.WAV);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error loading WAV file: " + www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                PlayAudioClip(audioClip, audioSource);
            }
        }
        else
        {
            // For other platforms, use the standard WWW class
            WWW www = new WWW("file://" + path);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("Error loading WAV file: " + www.error);
            }
            else
            {
                AudioClip audioClip = www.GetAudioClip(false, false, AudioType.WAV);
                PlayAudioClip(audioClip,audioSource);
            }
        }
    }

    private void PlayAudioClip(AudioClip audioClip,AudioSource audioSource)
    {
        if (audioClip != null)
        {
            Debug.Log("开始播放音频" + audioClip.name);
           
            if (audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }   
        }
        else
        {
            Debug.LogError("AudioClip is null. Cannot play the WAV file.");
        }
    }
}
