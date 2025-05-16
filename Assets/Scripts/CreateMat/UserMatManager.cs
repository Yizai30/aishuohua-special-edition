using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class UserMatManager : MonoBehaviour
{

    ServerPoster serverPoster;
    private string userName;

    // Start is called before the first frame update
    void Start()
    {
        userName = Server.getMatCreator();
        serverPoster = GetComponent<ServerPoster>();
    }

    //type:Actor/Prop/Background
    public void deleteActorMat(string matName)
    {
        if (DataMap.privateActorList.ContainMat(matName))
        {
            DataMap.privateActorList.DelMat(matName);
            if (Directory.Exists(Application.persistentDataPath + "/private/actor/" + matName))
            {
                Directory.Delete(Application.persistentDataPath + "/private/actor/" + matName, true);
            }
            
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name", matName));
        }            
    }

    public void deletePropMat(string matName)
    {
        if (DataMap.privatePropList.ContainMat(matName))
        {
            DataMap.privatePropList.DelMat(matName);
            if(Directory.Exists(Application.persistentDataPath + "/private/prop/" + matName))
            {
                Directory.Delete(Application.persistentDataPath + "/private/prop/" + matName,true);
            }
            
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Prop", "Name", matName));
        }
    }

    public void deleteBkgMat(string matName)
    {
        if (DataMap.privateBackgroundList.ContainMat(matName))
        {
            DataMap.privateBackgroundList.DelMat(matName);
            if (Directory.Exists(Application.persistentDataPath + "/private/background/" + matName))
            {
                Directory.Delete(Application.persistentDataPath + "/private/background/" + matName, true);
            }

            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Background", "Name", matName));
        }
    }


    public void renamePropMat(string oldMatname, string newRawName)
    {
        PrivateProp newPrivateProp;
        
        int fixIndex = -1;
        for (int i = 0; i < DataMap.privatePropList.privatePropList.Count; i++)
        {
            if (DataMap.privatePropList.privatePropList[i].Name.Equals(oldMatname))
            {
                fixIndex = i;

            }
        }
        if (fixIndex != -1)
        {
            //修改datamap
            string newMatName = userName + "_" + newRawName;
            string url = "/private/prop/" + newMatName;
            newPrivateProp = new PrivateProp(newMatName, newRawName, userName, url);
            DataMap.privatePropList.privatePropList[fixIndex] = newPrivateProp;

            //修改数据库
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Prop", "Name", oldMatname));

            StartCoroutine(serverPoster.AddElemToDataBase("Huiben_Private_Material", "Prop", newPrivateProp));
            //修改本地素材文件名称
            renameLocalMatFile("/private/prop/" + oldMatname, "/private/prop/" + newMatName, oldMatname + ".png", newMatName + ".png");
        }
       

    }

    public void renameActorMat(string oldMatname, string newRawName)
    {
        PrivateActor newPrivateActor;

        int fixIndex = -1;
        for (int i = 0; i < DataMap.privateActorList.privateActorList.Count; i++)
        {
            if (DataMap.privateActorList.privateActorList[i].Name.Equals(oldMatname))
            {
                fixIndex = i;

            }
        }
        if (fixIndex != -1)
        {
            //修改datamap
            string newMatName = userName + "_" + newRawName;
            string url = "/private/actor/" + newMatName;
            newPrivateActor = new PrivateActor(newMatName, newRawName, userName, url,"human");
            DataMap.privateActorList.privateActorList[fixIndex] = newPrivateActor;

            //修改数据库
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name", oldMatname));

            StartCoroutine(serverPoster.AddElemToDataBase("Huiben_Private_Material", "Actor", newPrivateActor));
            //修改本地素材文件名称
            renameLocalMatFile("/private/actor/" + oldMatname, "/private/actor/" + newMatName, oldMatname + ".png", newMatName + ".png");
        }
    }

    public void renameBkgMat(string oldMatname, string newRawName)
    {
        PrivateBackground newPrivateBackground;

        int fixIndex = -1;
        for (int i = 0; i < DataMap.privateBackgroundList.privateBackgroundList.Count; i++)
        {
            if (DataMap.privateBackgroundList.privateBackgroundList[i].Name.Equals(oldMatname))
            {
                fixIndex = i;

            }
        }
        if (fixIndex != -1)
        {
            //修改datamap
            string newMatName = userName + "_" + newRawName;
            string url = "/private/background/" + newMatName;
            newPrivateBackground = new PrivateBackground(newMatName, newRawName, userName, url);
            DataMap.privateBackgroundList.privateBackgroundList[fixIndex] = newPrivateBackground;

            //修改数据库
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Background", "Name", oldMatname));

            StartCoroutine(serverPoster.AddElemToDataBase("Huiben_Private_Material", "Background", newPrivateBackground));
            //修改本地素材文件名称
            renameLocalMatFile("/private/background/" + oldMatname, "/private/background/" + newMatName, oldMatname + ".png", newMatName + ".png");
        }
    }

    void renameLocalMatFile(string oldUrl,string newUrl,string oldFileName,string newFileName)
    {
        oldUrl = Application.persistentDataPath + "/" + oldUrl;
        newUrl = Application.persistentDataPath + "/" + newUrl;
        Directory.CreateDirectory( newUrl);
        if (File.Exists(oldUrl + "/" + oldFileName))
        {
            File.Move(oldUrl + "/" + oldFileName,
                newUrl + "/" + newFileName);
        }
        if (File.Exists(oldUrl))
        {
            Directory.Delete(oldUrl, true);
        }
        
    }
}
