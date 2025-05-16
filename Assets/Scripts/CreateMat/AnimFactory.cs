using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFactory : MonoBehaviour
{
    public static AnimationClip genStretchAnimClip(float originScale, string direction)
    {
        //AnimationCurve scaleX = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 3.0f);
        AnimationCurve scale = new AnimationCurve();
        scale.AddKey(new Keyframe(0, 1* originScale));
        scale.AddKey(new Keyframe(1, 1.3f*originScale));
        scale.AddKey(new Keyframe(2, 1*originScale));

        AnimationCurve originScaleCurve = new AnimationCurve();
        originScaleCurve.AddKey(new Keyframe(0, originScale));
        originScaleCurve.AddKey(new Keyframe(1, originScale));
        originScaleCurve.AddKey(new Keyframe(2, originScale));

        //AnimationCurve scaleZ = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 3.0f);
        AnimationClip animationClip = new AnimationClip();


        animationClip.legacy = true;
        //animationClip.SetCurve("", typeof(Transform), "localScale.x", scaleX);
        if (direction == "x")
        {
            animationClip.SetCurve("", typeof(Transform), "localScale.x", scale);
            animationClip.SetCurve("", typeof(Transform), "localScale.y", originScaleCurve);
            animationClip.SetCurve("", typeof(Transform), "localScale.z", originScaleCurve);
        }
        else if (direction == "y")
        {
            animationClip.SetCurve("", typeof(Transform), "localScale.y", scale);
            animationClip.SetCurve("", typeof(Transform), "localScale.x", originScaleCurve);
            animationClip.SetCurve("", typeof(Transform), "localScale.z", originScaleCurve);
        }
        else
        {
            animationClip.SetCurve("", typeof(Transform), "localScale.y", scale);
            animationClip.SetCurve("", typeof(Transform), "localScale.x", originScaleCurve);
            animationClip.SetCurve("", typeof(Transform), "localScale.z", originScaleCurve);
        }
        return animationClip;

    }


    public static void createAnim(GameObject gameObject)
    {
        
        
        //testGo.layer = 6;
        Animation animation = gameObject.AddComponent<Animation>();

        AnimationClip animationClip = AnimFactory.genStretchAnimClip(1,"y");

        animation.AddClip(animationClip, "testAnim");

        animation.Play("testAnim");



    }
}
