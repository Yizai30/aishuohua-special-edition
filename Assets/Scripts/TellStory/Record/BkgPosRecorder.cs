using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BkgPosRecorder
{

    //记录每个点位是什么物体
    public Dictionary<string, List<string>> bkgPosUseDic { set; get; }

    //记录物体的大小方向等
    public Dictionary<string, PathTransform> transformDic { set; get; }


    public Dictionary<string,List<string>> childDic { set; get; }
    
    public Dictionary<string,List<float>>  childParentGapDic { set; get; }

    public BkgPosRecorder()
    {
        initBkgPosUseDic();
        transformDic = new Dictionary<string, PathTransform>();
        childDic = new Dictionary<string, List<string>>();
        childParentGapDic = new Dictionary<string, List<float>>();
        
    }

    public bool isChildGoname(string goname)
    {
        foreach(string key in childDic.Keys)
        {
            if (childDic[key].Contains(goname)) return true;
        }
        return false;
    }

    public bool isChildMatname(string matName)
    {
        
        foreach (string key in childDic.Keys)
        {
            string goname = childDic[key][0];
            string tMatname = DataUtil.getMatnameByGoname(goname);
            if (tMatname == matName) return true;
        }
        return false;
    }
    public int getIndexByPos(string bkgName, List<float> pos)
    {
        int re = -1;
        bool ischild = false;
        
        string parent = "";
        
        /*
        foreach(string key in childDic.Keys)
        {
            if (goname.Equals("")) break;
            if (childDic[key].Contains(goname)&&goname!="")
            {
                ischild = true;
                parent = key;
                break;
            }
        }
        */

        List<List<float>> posList = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList;
        if (ischild)
        {
            List<string> useList = bkgPosUseDic[bkgName];
            for(int i = 0; i < useList.Count; i++)
            {
                if (useList[i].Equals(parent))
                {
                    re = i;
                    break;
                }
            }
        }
        else
        {         
            for (int i = 0; i < posList.Count; i++)
            {

                if (DataUtil.compareList(posList[i], pos))
                {
                    re = i;
                    break;
                }
            }
        }
       
        return re;


    }

    private void initBkgPosUseDic()
    {
        bkgPosUseDic = new Dictionary<string, List<string>>();
        BkgSettingList bkgSettingList = DataMap.bkgSettingList;
        foreach (BkgSettingElement el in bkgSettingList.bkgSettingList)
        {
            if (!bkgPosUseDic.ContainsKey(el.backgroundName))
            {
                int num = el.initPositionList.Count;
                List<string> tmpPosList = new List<string>();
                for (int i = 0; i < num; i++)
                {
                    tmpPosList.Add("");
                }

                bkgPosUseDic.Add(el.backgroundName, tmpPosList);
            }
        }
    }

    public PathTransform getTransformByGoname(string goname)
    {
        if (this.transformDic.ContainsKey(goname))
        {
            return transformDic[goname];
        }
        throw new System.Exception("transformDic中没有此物体" + goname);
    }


    public void updateTransform(string goname, PathTransform transform)
    {
        if (this.transformDic.ContainsKey(goname))
        {
            this.transformDic[goname] = transform;
        }
    }
    public void removeTransform(string goname)
    {
        if (this.transformDic.ContainsKey(goname))
        {
            this.transformDic.Remove(goname);
        }

    }
    public void addTransform(string goname, PathTransform transform)
    {
        if (this.transformDic.ContainsKey(goname))
        {
            this.transformDic[goname] = transform;
        }
        else
        {
            this.transformDic.Add(goname, transform);
        }
    }

    public void changeTransform(string goname, PathTransform transform)
    {
        if (!this.transformDic.ContainsKey(goname))
        {
            throw new System.Exception("transformDic中没有此物体" + goname);
        }
        else
        {
            this.transformDic[goname] = transform;
        }
    }
    /*
    public void changePos(string bkgName, string goname, int newIndex)
    {
        List<string> posList = bkgPosUseDic[bkgName];
        if (newIndex >= posList.Count)
        {
            throw new System.Exception("新的位置超出背景所有可选位置的预设列表长度");
        }
        for (int i = 0; i < posList.Count; i++)
        {
            if (posList[i].Equals(goname))
            {
                posList[i] = "";
                posList[newIndex] = goname;
                return;
            }
        }
        throw new System.Exception("背景中没有此角色，交换失败");
    }
    */

    public void changePos(string bkgName, string goname, List<float> newPos)
    {
        
        int newIndex = getIndexByPos( bkgName, newPos);
        List<string> posList = bkgPosUseDic[bkgName];
        if (newIndex >= posList.Count)
        {
            throw new System.Exception("新的位置超出背景所有可选位置的预设列表长度");
        }
        for (int i = 0; i < posList.Count; i++)
        {
            if (posList[i].Equals(goname))
            {
                posList[i] = "";
                posList[newIndex] = goname;
                return;
            }
        }
        //没有goname
        posList[newIndex] = goname;
        //throw new System.Exception("背景中没有此物体"+goname+"，交换失败");
    }

    public void setChild(string parentGo,string childGo,List<float> childPos,List<float>parentPos)
    {
        if (childDic.ContainsKey(parentGo))
        {
            throw new Exception(parentGo+ "只能有一个子物体");

        }
        else
        {
            childDic.Add(parentGo, new List<string> { childGo});
           
        }
    }

    public void unsetChild(string parentGo, string childGo)
    {
        if (childDic.ContainsKey(parentGo))
        {
            if (childDic[parentGo].Contains(childGo))
            {
                childDic.Remove(parentGo);
               
            }
            else
            {
                throw new Exception(parentGo + "没有这个子物体" + childGo);
            }

        }
       
    }

    public List<float> getForwardNeighborPoint(string bkgName, int masterIndex, int currIndex, int interval)
    {
        List<int> pointList = new List<int>();
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<PointMark> pointMarks = element.pointMarks;
        foreach (PointMark pointMark in pointMarks)
        {
            if (pointMark.pointList.Contains(masterIndex))
            {
                pointList = pointMark.pointList;
            }
        }

        if (!pointList.Contains(currIndex))
        {
            currIndex = pointList[pointList.Count - 1];
        }

        List<float> re = new List<float>();
        List<string> posList = bkgPosUseDic[bkgName];
        //已经相邻，
        if (Mathf.Abs(currIndex - masterIndex) == interval && posList[currIndex].Equals(""))
        {
            re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[currIndex];
            return re;
        }
        if (currIndex == masterIndex)
        {
            if (currIndex == 0) currIndex = pointList[pointList.Count - 1];
            else currIndex = 0;
        }

        
        int rightIndex = masterIndex + interval;
        int leftIndex = masterIndex - interval;
        //右边
        if (currIndex - masterIndex > 0)
        {
            if (rightIndex < posList.Count && posList[rightIndex].Equals("") && pointList.Contains(rightIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[rightIndex];
                return re;
            }
           

        }
        //左边
        else if (currIndex - masterIndex < 0)
        {
            if (leftIndex >= 0 && posList[leftIndex].Equals("") && pointList.Contains(leftIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[leftIndex];
                return re;
            }
           
        }


        return re;

    }


    //currIndex是参考点，邻点优先取和参考点同侧的点
    public List<float> getNeighborPoint(string bkgName, int masterIndex, int currIndex,string curgoname , int interval)
    {
        List<int> pointList = new List<int>();
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<PointMark> pointMarks = element.pointMarks;
        foreach(PointMark pointMark in pointMarks)
        {
            if (pointMark.pointList.Contains(masterIndex))
            {
                pointList = pointMark.pointList;
            }
        }

        if (!pointList.Contains(currIndex))
        {
            currIndex = pointList[pointList.Count - 1];
        }

        List<float> re = new List<float>();
        List<string> posList = bkgPosUseDic[bkgName];
        //已经相邻，
        if (Mathf.Abs(currIndex - masterIndex) == interval && posList[currIndex].Equals("") ||
            Mathf.Abs(currIndex - masterIndex) == interval && curgoname!="" && posList[currIndex].Equals(curgoname))
        {
            re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[currIndex];
            return re;
        }
        if (currIndex == masterIndex)
        {
            if (currIndex == 0) currIndex = pointList[pointList.Count - 1];
            else currIndex = 0;
        }
        int rightIndex = masterIndex + interval;
        int leftIndex = masterIndex - interval;
        //优先右边
        if( currIndex - masterIndex > 0){
            if (rightIndex < posList.Count && posList[rightIndex].Equals("") && pointList.Contains(rightIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[rightIndex];
                return re;
            }
            if (leftIndex >= 0 && posList[leftIndex].Equals("") && pointList.Contains(leftIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[leftIndex];
                return re;
            }

        }
        //优先左边
        else if( currIndex - masterIndex < 0)
        {           
            if (leftIndex >= 0 && posList[leftIndex].Equals("") && pointList.Contains(leftIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[leftIndex];
                return re;
            }
            if (rightIndex < posList.Count && posList[rightIndex].Equals("") && pointList.Contains(rightIndex))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[rightIndex];
                return re;
            }
        }

        //临边没有空余点
        //if (re.Count == 0)
        //{
        //    re = getSameTypeEmptyPoint(bkgName, masterIndex);
        //}
        
        
        return re;

    }


    public List<float> getLeftExit(string bkgName, List<int> posList)
    {
        List<float> re = new List<float>();
        foreach(int posIndex in posList)
        {
            List<float> pos = getPosByIndex(bkgName, posIndex);
            if (pos[0] < 0)
            {
                re = pos;
                return re;
            }
        }
        return re;
    }

    public List<float> getRightExit(string bkgName, List<int> posList)
    {
        List<float> re = new List<float>();
        foreach (int posIndex in posList)
        {
            List<float> pos = getPosByIndex(bkgName, posIndex);
            if (pos[0] > 0)
            {
                re = pos;
                return re;
            }
        }
        return re;
    }

    public List<float> getExitPos(string bkgName, List<float> curPos)
    {
        List<float> re = new List<float>();
        List<int> pointList = getSameTypePointList(bkgName, "exitPoint");
        List<float> leftExitPoint = getLeftExit(bkgName, pointList);
        List<float> rightPoint = getRightExit(bkgName, pointList);

        float disLeft = DataUtil.getDis(curPos, leftExitPoint);
        float disRight = DataUtil.getDis(curPos, rightPoint);
        if (disLeft < disRight)
        {
            re= leftExitPoint;
        }
        else
        {
            re= rightPoint;
        }
        if(re==null || re.Count == 0)
        {
            
            re=getEmptyRightFirst(bkgName, pointList, 1);
        }

        return re;
    }


    public bool isIndexEmpty(string bkgName,int index)
    {
        List<string> posList = bkgPosUseDic[bkgName];
        if (posList[index].Equals("")) return true;
        else return false;
    }

    //获取和当前点相同类型点位列表
    public List<int> getSameTypePointList(string bkgName,string pointFlag)
    {
        List<int> re = new List<int>();
        List<string> posList = bkgPosUseDic[bkgName];
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<PointMark> pointMarks = element.pointMarks;
        foreach(PointMark pointMark in pointMarks)
        {
            if (pointMark.pointType.Equals(pointFlag))
            {
                re = pointMark.pointList;
            }
        }
        return re;
    }

    public List<string> getSameTypeUseList(string bkgName,string pointFlag)
    {
        List<string> re = new List<string>();
        List<string> allPosList = bkgPosUseDic[bkgName];
        List<int> pointNum = getSameTypePointList(bkgName, pointFlag);
        for(int i = 0; i < pointNum.Count; i++)
        {
            re.Add(allPosList[pointNum[i]]);
        }
        return re;
    }

    public string getPointFlag(string bkgName,int index)
    {
        
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<PointMark> pointMarks = element.pointMarks;
        foreach (PointMark pointMark in pointMarks)
        {
            if (pointMark.pointList.Contains(index))
            {
                return pointMark.pointType;
            }
        }
        throw new Exception("没有找该点的flag");
    }

    //根据x坐标找一个flag里的点
    public int getPointIndexByX(string pointFlag, string bkgName,float pos_x,float range)
    {
        int re = -1;
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<List<float>> posList = element.initPositionList;

        List<int> flagPointList = getSameTypePointList(bkgName, pointFlag);

        for(int i = 0; i < flagPointList.Count; i++)
        {
            float tx = posList[flagPointList[i]][0];

            if (Mathf.Abs(tx-pos_x)<=range)
            {
                re = flagPointList[i];
            }
        }

       
        return re;
    }

    //取当前点同pointType的另一个空余点位
    public List<float> getSameTypeEmptyPoint(string bkgName,int pointIndex)
    {
       // List<List<float>> initPointList= DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList;
        List<float> re = new List<float>();
        List<string> posList = bkgPosUseDic[bkgName];
        BkgSettingElement element = DataMap.bkgSettingList.getElementByBkgName(bkgName);
        List<PointMark> pointMarks = element.pointMarks;
        foreach(PointMark pointMark in pointMarks)
        {
            if (pointMark.pointList.Contains(pointIndex))
            {
                for(int i = 0; i < pointMark.pointList.Count; i++)
                {
                    if (posList[pointMark.pointList[i]].Equals(""))
                    {
                        re = element.initPositionList[pointMark.pointList[i]];
                        break;
                    }
                }
            }
        }
        return re;
    }
    //在指定点
    public void addPos(string bkgName, List<float> pos,string goname)
    {
        int index = getIndexByPos(bkgName, pos);
        if (bkgPosUseDic[bkgName][index] == "")
        {
            bkgPosUseDic[bkgName][index] = goname;
        }
        else
        {
            throw new System.Exception("此位置已经被占据");
        }
    }

  

    //从左到右找空余为
    public List<float> getEmptyRightFirst(string bkgName, List<int> pointList, int interval)
    {
        List<string> allPosList = bkgPosUseDic[bkgName];
        Dictionary<int, string> posList = new Dictionary<int, string>();
        foreach (int useIndex in pointList)
        {
            posList.Add(useIndex, allPosList[useIndex]);
        }
        int left = 0;
        for (int i = 0; i < pointList.Count; i++)
        {
            if (posList[pointList[i]].Equals(""))
            {
                left++;
            }
        }
        if (left > 0)
        {
            
            for (int i = pointList[0]; i <= pointList[pointList.Count - 1]; i=i+interval)
            {
                if (posList.ContainsKey(i) && posList[i].Equals(""))
                {
                    return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[i];
                }
            }

            return getSameTypeEmptyPoint(bkgName, pointList[0]);

            //throw new System.Exception("生成新位置时发生未知错误");
        }
        else
        {
            return new List<float>();
        }
    }
    
    //返回列表中离当前点最近的坐标
    public List<float> getEmptyNearestPos(string bkgName,List<int> pointList,List<float> curPos)
    {
        if (pointList.Count == 0 || curPos==null || curPos.Count==0) return new List<float>();

        List<string> allPosList = bkgPosUseDic[bkgName];
        Dictionary<int, string> posList = new Dictionary<int, string>();
        foreach (int useIndex in pointList)
        {
            posList.Add(useIndex, allPosList[useIndex]);
        }

        int left = 0;
        for (int i = 0; i < pointList.Count; i++)
        {
            if (posList[pointList[i]].Equals(""))
            {
                left++;
            }
        }
        if (left > 0)
        {
            int targetIndex = -1;
            float minDis = float.MaxValue;
            foreach(int key in posList.Keys)
            {
                if (posList[key] != "") continue;
                List<float> tempPos = getPosByIndex(bkgName, key);
                float tempDis = DataUtil.getDis(tempPos, curPos);
                if (tempDis < minDis)
                {
                    minDis = tempDis;
                    targetIndex = key;
                }
            }
            return getPosByIndex(bkgName, targetIndex);
        }
        else
        {
            string pointFlag = getPointFlag(bkgName, pointList[0]);
            throw new System.Exception("背景角色人数达到上限" + pointFlag);
        }
    }

    public  List<float> getEmptyPosMidFirst(string bkgName,List<int>pointList,int interval,int midIndex)
    {

        if (pointList.Count == 0) return new List<float>();
        
        List<string> allPosList = bkgPosUseDic[bkgName];
        //key是范围点的索引
        Dictionary<int, string> posList = new Dictionary<int, string>();
        foreach(int useIndex in pointList)
        {
            posList.Add(useIndex, allPosList[useIndex]);
        }

        int left = 0;
        for (int i = 0; i < pointList.Count; i++)
        {
            if (posList[pointList[i]].Equals(""))
            {
                left++;
            }
        }
        if (left > 0)
        {           
            //int midIndex = posList.Count / 2;
            if (posList[pointList[midIndex]].Equals(""))
            {
                //posList[pointList[midIndex]] = goname;
                return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[pointList[midIndex]];
            }
            int switcher = 1;
            int scaler = 1;
            
            while(midIndex+scaler*interval<=posList.Count || midIndex - scaler * interval >= 0)
            {
                int rightI = midIndex + scaler * interval;
                int leftI = midIndex - scaler * interval;
               
                if(leftI>=0 && switcher % 2 == 1)
                {
                    if (posList[pointList[leftI]].Equals(""))
                    {
                        //posList[pointList[leftI]] = goname;
                        return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[pointList[leftI]];
                    }
                }
                if (rightI < posList.Count && switcher % 2 == 0)
                {
                    if (posList[pointList[rightI]].Equals(""))
                    {
                        //posList[pointList[rightI]] = goname;
                        return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[pointList[rightI]];
                    }
                }
                if (switcher % 2 == 0)
                {
                    scaler++;
                }
                //scaler++;
                switcher++;
            }
          
            for (int i = 0; i < pointList.Count; i = i + 1 + interval)
            {
                if (posList[pointList[i]].Equals(""))
                {
                    //posList[pointList[i]] = goname;
                    //allPosList[pointList[i]] = goname;
                    return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[pointList[i]];
                }
            }
                
                
            for(int j = 0; j < pointList.Count; j++)
            {
                if (posList[pointList[j]].Equals("")) { 
                 
                    //posList[pointList[j]] = goname;
                    //allPosList[pointList[j]] = goname;
                    return DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[pointList[j]];
                }
            }               
            
            throw new System.Exception("生成新位置时发生未知错误");
           
        }
        else
        {
            string pointFlag = getPointFlag(bkgName, pointList[0]);
            throw new System.Exception("背景角色人数达到上限"+pointFlag);
        }
        
       
    }

    public List<float>removePos(string goname)
    {
        List<float> re = new List<float>();
        foreach(string bkgName in bkgPosUseDic.Keys)
        {
            List<string> posList = bkgPosUseDic[bkgName];
            for (int i = 0; i < posList.Count; i++)
            {
                if (posList[i].Equals(goname))
                {
                    posList[i] = "";
                    re= DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[i];
                }
            }
        }
        return re;
    }


    public  List<float> getPosByGoname(string bkgName, string goName)
    {
        List<float> re = new List<float>();
        List<string> posList = bkgPosUseDic[bkgName];
        for (int i = 0; i < posList.Count; i++)
        {
            if (posList[i].Equals(goName))
            {
                re = DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[i];
            }
        }
        return re;
    }

    public bool isEmpty(string bkgName,int startIndex,int endIndex)
    {
        bool re = true;
        List<string> occList = this.bkgPosUseDic[bkgName];
        if(startIndex<0 || endIndex >= occList.Count)
        {
            throw new Exception("索引错误");
        }
        for(int i = startIndex; i < endIndex; i++)
        {
            if (occList[i] != "")
            {
                re = false;
            }
        }
        return re;
    }

    public List<float> getPosByIndex(string bkgName,int index)
    {
        List<float> pos = new List<float>();
        
        pos=DataUtil.Clone<List<float>>( DataMap.bkgSettingList.getElementByBkgName(bkgName).initPositionList[index]);

        return pos;
    }

    public bool belongType(string bkgName,string pointFlag,int pointIndex)
    {
        List<int> typeList = getSameTypePointList(bkgName, pointFlag);
        if (typeList.Contains(pointIndex)) return true;
        else return false;

    }

    //返回所有坐标在index左侧的非物品goname
    public List<string> getIndexLeftGoname(string bkgName,int index)
    {
        List<string> re = new List<string>();
        List<string> useDic = bkgPosUseDic[bkgName];
        List<float> indexPos = getPosByIndex(bkgName, index);
        for(int i = 0; i < useDic.Count; i++)
        {
            if (useDic[i] != "")
            {
                string goname = useDic[i];
                string matname = DataUtil.getMatnameByGoname(goname);
                MatSettingElement matSettingElement= DataMap.matSettingList.getMatSettingMap(matname);
                if (!matSettingElement.classList.Contains("character")) continue;
                

                List<float> tmpPos = getPosByIndex(bkgName,i);
                if (tmpPos[0] < indexPos[0])
                {
                    re.Add(useDic[i]);
                }
            }
        }

        return re;


    }

    //返回所有坐标在index右侧的非物品goname
    public List<string> getIndexRightGoname(string bkgName, int index)
    {
        List<string> re = new List<string>();
        List<string> useDic = bkgPosUseDic[bkgName];
        List<float> indexPos = getPosByIndex(bkgName, index);
        for (int i = 0; i < useDic.Count; i++)
        {
            if (useDic[i] != "")
            {
                string goname = useDic[i];
                string matname = DataUtil.getMatnameByGoname(goname);
                MatSettingElement matSettingElement = DataMap.matSettingList.getMatSettingMap(matname);
                if (!matSettingElement.classList.Contains("character")) continue;

                List<float> tmpPos = getPosByIndex(bkgName, i);
                if (tmpPos[0] > indexPos[0])
                {
                    re.Add(useDic[i]);
                }
            }
        }

        return re;


    }

    //找出竖直方向，距离当前位置最近的点，作为入口
    public int getNeareastBridgeGatePoint(string bkgName,List<float> curPos)
    {
        List<int> gatePoints = getSameTypePointList(bkgName, "bridgeGate");
        float minVertic = 100;
        int tarGateIndex = -1;
        if (gatePoints.Count == 0) return tarGateIndex;
        foreach (int gateIndex in gatePoints)
        {
            List<float> gatePos = getPosByIndex(bkgName, gateIndex);
            float verticDis = Mathf.Abs(gatePos[1] - curPos[1]);
            if (verticDis < minVertic)
            {
                minVertic = verticDis;
                tarGateIndex = gateIndex;
            }
        }
        return tarGateIndex;
    }


    //找出另一个桥出口
    public int getOtherBridgeGatePoint(string bkgName, List<float> curGate)
    {
        int re = -1;
        List<int> gatePoints = getSameTypePointList(bkgName, "bridgeGate");
        
        foreach (int gateIndex in gatePoints)
        {
            List<float> gatePos = getPosByIndex(bkgName, gateIndex);
            if (!DataUtil.compareList<float>(curGate, gatePos))
            {
                re = gateIndex;
            }
           
        }
        return re;
    }

    public int getBridgeSidePos(string bkgName, List<float>gatePos)
    {
        int re = -1;
        int otherGate = getOtherBridgeGatePoint(bkgName, gatePos);
        List<float> otherGatePos = getPosByIndex(bkgName, otherGate);
        List<float> tarPos = new List<float>();
        if (gatePos[1] < otherGatePos[1])
        {
            //default
            List<int> tarList= getSameTypePointList(bkgName, "defaultPoint");
            tarPos= getEmptyPosMidFirst(bkgName, tarList, 2, tarList.Count / 2);
        }
        else
        {
            //bridgeSide
            List<int> tarList = getSameTypePointList(bkgName, "bridgeSide");
            tarPos = getEmptyPosMidFirst(bkgName, tarList, 2, tarList.Count / 2);

        }
        re = getIndexByPos(bkgName ,tarPos);
        return re;
    }




}
