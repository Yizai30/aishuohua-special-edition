using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NlpServer
{
    public static string HOST = "https://test1.deepsoft-tech.com:8443";
    //public static string HOST = "http://127.0.0.1:8000";

    public static SementicList sementicList = new SementicList();

    
    public static IEnumerator GetSementics(string text)
    {
        string url = HOST + "/nlp/srl/{"+text+"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                sementicList = JsonOperator.parseObjFormStr<SementicList>(webRequest.downloadHandler.text);
                //sementicList = JsonUtility.FromJson<SementicList>(webRequest.downloadHandler.text);
                Debug.Log(sementicList.sementics.Length);


            }
        }
    }



   

}


[Serializable]
public class Sementic
{
    public string form;
    public string role;
    public int begin;
    public int end;
}
[Serializable]
public class SementicList
{
    public Sementic[][] sementics { get; set; }

    public List<Sementic> getSubSem(int i)
    {
        List<Sementic> re = new List<Sementic>();
        Sementic sementic= getSemByRoleName(i, "ARG0");
        if (sementic != null) re.Add(sementic);
        return re;
    }

    
    public List<Sementic> getObjSem(int i)
    {
        List<Sementic> re = new List<Sementic>();
        Sementic sementic = getSemByRoleName(i, "ARG1");
        if (sementic != null) re.Add(sementic);
        return re;
    }

    public List<Sementic> getActSem(int i)
    {
        List<Sementic> re = new List<Sementic>();
        Sementic sementic = getSemByRoleName(i, "PRED");
        if (sementic != null) re.Add(sementic);
        return re;
    }

    //可能在状语和宾语里面
    public List<Sementic> getPlaceSem(int i)
    {
        List<Sementic> re = new List<Sementic>();
        Sementic sementic1 = getSemByRoleName(i, "ARGM-ADV");
        if (sementic1 != null) re.Add(sementic1);

        Sementic sementic2 = getSemByRoleName(i, "ARGM-LOC");
        if (sementic2 != null) re.Add(sementic2);

        Sementic sementic3 = getSemByRoleName(i, "ARG1");
        if (sementic3 != null) re.Add(sementic3);

        return re;
    }


     Sementic getSemByRoleName(int i,string role)
    {
        if (i >= sementics.Length) return null;
        Sementic[] sementicsI = sementics[i];
        foreach (Sementic sementic in sementicsI)
        {
            if (sementic.role.Equals(role))
            {
                return sementic;
            }
        }
        return null;
    }

}


