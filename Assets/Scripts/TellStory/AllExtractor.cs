 using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AnimGenerator;
using System.Text.RegularExpressions;
using System;
using System.Reflection;
using UnityEngine.Profiling;
 
 
public class EventsWrapper
{
    public string storyContent { get; set; }
    public List<BasicEvent> eventList { get; set; }

    public EventsWrapper(string storyContent, List<BasicEvent> eventList)
    {
        this.storyContent = storyContent;
        this.eventList = eventList;
    }
}

//从标注好词性的文本中提取event的元素
public class AllExtractor :MonoBehaviour
{

    public string line = "";

    public int stateNum = 0;

    public List<BasicEvent> eventList = new List<BasicEvent>();
    
    public List<EventsWrapper> allEventWrapperList = new List<EventsWrapper>();

    public KeyFrameList keyFrameList = new KeyFrameList();

    public List<KeyValuePair<string, string>> pairList = new List<KeyValuePair<string, string>>();

    public string subtitleText = "";

    //public string speakLine = "";

    public string bkgName = "";

    public bool bkgChange = false;

    float eventDuration = 5.0f;
    float eventSeqNum = 0;//event序列号

    string storyName = "";



    Recorder curRecorder;

    SementicList sementicList=new SementicList();//得到的语义标注

    public bool extractFinished = false;


    public void setStartVal(string line,float eventDuration,string storyName,int stateNum)
    {
        this.line = line;
        this.eventDuration = eventDuration;
        this.stateNum = stateNum;
        this.storyName = storyName;
        curRecorder = RecorderList.getRecordByNum(stateNum);

        pairList = new List<KeyValuePair<string, string>>();
        eventList = new List<BasicEvent>();
        keyFrameList = new KeyFrameList();
       

        bkgName = "";

        bkgChange = false;

        sementicList = new SementicList();

        extractFinished = false;
        eventSeqNum = 0;
    }

 

    public void extractEvent()
    {
        List<KeyValuePair<string, string>> tmpPairList = getkvList(this.line);
        if (tmpPairList.Count == 0)
        {
            throw new Exception("没有从输入提取到任何信息");
        }

        //纠正词性
        List<KeyValuePair<string, string>> correctPairList = new List<KeyValuePair<string, string>>();
        foreach (KeyValuePair<string, string> pair in tmpPairList)
        {
            string correctFlag = DataMap.correctList.getCorrectFlag(pair.Value);
            KeyValuePair<string, string> tmpPair;
            if (correctFlag != "")
            {
                tmpPair = new KeyValuePair<string, string>(correctFlag, pair.Value);
                //Debug.Log("已修正词语:" + pair.Value + " 修正后的词性为:" + correctFlag);

            }
            else
            {
                tmpPair = new KeyValuePair<string, string>(pair.Key, pair.Value);
            }
            correctPairList.Add(tmpPair);
        }

        //生成语音事件
        BasicEvent audioEvent = new BasicEvent(BasicEvent.BasicEventType.audio, "", "", "", "", eventSeqNum, 0, "");
        eventList.Add(audioEvent);

        //生成字幕事件      
        BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.subtitle, "", "", "", "", eventSeqNum, 0, "");
        eventList.Add(actEvent);

        //尝试根据拼音补全主语
        //addSubByPinyin();
        //if (cutPairList.Count == 0) return;
        inspectPinyin(correctPairList, 0, correctPairList.Count - 1);
        //更新字幕内容
        this.subtitleText = extractPureText(correctPairList);

        //截取截断词
        List<KeyValuePair<string, string>> cutPairList = cutLine(correctPairList);

        //cutPairList = cutLine(correctPairList);




        //排除a的b
        //List<KeyValuePair<string, string>> handleHasPairList = handleHas(cutPairList);

        // pairList = handleHasPairList;
        pairList = cutPairList;
        curRecorder.pairList = this.pairList;

        //用hanlp获取语义角色列表
        //StartCoroutine(getSementic(GetSementicFinished,useExtractSuit));

      

        legacyExtractEvent();
       


    }

    public void extractBkgEvent()
    {
        //*******提取bkgEvent******
        float bkgEventsLen = 0.0001f;
        BackgroundExtractor backgroundExtractor = new BackgroundExtractor(this.pairList, eventSeqNum, curRecorder, bkgEventsLen);
        try
        {
            backgroundExtractor.extractEvent();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
        this.bkgName = backgroundExtractor.getCurBkgName();
        this.bkgChange = backgroundExtractor.getCurBkgIsChanged();

        this.eventList.AddRange(backgroundExtractor.eventList);

        //eventSeqNum = eventSeqNum + bkgEventsLen;
        eventSeqNum = eventSeqNum + backgroundExtractor.eventsLen;
    }

    public void extractInitEvent()
    {
        float initEventsLen = 0.001f;
        InitExtractor initExtractor = new InitExtractor(pairList, eventSeqNum, curRecorder, initEventsLen, this.bkgName, this.bkgChange);
        try
        {
            initExtractor.extractEvent();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        if (initExtractor.eventList.Count != 0)
        {
            this.eventList.AddRange(initExtractor.eventList);
            eventSeqNum = eventSeqNum + initEventsLen;
        }
    }

    public void extractActionEvent()
    {
        float actEventsLen = 1.5f;
        ActionExtractor actionExtractor = new ActionExtractor(pairList, eventSeqNum, curRecorder, actEventsLen, this.bkgName, this.bkgChange);
        try
        {
            actionExtractor.extractEvent();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

        if (actionExtractor.eventList.Count != 0)
        {
            this.eventList.AddRange(actionExtractor.eventList);
            eventSeqNum += actEventsLen;
        }
    }

    public void extractActionEvent_sementic()
    {
        float actEventsLen = 1.5f;
        SementicActionExtractor actionExtractor = new SementicActionExtractor(pairList, eventSeqNum, curRecorder, actEventsLen, this.bkgName, this.bkgChange,this.sementicList);
        try
        {
            actionExtractor.extractEvent();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

        if (actionExtractor.eventList.Count != 0)
        {
            this.eventList.AddRange(actionExtractor.eventList);
            eventSeqNum += actEventsLen;
        }
    }

    //提取事件流，要规划时间和顺序
    public void  setEventTime()
    {                  
        //整理event的starttime和duration 
        foreach (BasicEvent basicEvent in this.eventList)
        {
           
            if (eventSeqNum == 0)
            {
                basicEvent.startTime = 0f;
                basicEvent.duration = eventDuration;
            }
            
            else
            {
                if (basicEvent.eventType.Equals(BasicEvent.BasicEventType.subtitle)||
                    basicEvent.eventType.Equals(BasicEvent.BasicEventType.audio))
                {
                    basicEvent.startTime = 0f;
                    basicEvent.duration = this.eventDuration;
                }
                else
                {
                   // int currEventSeq = basicEvent.startTime;
                    float startTime = basicEvent.startTime / eventSeqNum * eventDuration;
                    basicEvent.startTime = startTime;
                    basicEvent.duration = basicEvent.duration / eventSeqNum*eventDuration;
                }
                
            }
            
        }
              
         
    }
    //根据事件，生成帧
    public void genKeyframes()
    {

        KeyFrameList keyFrameList = new KeyFrameList();
      
        //for each event,genarate keyframe 
        foreach(BasicEvent basicEvent in this.eventList)
        {
           
            //用规划器生成帧
            Planner pl = plannerSelect(basicEvent);

            //转换名称
            basicEvent.mapElementName();
            //规划
            try
            {
                pl.planning();
            }catch(Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }
            
            if (pl.keyFrames.Count != 0)
            {
                keyFrameList.keyFrames.AddRange(pl.keyFrames);
            }
            
        }

        keyFrameList.keyFrames = delDefaultKeyframe(keyFrameList.keyFrames);

        this.keyFrameList.keyFrames.AddRange(keyFrameList.keyFrames);

        addUserMatAnim(keyFrameList.keyFrames);
        //记录当前句子的keyframe结果
        curRecorder.setKeyframeList(this.keyFrameList.keyFrames);
    }


   

    //根据不同种类，选择规划器
    public Planner plannerSelect(BasicEvent basicEvent) 
    {
        Planner planner = null;
        if (basicEvent.eventType == BasicEvent.BasicEventType.background)
        {
            planner = new BkgPlanner(basicEvent,curRecorder);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.subtitle)
        {
            planner = new SubPlanner(basicEvent,curRecorder, subtitleText);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.audio)
        {
            planner = new AudioPlanner(basicEvent,curRecorder, this.storyName+RecorderList.stateNum);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.init)
        {
            string actionRawName = getInitActionName(basicEvent);

            planner = new InitPlanner(basicEvent,curRecorder,actionRawName);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.disappear)
        {
            planner = new DisPlanner(basicEvent,curRecorder);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.attrChange)
        {
            planner = new AttrChangePlanner(basicEvent,curRecorder);
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.action)
        {
            //subject 
            if ((!basicEvent.subjectName.Equals("")) && basicEvent.objectName.Equals("") && basicEvent.actionName.Equals(""))
            {
                string actionRawName = getInitActionName(basicEvent);
                planner = new InitPlanner(basicEvent,curRecorder,actionRawName);
                return planner;
            }
            //subject action
            else if ((!basicEvent.subjectName.Equals("")) && (basicEvent.objectName.Equals("")) && (!basicEvent.actionName.Equals("")))
            {
                //string actName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { basicEvent.subjectName, basicEvent.actionName });
                string actName = DataMap.GetActNameMapValue(new List<string> { basicEvent.subjectName, basicEvent.actionName });
                string actType = DataMap.actSettingList.getActSettingEle(actName).actType;
                if (actType.Equals("movable"))
                {

                    planner = new SingleMoveActionPlanner(basicEvent, curRecorder, basicEvent.actionName);

                }
                else
                {
                    planner = new SingleStaticActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                }
            }
            //subject action object
            else if ((!basicEvent.subjectName.Equals("")) && (!basicEvent.objectName.Equals("")) && (!basicEvent.actionName.Equals("")))
            {
                //string actName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { basicEvent.subjectName, basicEvent.actionName });
                string actName = DataMap.GetActNameMapValue(new List<string> { basicEvent.subjectName, basicEvent.actionName });
                string actType = DataMap.actSettingList.getActSettingEle(actName).actType;
                if (actType.Equals("couple_static"))
                {
                    planner = new CoupleStaticActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                }
                else if (actType.Equals("movable"))
                {
                    planner = new CoupleMoveActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                }
                else if (actType.Equals("single_static"))
                {
                    planner = new SingleStaticActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                }
            }
        }
        else if (basicEvent.eventType == BasicEvent.BasicEventType.n_action)
        {
            
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvent.actionName);
            Type plannerType =Type. GetType(diyActElement.defaultPlanner);
            ConstructorInfo ctor = plannerType.GetConstructor(new[] { typeof(BasicEvent),typeof(Recorder),typeof(string) });
            planner = (Planner)ctor.Invoke(new object[] { basicEvent,curRecorder,basicEvent.actionName });

            if(correctN_actionPlanner(basicEvent,planner) != null)
            {
                planner = correctN_actionPlanner(basicEvent,planner);
            }


        }
        else
        {
            throw new Exception("未知的事件类型");
        }
        if (planner == null)
        {
            throw new Exception("对于该事件没有合适的规划器");
        }
        //Debug.Log("主语：" + basicEvent.subjectName + "动作：" + basicEvent.actionName + "选择规划器：" + planner.GetType());
        return planner;
    }

    
    private string getInitActionName(BasicEvent basicEvent)
    {
        string subName = basicEvent.subjectName;
        string actionRawName = "";
        foreach (BasicEvent tempBasicEvent in eventList)
        {
            if ((tempBasicEvent.eventType == BasicEvent.BasicEventType.action || tempBasicEvent.eventType==BasicEvent.BasicEventType.n_action)
                && subName.Equals(tempBasicEvent.subjectName))
            {
                actionRawName = tempBasicEvent.actionName;
                break;
            }
        }
        return actionRawName;
    }

    //从句子中获取单词-词性对应表
    private List<KeyValuePair<string,string>> getkvList(string inputLine)
    {
        string line = inputLine;
        List<string> wordList = new List<string>();//非词性标识符列表
        List<string> symList = new List<string>();//词性标识符列表
        List<KeyValuePair<string, string>> pairList = new List<KeyValuePair<string, string>>();

        int index = 0;
        string tempWord = "";
        string tempSymbol = "";
        while (index < line.Length)
        {


            while( index < line.Length && (line[index] > 'z' || line[index] < 'a'))
            {
                tempWord += line[index];
                index++;
            }
            wordList.Add(tempWord);
            tempWord = "";

            while (index < line.Length && line[index] <= 'z' && line[index] >= 'a')
            {
                tempSymbol += line[index];
                index++;
            }
            symList.Add(tempSymbol);
            tempSymbol = "";

        }
        if (wordList.Count != symList.Count)
        {
            throw new System.Exception("词性标注有误");
        }
        for (int i = 0; i < wordList.Count; i++)
        {
            pairList.Add(new KeyValuePair<string, string>(symList[i], wordList[i]));
        }
        return pairList;
    }


    //截断截断词后的内容
    private List<KeyValuePair<string,string>> cutLine(List<KeyValuePair<string,string>> pairList)
    {
        List<KeyValuePair<string, string>> tempPairList = new List<KeyValuePair<string, string>>();
        //处理截断词说后面的内容
        int speakNum = -1;
        for (int i = 0; i < pairList.Count; i++)
        {
            if (DataMap.cutFlag.Contains(pairList[i].Value))
            {
                speakNum = i;
                break;
            }
        }
        if (speakNum == -1)
        {
            tempPairList = pairList;
            return tempPairList;
        }

        //截断后面的句子'
       
        string tmpSpeakLine = "";
        if (speakNum != -1)
        {
            for (int j = speakNum + 1; j < pairList.Count; j++)
            {

                //if (!pairList[j].Key.Equals("x"))
                //{
                tmpSpeakLine += pairList[j].Value;
                //}

            }
           

            for (int j = 0; j <= speakNum; j++)
            {
                tempPairList.Add(pairList[j]);
            }
        }
      
        return tempPairList;

        
    }

    //提取纯文字作为字幕
    string extractPureText(List<KeyValuePair<string,string>>pairList)
    {
        string re = "";
        for(int i = 0; i < pairList.Count; i++)
        {
            re += pairList[i].Value;
        }
        return re;
    }

    //探查没有主语句子的主语,保守策略，
    void addSubByPinyin()
    {
        WordInspector wordInspector = new WordInspector();
        int firstActIndex = -1;
        for (int i = 0; i < pairList.Count; i++)
        {
            if (DataMap.convMatMapList.getAllActionRawName().Contains(pairList[i].Value))
            {
                firstActIndex = i;
                break;
            }
        }
        if (firstActIndex > 0)
        {
            for (int i = 0; i < firstActIndex; i++)
            {
                string samePinyin = wordInspector.getSamePinyinWord(pairList[i].Value);
                string originKey = pairList[i].Key;
                if (samePinyin != "")
                {
                    pairList.RemoveAt(i);
                    pairList.Insert(i, new KeyValuePair<string, string>(originKey, samePinyin));
                }
            }
        }
        else
        {
            inspectPinyin(pairList, 0,pairList.Count-1);
        }
    }

    //全部探查替换
    void inspectPinyin(List<KeyValuePair<string, string>> pairList, int startIndex,int endIndex)
    {
        WordInspector wordInspector = new WordInspector();
      
        for(int i = startIndex; i <= endIndex; i++)
        {
            if (DataMap.allRawNameList.Contains(pairList[i].Value)) continue;
            if(pairList[i].Key.Contains("n")&&pairList[i].Value.Length>1)
            {
                string samePinyin = wordInspector.getSamePinyinWord(pairList[i].Value);
                string originKey = pairList[i].Key;
                if (samePinyin != "")
                {
                    pairList.RemoveAt(i);
                    pairList.Insert(i, new KeyValuePair<string, string>(originKey, samePinyin));
                }
            }
            
        }
    }

    List<KeyValuePair<string, string>> handleHas(List<KeyValuePair<string, string>> pairList)
    {
        List<string> hideList = new List<string> { "r", "n", "nr" };
        List<KeyValuePair<string, string>> delPairList = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> newPairList = new List<KeyValuePair<string, string>>();

        for (int i = 0; i < pairList.Count; i++)
        {
            if (pairList[i].Value.Equals("的") && i>=1)
            {
                if (hideList.Contains(pairList[i - 1].Key))
                {
                    delPairList.Add(pairList[i - 1]);
                }
            }
        }
        for(int i = 0; i < pairList.Count; i++)
        {
            if(!delPairList.Contains(pairList[i]))
            {
                newPairList.Add(pairList[i]);
            }
        }
        return newPairList;
    }
 
   //根据有无object修改diyaction的planner
    Planner correctN_actionPlanner(BasicEvent basicEvent,Planner planner)
    {
        string actionName = basicEvent.actionName;
        if (DataMap.diyActList.getElemByRawName(actionName).level == "strict") return null;
        if (basicEvent.eventType.Equals(BasicEvent.BasicEventType.n_action))
        {
            if (Type.GetType("SingleMoveActionPlanner").Equals(planner.GetType()) && basicEvent.objectName!="")
            {
                CoupleMoveActionPlanner coupleMoveActionPlanner = new CoupleMoveActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                return coupleMoveActionPlanner;
            }
            if (Type.GetType("SingleStaticActionPlanner").Equals(planner.GetType()) && basicEvent.objectName != "")
            {
                CoupleStaticActionPlanner coupleMoveActionPlanner = new CoupleStaticActionPlanner(basicEvent, curRecorder, basicEvent.actionName);
                return coupleMoveActionPlanner;
            }
        }
        return null;
    }

    //为自定义素材加上默认动画
    void addUserMatAnim(List<KeyFrame> keyFrames)
    {
        foreach(KeyFrame keyFrame in keyFrames)
        {
            if(keyFrame.action==2)
            {
                string matName = keyFrame.name;
                
                if (DataMap.privateActorList.ContainMat(matName) && DataMap.matGifCollection.FindMatGifListByName(matName)==null)
                {
                    keyFrame.content = "testAnim";
                }
            }
        }
    }

    //删除不必要的默认动作帧
    List<KeyFrame> delDefaultKeyframe(List<KeyFrame> inputKeyframes)
    {
        Dictionary<string, List<KeyFrame>> tempActorDic = new Dictionary<string, List<KeyFrame>>();
        List<KeyFrame> re = inputKeyframes;
        for(int i = 0; i < inputKeyframes.Count; i++)
        {
            string goname = inputKeyframes[i].goname;
            if (tempActorDic.ContainsKey(goname))
            {
                tempActorDic[goname].Add(inputKeyframes[i]);
            }
            else
            {
                tempActorDic.Add(goname, new List<KeyFrame> { inputKeyframes[i] });
            }
        }

        foreach(string key in tempActorDic.Keys)
        {

            
            List<KeyFrame> tmpkeyframes = tempActorDic[key];
            if (tmpkeyframes.Count > 1)
            {

                //找所有开始时间相同的
                Dictionary<float, List<KeyFrame>> tempTimeDic = new Dictionary<float, List<KeyFrame>>();
                foreach(KeyFrame keyFrame in tmpkeyframes)
                {
                    float timestamp = keyFrame.timestamp;
                    if (tempTimeDic.ContainsKey(timestamp))
                    {
                        tempTimeDic[timestamp].Add(keyFrame);
                    }
                    else
                    {
                        tempTimeDic.Add(timestamp, new List<KeyFrame> { keyFrame });
                    }
                }

                foreach(float starttime in tempTimeDic.Keys)
                {
                    if (tempTimeDic[starttime].Count > 1)
                    {
                        List<KeyFrame> frames = tempTimeDic[starttime];
                        foreach(KeyFrame frame in frames)
                        {
                            if(frame.action==2 &&
                                DataUtil.compareList<float>(frame.startpos,frame.endpos)&&
                                DataUtil.compareList<float>(frame.startrotation,frame.endrotation)&&
                                DataUtil.compareList<float>(frame.startscale, frame.endscale))
                            {
                                re.Remove(frame);
                                break;
                            }
                        }
                    }
                }




            }
            
        }
        return re;
    }


    public IEnumerator getSementic(Func<float,string, bool> func,Action callback)
    {
        string text = "";
        foreach (KeyValuePair<string, string> keyValuePair in pairList)
        {
            text += keyValuePair.Value + " ";
        }
        float startTime = Time.time;
        StartCoroutine(NlpServer.GetSementics(text));


        yield return new WaitUntil(() => func(startTime,text));
        callback();

    }



    bool GetSementicFinished(float startTime, string text)
    {      
        if (Time.time - startTime > 2)
        {
            Debug.Log("等待超时");
            return true;
        }
            
        if ( NlpServer.sementicList.sementics!=null)
        {
            if (NlpServer.sementicList.sementics.Length > 0)
            {
                this.sementicList = DataUtil.Clone<SementicList>(NlpServer.sementicList);
                NlpServer.sementicList = new SementicList();
            }
            return true;
        }
        else
        {
            return false;
        }
    }


    void useExtractSuit()
    {
        if (this.sementicList.sementics != null)
        {
            Debug.Log("采用语义标记方法");
            sementicExtractEvent();
            extractFinished = true;
        }
        else
        {
            Debug.Log("采用原方法");
            legacyExtractEvent();
            extractFinished = true;
        }
    }

    void legacyExtractEvent()
    {
       
        extractBkgEvent();
        
        extractInitEvent();

        extractActionEvent();
        
        setEventTime();

    }

    void sementicExtractEvent()
    {
        extractBkgEvent();
        extractInitEvent();
        extractActionEvent_sementic();
        //extractActionEvent();
        setEventTime();
    }


}
 
 
 
