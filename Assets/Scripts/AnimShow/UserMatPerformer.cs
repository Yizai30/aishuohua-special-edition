using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMatPerformer : Performer
{
    Coroutine coroutine; 
    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation,Vector3 startScale)
    {
       
        GameObject userGo = GameObject.Instantiate(actorRes.prefab, startPosition, startRotation,panel.transform);
        userGo.name = actorRes.goname;
        Animation animation = userGo.AddComponent<Animation>();

        Debug.Log("testScale:" + userGo.transform.localScale);
        Debug.Log("testScale:" + userGo.transform.lossyScale);
        AnimationClip animationClip = AnimFactory.genStretchAnimClip(startScale.y,"y");

        animation.AddClip(animationClip, "testAnim");


        setupPosZ(ref startPosition, actorRes.prefabname);
        userGo.transform.localPosition = startPosition;
        userGo.transform.localRotation = startRotation;
        userGo.transform.localScale = startScale;
        this.setGenGo(userGo);

    }
   
    
    public override void ChangeAnim(ActorRes actorRes,int loop,float duration,
        Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
        
        Animator animator = actorRes.go.GetComponent<Animator>();
        if(animator==null || animator.enabled==false)
        {
            Animation animation = actorRes.go.GetComponent<Animation>();
            if (loop == 1)
            {
                animation.wrapMode = WrapMode.Loop;
            }
            else
            {
                animation.wrapMode = WrapMode.Once;
            }

            //test
            actorRes.mAnimName = "testAnim";

            if (!actorRes.mAnimName.Equals(""))
            {
                animation.Play(actorRes.mAnimName);

            }         

        }
        else
        {
            
            animator.enabled = true;
            Animation animation = actorRes.go.GetComponent<Animation>();
            if (animation != null)
            {
                animation.enabled = false;
            }         
            

            animator.SetTrigger(actorRes.mAnimName);
            
        }

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(Animate(actorRes, duration, startPos, endPos, startScale,
               endScale, startRot, endRot));
        //注意这里要是不开新的协程，会一直受上一个协程的干扰停止播放动画
       

        //StartCoroutine(stopAnim(animation, duration));

    }



    private IEnumerator Animate(ActorRes actorRes,float duration, Vector3 startPos, Vector3 endPos, 
        Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
       
        float elapsed = 0;
        
        //Debug.Log(actorRes.mAnimName);
        //Debug.Log("el:" + elapsed);
        // 插值改变物体 transform
        Transform transform = actorRes.go.transform;
        setupPosZ(ref startPos, actorRes.prefabname);
        //transform.SetPositionAndRotation(startPos, startRot);
        transform.localPosition = startPos;
        transform.localRotation = startRot;
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            setupPosZ(ref startPos, actorRes.prefabname);
            setupPosZ(ref endPos, actorRes.prefabname);
            transform.localPosition = LerpUtility.Linear(startPos, endPos, elapsed / duration);
            transform.localRotation = LerpUtility.Linear(startRot, endRot, elapsed / duration);
            //transform.SetPositionAndRotation(
            //    LerpUtility.Linear(startPos,endPos, elapsed / duration),
            //    LerpUtility.Linear(startRot, endRot, elapsed / duration)
            //    );

            //龙骨动画不用指定何时停止，unity动画不能指定循环次数，需要指定停止
            elapsed += Time.deltaTime;
            if (elapsed >= duration && actorRes.ActorType.Substring(0, 2) != "db")
            {
                if (actorRes.go.GetComponent<Animation>() != null)
                {
                    actorRes.go.GetComponent<Animation>().Stop();
                }
                else
                {
                    actorRes.go.GetComponent<Animator>().speed = 0;
                }
                
                
            }

            yield return new WaitForEndOfFrame();
            //yield return null;
        }
        

    }

}
