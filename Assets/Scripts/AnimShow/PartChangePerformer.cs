using AnimGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartChangePerformer : Performer
{
    public GameObject mSkeleton;
    Coroutine coroutine; 
    private void DestroyAll()
    {
        if (mSkeleton != null)
            GameObject.DestroyImmediate(mSkeleton);
        partGoDic.Clear();
        //foreach (var key in partGoDic.Keys.ToList())
        //{
        //    partGoDic[key] = null;
        //}
    }

    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation,Vector3 startScale)
    {
        DestroyAll();
        partGoDic.Clear();
        /*
        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            GameObject go = new GameObject();
            partGoDic.Add(code, go);
        }
        */

        foreach(var name in actorRes.clothesNameDic.Keys)
        {
            GameObject go = new GameObject();
            partGoDic.Add(name, go);
        }
        Destroy(actorRes.go);
        this.genGo = Instantiate(actorRes.prefab, startPosition, startRotation);
        actorRes.go = this.genGo;
        mSkeleton = GameObject.Instantiate(actorRes.mSkeleton, startPosition, startRotation);
      
        mSkeleton.Reset(this.genGo);
        mSkeleton.transform.localPosition = new Vector3(0, 0, 0);
        mSkeleton.name = actorRes.mSkeleton.name;

        mAnim = mSkeleton.GetComponent<Animation>();

        /*
        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            if (code != (int)EPart.EP_All)
            {
                ChangeEquip(code, actorRes);
            }

        }
        */
        foreach(var name in actorRes.clothesNameDic.Keys)
        {
            ChangeEquip(name, actorRes);
        }

        //ChangeAnim(actorRes,0,0);//人形动画还未完善
    }

    void ChangeClothes(ActorRes actorRes)
    {

    }

    /// <summary>
    /// 更换某个部位的服装 
    /// </summary>
    /// <param name="type">部位枚举名</param>
    /// <param name="ActorRes">包含各个部位的资源索引号</param>
    public void ChangeEquip(string partName, ActorRes actorRes)
    {
        GameObject tmpGo = partGoDic[partName];
        string partPreName = actorRes.clothesNameDic[partName];
        var partPreList = actorRes.clothesListDic[partName];
        GameObject tarGo = new GameObject();
        foreach(var go in partPreList)
        {
            if (go.name == partPreName)
            {
                tarGo = go;
            }
        }
        ChangeEquip(ref tmpGo, tarGo);
        partGoDic[partPreName] = tmpGo;

    }

    /// <summary>
    /// 指定类型进行该部位gameobject的替换操作
    /// </summary>
    private void ChangeEquip(ref GameObject go, GameObject resgo)
    {
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
        }

        go = GameObject.Instantiate(resgo);
        go.Reset(mSkeleton);//设置
        go.name = resgo.name;

        //获取render组件
        SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
        ShareSkeletonInstanceWith(render, mSkeleton);
    }

    // 共享骨骼
    public void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    {
        Transform[] newBones = new Transform[selfSkin.bones.Length];
        for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
        {
            GameObject bone = selfSkin.bones[i].gameObject;

            // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
            newBones[i] = FindChildRecursion(target.transform, bone.name);
        }

        selfSkin.bones = newBones;
    }

    // 递归查找
    public Transform FindChildRecursion(Transform t, string name)
    {
        foreach (Transform child in t)
        {
            if (child.name == name)
            {
                return child;
            }
            else
            {
                Transform ret = FindChildRecursion(child, name);
                if (ret != null)
                    return ret;
            }
        }

        return null;
    }

    public override void ChangeAnim(ActorRes actorRes, int loop, float duration,
        Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {

        //Animation animation = actorRes.go.GetComponent<Animation>();
        Animation animation = new Animation();
        foreach (Transform child in actorRes.go.transform)
        {
            if(child.name== "skeleton")
            {
                animation = child.GetComponent<Animation>();
            }
        }
        if (loop == 1)
        {
            animation.wrapMode = WrapMode.Loop;
        }
        else
        {
            animation.wrapMode = WrapMode.Once;
        }
        animation.Play(actorRes.mAnimName);
        //注意这里要是不开新的协程，会一直受上一个协程的干扰停止播放动画
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(Animate(actorRes, duration, startPos, endPos, startScale,
            endScale, startRot, endRot));

        //StartCoroutine(stopAnim(animation, duration));

    }



    private IEnumerator Animate(ActorRes actorRes, float duration, Vector3 startPos, Vector3 endPos,
        Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
        var anim = actorRes.go.GetComponent<Animation>();
        float elapsed = 0;

        Debug.Log(actorRes.mAnimName);
        Debug.Log("el:" + elapsed);
        // 插值改变物体 transform
        Transform transform = actorRes.go.transform;
        transform.SetPositionAndRotation(startPos, startRot);
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            transform.SetPositionAndRotation(
                LerpUtility.Linear(startPos, endPos, elapsed / duration),
                LerpUtility.Linear(startRot, endRot, elapsed / duration)
                );

            //龙骨动画不用指定何时停止，unity动画不能指定循环次数，需要指定停止
            elapsed += Time.deltaTime;
            if (elapsed >= duration && actorRes.ActorType.Substring(0, 2) != "db")
            {
                foreach (Transform child in actorRes.go.transform)
                {
                    if (child.name == "skeleton")
                    {
                        child.GetComponent<Animation>().Stop();
                    }
                }
                

            }

            yield return new WaitForEndOfFrame();
            //yield return null;
        }


    }

}
