using Assets.Scripts.TellStory.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCutLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Test("知恩的爸爸妈妈想带知恩出去玩儿。" +
            "天气好热啊！" +
            "知恩要是大量出汗，会加重皮炎的。" +
            "爸爸妈妈很担心知恩的敏感皮肤。" +
            "没关系！知恩高兴地喊着。" +
            "知恩和爸爸妈妈来到游乐园。" +
            "动物玩偶们随着音乐愉快地前进。" +
            "知恩坐在大熊玩偶的肩膀上，跟着动物们一起向前走。" +
            "过了好一会儿，大熊玩偶在喷泉面前把知恩放了下来。" +
            "知恩向四周看了看，说，咦，爸爸妈妈呢？" +
            "太阳火辣辣的，知恩又着急，没一会儿就满头大汗了。" +
            "知恩！爸爸和妈妈跑了过来。" +
            "知恩强忍着的眼泪一下子就涌了出来。" +
            "你怎么可以自己随便乱跑，爸爸妈妈多担心你。" +
            "妈妈紧紧抱住知恩。" +
            "迷路的时候一定要站在原地不动，你可以向周围的工作人员求救，" +
            "要说出你的名字和爸爸妈妈的联系方式，知道了吗？爸爸紧紧地抓住知恩的手说。" +
            "回到家后，知恩洗了个温水澡，但是洗澡后全身开始痒痒。" +
            "难道皮炎又犯了？趁着不是很严重，赶紧去找叮咚医生吧。妈妈看着知恩说。" +
            "叮咚，叮咚，知恩和爸爸妈妈一起来到叮咚医院。" +
            "叮咚医生，我全身痒痒。知恩一边挠着痒痒一边跟医生说。" +
            "出汗太多导致皮肤干燥，或是皮肤缺乏营养时，还有身体疲劳或压力大时，都会加重皮炎。" +
            "医生仔细观察了知恩的皮肤后说。");
    }

    void Test(string line)
    {
        AsrMain.usrDicPath = Application.persistentDataPath + "/jiebaConfig/usr_dic.txt";
        foreach (string r in AsrMain.resourceFiles)
        {
            AsrMain.requests[r] = new WWW(ResourceUtils.getReaderPath(r));
        }
        while (!AsrMain.allRequestsDone()) { }

        AsrMain.InitJieba();
        AsrMain asrMain = GetComponent<AsrMain>();
        string re = asrMain.cutLine(line);
        print(re);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
