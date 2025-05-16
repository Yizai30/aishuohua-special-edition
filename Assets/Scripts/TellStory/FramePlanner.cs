using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;

namespace AnimGenerator
{
    public class FramePlanner : MonoBehaviour
    {

        //public List<List<KeyFrame>> KeyFrameListBuffer { set; get; }
        public KeyframeListBuffer keyframeListBuffer { set; get; }

        //根据下一个句子内容，修正本句
        public void FixCurByNextFrame()
        {
            if(keyframeListBuffer == null || keyframeListBuffer.keyFrameListList.Count == 0)
            {
                Debug.Log("没有找到keyframeList");
                return;
            }

            if (keyframeListBuffer.keyFrameListList.Count < 2) return;
            for(int i = 0; i < keyframeListBuffer.keyFrameListList.Count-1; i++)
            {
                
                KeyFrameList curFrameList =new KeyFrameList( keyframeListBuffer.keyFrameListList[i]);
                KeyFrameList nextFrameList =new KeyFrameList( keyframeListBuffer.keyFrameListList[i + 1]);

                if (curFrameList.getBkgName() != nextFrameList.getBkgName())
                {
                    return;
                }

                List<int> initFrameIndex = ExtractInitFrameIndex(nextFrameList.keyFrames, i + 1);
                List<KeyFrame> initFrameList = ExtractInitFrameList(nextFrameList.keyFrames, initFrameIndex);
                float duration = curFrameList.getDuration();
                List<KeyFrame> defaultActList = getDefaultActFrame(initFrameList, duration);
                curFrameList.keyFrames.AddRange(initFrameList);
                curFrameList.keyFrames.AddRange(defaultActList);

                List<KeyFrame> newNextFrameList = ExtractExceptInitFrameList(nextFrameList.keyFrames, initFrameIndex);
                keyframeListBuffer.keyFrameListList[i + 1] = newNextFrameList;
                keyframeListBuffer.keyFrameListList[i] = curFrameList.keyFrames;
            }
        }

        //根据新的init帧，添加默认动画帧
        List<KeyFrame> getDefaultActFrame(List<KeyFrame> initFrameList,float duration)
        {
            List<KeyFrame> re = new List<KeyFrame>();
            foreach(KeyFrame keyFrame in initFrameList)
            {
                List<string> classList = DataMap.matSettingList.getMatSettingMap(keyFrame.name).classList;
                if (classList.Contains("prop") || classList.Contains("env_prop")) return re;

                string matName = keyFrame.name;
                string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);

                string actRawName = ActionExtractor.getDefaultRawActionName(matName);
                //string actName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { rawName, actRawName });
                string actName = DataMap.GetActNameMapValue(new List<string> { rawName, actRawName });
                KeyFrame frame = new KeyFrame(2, 2, keyFrame.name, keyFrame.goname, keyFrame.timestamp+0.01f,
            keyFrame.startpos, keyFrame.endpos, keyFrame.startrotation,
            keyFrame.endrotation, keyFrame.startscale, keyFrame.endscale,
            duration, actName, 1, new List<float> { 0, 0, 0 });
                re.Add(frame);
            }
            return re;
        }

        //提取一个帧列表中非出场物品角色的init帧序号
        List<int> ExtractInitFrameIndex( List<KeyFrame> keyFrameList,int kn)
        {
            List<KeyFrame> newKeyframeList = new List<KeyFrame>();

            Recorder recorder = RecorderList.getRecordByNum(kn);
            List<string> comeGoNameList = recorder.comeActor;
            List<int> re = new List<int>();

            for(int i = 0; i < keyFrameList.Count; i++)
            {
                if (keyFrameList[i].action == 0 && !comeGoNameList.Contains(keyFrameList[i].goname))
                {                 
                    re.Add(i);
                }
            }
            return re;
        }

        //提取帧列表中的非出场物品角色的init帧
        List<KeyFrame> ExtractInitFrameList(List<KeyFrame> keyFrameList,List<int> index)
        {
            List<KeyFrame> re = new List<KeyFrame>();
            foreach(int id in index)
            {
                KeyFrame temp = DataUtil.Clone<KeyFrame>(keyFrameList[id]);
                re.Add(temp);
            }
            return re;
        }

        //提取帧列表中的 除了 非出场物品角色的init帧的其他帧
        List<KeyFrame> ExtractExceptInitFrameList(List<KeyFrame> keyFrameList, List<int> index)
        {
           List<KeyFrame> re = new List<KeyFrame>();
           for(int i = 0; i < keyFrameList.Count; i++)
           {
                if (!index.Contains(i))
                {
                    KeyFrame temp = DataUtil.Clone<KeyFrame>(keyFrameList[i]);
                    re.Add(temp);
                    //re.Add(keyFrameList[i]);
                }
           }
           return re;
        }
       
    


    }
}

