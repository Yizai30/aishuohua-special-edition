using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllChangePerformer : Performer
{
    Coroutine coroutine;
    Animation animation = new Animation();

    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation,Vector3 startScale)
    {
        this.setGenGo(GameObject.Instantiate(actorRes.prefab,startPosition,startRotation));
        var clothes = GameObject.Instantiate(actorRes.clothesListDic["All"][0],startPosition,startRotation);
        clothes.name = "clothes";
        clothes.transform.parent = this.genGo.transform;
        this.genGo.name = actorRes.goname;
    }
    public void ChangeClothes(ActorRes actorRes)
    {
        var clothes = GameObject.Instantiate(actorRes.clothesListDic["All"][0], actorRes.go.transform.position, actorRes.go.transform.rotation);
        
        foreach (Transform child in actorRes.go.transform)
        {
            if (child.name == "clothes")
            {
                Destroy(child.gameObject);
                clothes.transform.parent = actorRes.go.transform;

            }
        }

        Animation ani;
        ani = clothes.AddComponent<Animation>();
       
        foreach(AnimationClip aniclip in actorRes.mAnimList)
        {
             
             aniclip.legacy = true;
             ani.AddClip(aniclip, aniclip.name);
                      
        }
        //添加Animation组件，以及该角色的动画列表
       

        clothes.name = "clothes";
    }

    private void Update()
    {
        if (animation == null) return;
        if (animation.isPlaying)
        {
            Debug.Log("hunter is playing");
        }
        else
        {
            Debug.Log("hunter is not playing");
        }
    }

    public override void ChangeAnim(ActorRes actorRes, int loop, float duration, Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
        
        foreach (Transform child in actorRes.go.transform)
        {
            if (child.name == "clothes")
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
                    if (child.name == "clothes")
                    {
                        var animation = child.GetComponent<Animation>();
                        animation.Stop();
                    }
                }
                //actorRes.go.GetComponent<Animation>().Stop();

            }

            yield return new WaitForEndOfFrame();
            //yield return null;
        }


    }


}
