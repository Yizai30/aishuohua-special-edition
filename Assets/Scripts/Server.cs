using Assets.Scripts.TellStory.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class Server
{
    // public static string HOST = "http://139.224.214.59:8443/";
    // public static string HOSTANIM = "http://139.224.214.59:8444/";
    public static string HOST = "http://139.224.239.149:8443/";
    public static string HOSTANIM = "http://139.224.239.149:8444/";
    public static string USERNAME = "testuser";
    public static bool sendLogToServer = false;
    public static string deviceName, deviceUniqueIdentifier;

    public static List<string> audioFiles = new List<string>();
    public static string child_id, name;
    public static int SIMPLE_REQUEST_TIME_OUT = 3;

    public static double UploadProgress = 0;
    public static int TotalCount = 100;
    public static bool StopUploadProgress = false;


    public static string getMatCreator()
    {
        if (Server.deviceUniqueIdentifier == null ||
            Server.deviceUniqueIdentifier.Length == 0)
        {
            return "testuser";
        }
        else
        {
            return Server.deviceUniqueIdentifier;
        }
    }

    public static IEnumerator UpdateProgress()
    {
        StopUploadProgress = false;
        UploadProgress = 0;
        
        while (!StopUploadProgress && UploadProgress < 1.0 - 1.0 / TotalCount)
        {
            yield return new WaitForSeconds(1);
            UploadProgress += 1.0 / TotalCount;
        }
    }

    public static IEnumerator FixRecordedVideoAndSaveToFile()
    {
        string recordedVideoPath = "/storage/emulated/0/Movies/TellStory/" + AndroidUtils.RecordFileName;
        //string recordedVideoPath = Application.persistentDataPath + "/videos/" + AndroidUtils.RecordFileName;
        WWWForm form = new WWWForm();
        if (string.IsNullOrEmpty(child_id))
        {
            form.AddField("child_id", "未知作者");
        } else
        {
            form.AddField("child_id", child_id);
        }
        if (string.IsNullOrEmpty(name))
        {
            form.AddField("name", "未命名");
        }
        else
        {
            form.AddField("name", name);
        }

        byte[] videoData = File.ReadAllBytes(recordedVideoPath);
        Debug.Log("Read " + videoData.Length + " bytes from " + recordedVideoPath);
        int maxTry = 5, curTry = 0;
        while (curTry < maxTry && videoData.Length < 10240)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Again: read " + videoData.Length + " bytes from " + recordedVideoPath);
            videoData = File.ReadAllBytes(recordedVideoPath);
            curTry++;
        }
        if (videoData.Length < 10240)
        {
            AndroidUtils.ShowToast("视频文件数据异常，可能无法正常播放");
        }
        Debug.Log("Prepare to upload " + videoData.Length + " bytes from " + recordedVideoPath);
        form.AddBinaryData("video", videoData, recordedVideoPath);
        UnityWebRequest www = UnityWebRequest.Post($"{HOST}api/v2/upload/", form);
        www.timeout = TotalCount;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            StopUploadProgress = true;
            Debug.Log("Upload fail: "+ www.responseCode + " , reason: " + www.error);
            AndroidUtils.ShowToast("上传失败，请检查网络状况");
            SceneManager.LoadScene("SceneMainMenu");
        }
        else
        {
            StopUploadProgress = true;
            Debug.Log("Upload success");
        }
        //string videoSavedPath = Application.persistentDataPath + "/video/ScreenCapture.mp4";
        //System.IO.Directory.CreateDirectory(videoSavedPath.Substring(0, videoSavedPath.LastIndexOf("/")));
        //System.IO.File.WriteAllBytes(videoSavedPath, www.downloadHandler.data);
        //Debug.Log("Write to: " + videoSavedPath);
        AndroidUtils.RecordFileName = JsonUtility.FromJson<UploadResult>(www.downloadHandler.text).videoUrl;
        Debug.Log("RecordFileName: "+ www.downloadHandler.text);
        ReRecord.uploaded = true;
        if (sendLogToServer)
        {
            foreach (var af in audioFiles)
            {
                if (af.Length > 0)
                {
                    yield return sendLogAudioToServer(af);
                }
            }
            audioFiles.Clear();
        }
    }

    public static StoriesList storiesList;
    public static Dictionary<string, bool> selectedStories = new Dictionary<string, bool>();
    public static IEnumerator GetStories(Action callback)
    {
        UnityWebRequest www = UnityWebRequest.Get($"{HOST}api/v2/myStories/");
        www.timeout = SIMPLE_REQUEST_TIME_OUT;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log(www.error);
            AndroidUtils.ShowToast("获取视频列表失败");
        }
        else
        {
            storiesList = JsonUtility.FromJson<StoriesList>(www.downloadHandler.text);
            Debug.Log("Fetch " + storiesList.stories.Length + " stories.");
            callback();
        }
    }

    public static IEnumerator GetStories(string date, string classname, Action callback)
    {
        string url = $"{HOST}api/v2/query_creations?";
        if (date != null)
        {
            url += "&date=" + date;
        }
        if (classname != null)
        {
            url += "&classname=" + classname;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = SIMPLE_REQUEST_TIME_OUT;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log(www.error);
            AndroidUtils.ShowToast("获取视频列表失败");
        }
        else
        {
            storiesList = JsonUtility.FromJson<StoriesList>(www.downloadHandler.text);
            Debug.Log("Fetch " + storiesList.stories.Length + " stories.");
            callback();
        }
    }

    public static IEnumerator GetClassNames(Action<string[]> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get($"{HOST}api/v2/query_all_classes");
        www.timeout = SIMPLE_REQUEST_TIME_OUT;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log(www.error);
            AndroidUtils.ShowToast("获取班级列表失败");
        }
        else
        {
            string[] results = www.downloadHandler.text.
                Replace("\"", "").Replace("[", "").Replace("]", "").Split(',');
            Debug.Log("Fetch " + results.Length + " classes.");
            callback(results);
        }
    }

    public static IEnumerator ShareTo(string role, Action callback)
    {
        int success = 0, failure = 0;
        foreach (var s in selectedStories.Keys)
        {
            if (selectedStories[s])
            {
                UnityWebRequest www = UnityWebRequest.Get($"{HOST}api/v2/share/get/{role}/{s}");
                www.timeout = SIMPLE_REQUEST_TIME_OUT;
                www.useHttpContinue = false;
                yield return www.SendWebRequest();
                if (www.responseCode != 200)
                {
                    Debug.Log(www.error);
                    failure += 1;
                }
                else
                {
                    var phoneList = www.downloadHandler.text;
                    if (phoneList == "[]") continue;
                    string[] phones = phoneList.Replace("\"", "").Replace("[", "").Replace("]", "").Split(',');
                    foreach(var phone in phones)
                    {
                        if (!string.IsNullOrEmpty(phone))
                        {
                            www = UnityWebRequest.Get($"{HOST}api/v2/share/add/{phone}/{s}");
                            www.timeout = SIMPLE_REQUEST_TIME_OUT;
                            yield return www.SendWebRequest();
                            if (www.responseCode != 200)
                            {
                                Debug.Log(www.error);
                                failure += 1;
                            }
                        }
                    }
                    success += 1;
                }
            }
        }

        if (failure > 0)
        {
            AndroidUtils.ShowToast($"分享成功{success}个故事，失败{failure}个故事");
        } else
        {
            AndroidUtils.ShowToast($"{success}个故事已全部分享成功");
        }
        callback.Invoke();
    }
    public static IEnumerator DeleteStories(Action callback)
    {
        Debug.Log("Delete: " + AndroidUtils.RecordFileName);
        var oid = AndroidUtils.RecordFileName;
        oid = oid.Replace(Server.HOST+"api/v2/uploadedFiles/", "");
        oid = oid.Replace(".mp4", "");
        UnityWebRequest www = UnityWebRequest.Get($"{HOST}api/v2/delete_creation/{oid}");
        www.timeout = SIMPLE_REQUEST_TIME_OUT;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log(www.error);
            AndroidUtils.ShowToast("视频删除失败");
        }
        else
        {
            Debug.Log("Delete Success! ");
            callback();
        }
    }

    public static bool CheckProcessedFinished(string videoUrl)
    {
        var queryUrl = videoUrl.Replace($"{HOST}api/v2/uploadedFiles/", $"{HOST}api/v2/uploadedFileProcessed/");
        WWW reader = new WWW(queryUrl);
        while (!reader.isDone) { }
        return JsonUtility.FromJson<UploadResult>(reader.text).videoUrl.Length > 0;
    }

    public static IEnumerator CombineAudioVideo(string videoUrl, string audioUrl, System.Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("video", videoUrl);
        form.AddField("subtitle", AndroidUtils.recordResult);
        form.AddBinaryData("audio", File.ReadAllBytes(audioUrl), audioUrl);
        UnityWebRequest www = UnityWebRequest.Post($"{HOST}api/combineAudioVideo/{USERNAME}", form);
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log(www.error);
            AndroidUtils.ShowToast("上传失败，请检查网络状况");
            SceneManager.LoadScene("SceneMainMenu");
        }
        else
        {
            Debug.Log("CombineAudioVideo Success!");
            AndroidUtils.RecordFileName = JsonUtility.FromJson<UploadResult>(www.downloadHandler.text).videoUrl;
            Debug.Log("RecordFileName: " + AndroidUtils.RecordFileName);
            callback(AndroidUtils.RecordFileName);
        }
    }

    public static string videoUrl, audioUrl;
    public static void CombineAudioVideo()
    {
        var task = Task.Run(() => PostHTTPRequestAsyncCombineAudioVideo($"{HOST}api/combineAudioVideo/{USERNAME}"));
        task.Wait();
    }

    private static async Task<string> PostHTTPRequestAsyncCombineAudioVideo(string url)
    {
        var fsc = new ByteArrayContent(File.ReadAllBytes(audioUrl));
        fsc.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        var form = new MultipartFormDataContent
        {
            { new StringContent(videoUrl), "video" },
            { new StringContent(AndroidUtils.recordResult), "subtitle" },
            { fsc , "audio", "record.wav"}
        };
        using HttpResponseMessage response = await client.PostAsync(url, form).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    static readonly HttpClient client = new HttpClient();
    private static async Task<string> PostHTTPRequestAsync(string url, Dictionary<string, string> data)
    {
        using HttpContent formContent = new FormUrlEncodedContent(data);
        using HttpResponseMessage response = await client.PostAsync(url, formContent).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public static void sendLogMessageToServer(string logString, string stackTrace, LogType type)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var localLogPath = ResourceUtils.persistentDataPath + "/Logs/unity_" + deviceUniqueIdentifier + "_"
            + System.Diagnostics.Process.GetCurrentProcess().Id + "_" + System.Diagnostics.Process.GetCurrentProcess().StartTime.ToString("yyyy-dd-MMThh-mm-ss") + ".txt";
        if (!File.Exists(localLogPath))
        {
            File.CreateText(localLogPath).Dispose();
        }
        using (StreamWriter sw = File.AppendText(localLogPath))
        {
            sw.WriteLine(DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + " " + type.ToString() +
                " [" + deviceName + "]: " + logString);
            if (stackTrace.Length > 0)
            {
                sw.WriteLine(stackTrace);
            }
        }
#endif
        if (sendLogToServer)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("deviceName", deviceName);
            postData.Add("deviceUniqueIdentifier", deviceUniqueIdentifier);
            postData.Add("pid", System.Diagnostics.Process.GetCurrentProcess().Id + "");
            postData.Add("startTime", System.Diagnostics.Process.GetCurrentProcess().StartTime.ToString("yyyy-dd-MMThh-mm-ss")) ;
            postData.Add("createTime", DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
            postData.Add("message", logString);
            postData.Add("stackTrace", stackTrace);
            postData.Add("level", type.ToString());
            var task = Task.Run(() => PostHTTPRequestAsync($"{HOST}api/v2/log", postData));
            task.Wait();
        }
    }

    public static IEnumerator sendLogAudioToServer(string originalPath)
    {
        if (sendLogToServer)
        {
            WWWForm form = new WWWForm();
            byte[] audioData = File.ReadAllBytes(originalPath);
            Debug.Log("Read " + audioData.Length + " bytes from " + originalPath);
            form.AddBinaryData("audio", audioData, originalPath);
            form.AddField("deviceUniqueIdentifier", deviceUniqueIdentifier);
            form.AddField("originalName", originalPath.Substring(originalPath.IndexOf("CacheData")));
            UnityWebRequest www = UnityWebRequest.Post($"{HOST}api/logAudio", form);
            www.useHttpContinue = false;
            yield return www.SendWebRequest();
            if (www.responseCode != 200)
            {
                Debug.Log("Upload fail: " + www.responseCode + " , reason: " + www.error);
            }
            else
            {
                Debug.Log("Upload success");
            }
        }
    }

    static string timestamp = "";
    public static void refreshTimeStamp ()
    {
        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
    }
    public static IEnumerator sendLogResultTextToServer(string originalPath)
    {
        if (sendLogToServer)
        {
            WWWForm form = new WWWForm();
            byte[] data = File.ReadAllBytes(originalPath);
            Debug.Log("Read " + data.Length + " bytes from " + originalPath);
            form.AddBinaryData("data", data, originalPath);
            form.AddField("deviceUniqueIdentifier", deviceUniqueIdentifier);
            form.AddField("name", timestamp + "_result.txt");
            UnityWebRequest www = UnityWebRequest.Post($"{HOST}api/logResult", form);
            www.useHttpContinue = false;
            yield return www.SendWebRequest();
            if (www.responseCode != 200)
            {
                Debug.Log("Upload fail: " + www.responseCode + " , reason: " + www.error);
            }
            else
            {
                Debug.Log("Upload success");
            }
        }
    }
    public static IEnumerator sendLogJsonFileToServer(string originalPath)
    {
        if (sendLogToServer)
        {
            WWWForm form = new WWWForm();
            byte[] data = File.ReadAllBytes(originalPath);
            Debug.Log("Read " + data.Length + " bytes from " + originalPath);
            form.AddBinaryData("data", data, originalPath);
            form.AddField("deviceUniqueIdentifier", deviceUniqueIdentifier);
            form.AddField("name", timestamp + "_all.json");
            UnityWebRequest www = UnityWebRequest.Post($"{HOST}api/logResult", form);
            www.useHttpContinue = false;
            yield return www.SendWebRequest();
            if (www.responseCode != 200)
            {
                Debug.Log("Upload fail: " + www.responseCode + " , reason: " + www.error);
            }
            else
            {
                Debug.Log("Upload success");
            }
        }
    }

}

[System.Serializable]
public class StoryInfo
{
    public string _id;
    public string time;
    public string name;
    public string duration;
    public string creator;
}
[System.Serializable]
public class StoriesList
{
    public StoryInfo[] stories;
}
[System.Serializable]
class UploadResult
{
    public string videoUrl;
}