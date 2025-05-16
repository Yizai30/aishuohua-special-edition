using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Performer : MonoBehaviour
{
    protected GameObject genGo;//封装位置等信息后生成的go，传回
    protected Animation mAnim;//挂在物体的Animation组件

    //各个部位的gameobject字典
    protected Dictionary<string, GameObject> partGoDic = new Dictionary<string, GameObject>();

    //当前go的位置和角度
    public Vector3 position;
    public Quaternion rotation;

    public Image panel;

   
    public GameObject getGenGo()
    {
        return genGo;
    }
    public void setGenGo(GameObject go)
    {
        this.genGo = go;
    }
    //指定父go的名字
    public void setName(string name)
    {
        gameObject.name = name;
    }

    public abstract void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation,Vector3 startScale);


    public abstract void ChangeAnim(ActorRes actorRes, int loop, float duration,
        Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot);
   

    public GameObject getGameObject()
    {
        return this.genGo;
    }


    protected void setupPosZ(ref Vector3 pos, string matName)
    {
        MatSettingElement settingElement = DataMap.matSettingList.getMatSettingMap(matName);
        if (settingElement.classList.Contains("env_prop"))
        {
            pos[2] = -1;
        }
        else if (settingElement.classList.Contains("prop"))
        {
            pos[2] = -2;
        }
        else
        {
            pos[2] = -3;
        }


    }

}
