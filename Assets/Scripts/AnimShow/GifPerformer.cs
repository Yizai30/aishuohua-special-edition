using AnimGenerator;
using DragonBones;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GifPerformer : Performer
{
    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation, Vector3 startScale)
    {
        GameObject genGo = GameObject.Instantiate(actorRes.prefab, startPosition, startRotation, panel.transform);
        setupPosZ(ref startPosition, actorRes.prefabname);
        genGo.transform.localPosition = startPosition;

        genGo.transform.localRotation = startRotation;
        genGo.transform.localScale = startScale;
        this.setGenGo(genGo);

        this.genGo.name = actorRes.goname;

        //this.setGenGo(GameObject.Instantiate(actorRes.prefab, startPosition, startRotation));
    }




    public override void ChangeAnim(ActorRes actorRes, int loop, float duration,
        Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
        // 播放 gif 动画
        UniGifImage uniGifImage = actorRes.go.GetComponent<UniGifImage>();
        //UnityArmatureComponent armatureComp = actorRes.go.GetComponent<UnityArmatureComponent>();

        string content = actorRes.mAnimName;

        PlayableGif playableGif = DataMap.matGifCollection.FindPlayableGif(content);

        //if (content == "")
        //{
        //    print("content为空");
        //    return;
        //}

        if (content=="" || playableGif == null ) {
            print("没有这个素材" + content+"使用默认素材"+actorRes.prefabname+"_walk");
            playableGif = DataMap.matGifCollection.FindPlayableGif(actorRes.prefabname+"_walk");
            if (playableGif == null)
            {
                print("缺少默认素材");
                return;
            }
            
        }
       
        uniGifImage.playableGif = playableGif;
        uniGifImage.nowState = UniGifImage.State.Ready;
        uniGifImage.Play();


        StartCoroutine(Animate(actorRes, duration, startPos, endPos, startScale,
            endScale, startRot, endRot));
    }

    private IEnumerator Animate(ActorRes actorRes, float duration, Vector3 startPos, Vector3 endPos,
        Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {

        UniGifImage uniGifImage = actorRes.go.GetComponent<UniGifImage>();

        float elapsed = 0;
        // 插值改变物体 transform
        UnityEngine.Transform transform = actorRes.go.transform;
        setupPosZ(ref startPos, actorRes.prefabname);
        transform.localPosition = startPos;

        transform.localRotation = startRot;
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            setupPosZ(ref startPos, actorRes.prefabname);
            setupPosZ(ref endPos, actorRes.prefabname);
            transform.localPosition = LerpUtility.Linear(startPos, endPos, elapsed / duration);

            transform.localRotation = LerpUtility.Linear(startRot, endRot, elapsed / duration);

            //龙骨动画不用指定何时停止，unity动画不能指定循环次数，需要指定停止
            elapsed += Time.deltaTime;
            if (elapsed >= duration)
            {
                uniGifImage.Stop();
            }

            yield return new WaitForEndOfFrame();
        }

    }
}
