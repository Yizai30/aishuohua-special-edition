using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{

    public void AndroidToUnity(string str)
    {
        //AndroidUtils.ShowToast("���յ���"+ str);
        Server.USERNAME = str;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}

