using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{

    public void AndroidToUnity(string str)
    {
        //AndroidUtils.ShowToast("Ω” ’µΩ£∫"+ str);
        Server.USERNAME = str;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}

