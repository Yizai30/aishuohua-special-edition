using AnimGenerator;
using DragonBones;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DbPerformer : Performer
{
    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation,Vector3 startScale)
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


    public static void ChangeArmatureData(UnityArmatureComponent _armatureComponent, string armatureName, string dragonBonesName)
    {
        bool isUGUI = _armatureComponent.isUGUI;
        UnityDragonBonesData unityData = null;
        Slot slot = null;
        if (_armatureComponent.armature != null)
        {
            unityData = _armatureComponent.unityData;
            slot = _armatureComponent.armature.parent;
            _armatureComponent.Dispose(false);

            UnityFactory.factory._dragonBones.AdvanceTime(0.0f);

            _armatureComponent.unityData = unityData;
        }

        _armatureComponent.armatureName = armatureName;
        _armatureComponent.isUGUI = isUGUI;

        _armatureComponent = UnityFactory.factory.BuildArmatureComponent(_armatureComponent.armatureName, dragonBonesName, null, _armatureComponent.unityData.dataName, _armatureComponent.gameObject, _armatureComponent.isUGUI);
        if (slot != null)
        {
            slot.childArmature = _armatureComponent.armature;
        }

        _armatureComponent.sortingLayerName = _armatureComponent.sortingLayerName;
        _armatureComponent.sortingOrder = _armatureComponent.sortingOrder;
    }



    public override void ChangeAnim(ActorRes actorRes, int loop, float duration,
        Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {

        // 播放 DrangonBones 动画
       
        UnityArmatureComponent armatureComp = actorRes.go.GetComponent<UnityArmatureComponent>();
       
        string content = actorRes.mAnimName;
       



        if (content != "")
        {
            //armatureComp.armatureBaseName = content;
            //if(content== "db_speak" || content == "db_eat")
            //{
            //    ChangeArmatureData(armatureComp, content, actorRes.prefabname);
            //}
            //else
            //{
            //    ChangeArmatureData(armatureComp, "Armature", actorRes.prefabname);
            //}

            DragonBones.Animation animation = armatureComp.animation;
            Dictionary<string, AnimationData> animations = animation.animations;


            float length = animations[content].duration;
            if (loop > 0)
                animation.timeScale = length / (duration / loop);
            else
                animation.timeScale = 1;
            animation.Play(content, loop);
        }
        
        
        StartCoroutine(Animate(actorRes, duration, startPos, endPos, startScale,
            endScale, startRot, endRot));
    }

    private IEnumerator Animate(ActorRes actorRes, float duration, Vector3 startPos, Vector3 endPos,
        Vector3 startScale, Vector3 endScale, Quaternion startRot, Quaternion endRot)
    {
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
            if (elapsed >= duration && actorRes.ActorType.Substring(0, 2) != "db")
            {
                actorRes.go.GetComponent<UnityEngine.Animation>().Stop();
            }

            yield return new WaitForEndOfFrame();
        }

    }
}
