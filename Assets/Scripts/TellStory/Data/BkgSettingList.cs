using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BkgSettingList 
{
   public List<BkgSettingElement> bkgSettingList { set; get; }

    public BkgSettingElement getElementByBkgName(string bkgName)
    {
        foreach(BkgSettingElement element in this.bkgSettingList)
        {
            if (element.backgroundName.Equals(bkgName))
            {
                return element;
            }
        }
        return null;
    }

    public void delElementByBkgName(string bkgName)
    {
        for(int i = bkgSettingList.Count - 1; i >= 0; i--)
        {
            if (bkgSettingList[i].backgroundName.Equals(bkgName))
            {
                bkgSettingList.RemoveAt(i);
            }
        }
      
    }

    public List<BkgSettingElement> getElementsContainBkgName(string bkgName)
    {
        List<BkgSettingElement> re = new List<BkgSettingElement>();
        foreach (BkgSettingElement element in this.bkgSettingList)
        {
            if (element.backgroundName.Contains(bkgName))
            {
                re.Add( element);
            }
        }
        return re;
    }

    public string getBkgNameByPointType(string pointType,string currBkgName)
    {
        string re = "";
        List<string> normalType = new List<string> { "defaultPoint", "exitPoint","escapePoint" };
        if(!normalType.Contains(pointType))
        {
            if (currBkgName != "")
            {
                BkgSettingElement defaultElement = getElementByBkgName(currBkgName);
                List<PointMark> defaultpointMarks = defaultElement.pointMarks;
                foreach (PointMark defaultpointMark in defaultpointMarks)
                {
                    if (defaultpointMark.pointType.Equals(pointType))
                    {
                        re = defaultElement.backgroundName;
                        return re;
                    }
                }
            }
           
            foreach (BkgSettingElement bkgSettingElement in bkgSettingList)
            {
                List<PointMark> pointMarks = bkgSettingElement.pointMarks;
                foreach(PointMark pointMark in pointMarks)
                {
                    if (pointMark.pointType.Equals(pointType))
                    {
                        re = bkgSettingElement.backgroundName;
                        return re;
                    }
                }
            }
        }
        return re;
    }
    
    public BkgSettingList(List<BkgSettingElement> bkgSettingList)
    {
        this.bkgSettingList = bkgSettingList;
    }

    public BkgSettingList()
    {
        this.bkgSettingList = new List<BkgSettingElement>();
    }
}


public class BkgSettingElement
{
    public string backgroundName { set; get; }
    public List<List<float>> initPositionList { set; get; }
    public List<PointMark> pointMarks { set; get; }

    [JsonConstructor]
    public BkgSettingElement(string backgroundName, List<List<float>> initPositionList, List<PointMark> pointMarks)
    {
        this.backgroundName = backgroundName;
        this.initPositionList = initPositionList;
        this.pointMarks = pointMarks;
    }

    public BkgSettingElement(string backgroundName)
    {
        this.backgroundName = backgroundName;
        this.initPositionList = new List<List<float>>();
        this.pointMarks = new List<PointMark>();
    }

    public List<int> getPointListByPointType(string pointType)
    {
        List<int> re = new List<int>();
        foreach (PointMark pointMark in pointMarks)
        {
            if (pointMark.pointType.Equals(pointType))
            {
                re = pointMark.pointList;
            }
        }
        return re;
    }

    public bool containsPointType(string pointType)
    {
        foreach (PointMark pointMark in pointMarks)
        {
            if (pointMark.pointType.Equals(pointType))
            {
                return true;
            }
        }
        return false;
    }



    public void deletePointTypeAndPoint(string pointType)
    {
        if (!containsPointType(pointType)) return;
        PointMark dePointMark = null;
        foreach (PointMark pointMark in pointMarks)
        {
            if (pointMark.pointType == pointType)
            {
                dePointMark = pointMark;
                break;
            }
        }

        List<int> dePointIndexList = getPointListByPointType(pointType);
        pointMarks.Remove(dePointMark);


        //移除initPos
        List<List<float>> newIndexList = new List<List<float>>();
        for (int index = 0; index < initPositionList.Count; index++)
        {
            if (!dePointIndexList.Contains(index))
            {
                newIndexList.Add(initPositionList[index]);
            }
        }
        this.initPositionList = newIndexList;

        //更新旧的index

        int firstDeIndex = dePointIndexList[0];
        foreach (PointMark mark in pointMarks)
        {
            for (int i = 0; i < mark.pointList.Count; i++)
            {
                if (mark.pointList[i] > firstDeIndex)
                {
                    mark.pointList[i] -= dePointIndexList.Count;
                }
            }
        }

    }

    public void addPointsToType(string pointType, List<List<float>> posList)
    {
        if (!containsPointType(pointType))
        {
            addPointTypeAndPoint(pointType, posList);
            return;
        }
        List<int> curIndexList = getPointListByPointType(pointType);
        List<List<float>> newPosList = new List<List<float>>();
        for (int i = 0; i < curIndexList.Count; i++)
        {
            newPosList.Add(initPositionList[curIndexList[i]]);
        }
        newPosList.AddRange(posList);

        deletePointTypeAndPoint(pointType);

        addPointTypeAndPoint(pointType, newPosList);

    }

    //添加新的pointType和对应的坐标
    public void addPointTypeAndPoint(string pointType, List<List<float>> posList)
    {
        if (containsPointType(pointType))
        {
            deletePointTypeAndPoint(pointType);
        }
        //新的indexList
        int newStartIndex = initPositionList.Count;
        List<int> newIndexList = new List<int>();
        for (int i = newStartIndex; i < newStartIndex + posList.Count; i++)
        {
            newIndexList.Add(i);
        }

        //将新的坐标加入末尾
        //检测有无重复点位，否则报错
        foreach (List<float> pos in posList)
        {
            if (initPositionList.Contains(pos))
            {
                throw new System.Exception("添加了重复的点位坐标" + pos[0] + " " + pos[1] + " " + pos[2]);
            }
        }
        initPositionList.AddRange(posList);

        //生成新的pointMark

        PointMark pointMark = new PointMark(pointType, newIndexList);

        this.pointMarks.Add(pointMark);

    }

    //复制另一个背景的全部点位信息
    public void copyElementInfo(BkgSettingElement sourceElement)
    {
        this.initPositionList = DataUtil.Clone<List<List<float>>>(sourceElement.initPositionList);
        this.pointMarks = DataUtil.Clone<List<PointMark>>(sourceElement.pointMarks);
    }

    //新增一个已有点位的pointtype
    public void referPointType(string sourcePointType, string targetPointType)
    {
        if (!containsPointType(sourcePointType)) return;
        if (containsPointType(targetPointType))
        {
            if (isReferenceType(targetPointType))
            {
                deleteType(targetPointType);
            }
            else
            {
                deletePointTypeAndPoint(targetPointType);
            }

        }
        List<int> sourcePosIndex = getPointListByPointType(sourcePointType);
        PointMark rePointMark = new PointMark(targetPointType, sourcePosIndex);
        this.pointMarks.Add(rePointMark);
    }

    private void deleteType(string typeName)
    {
        if (!containsPointType(typeName)) return;
        List<PointMark> newPointMarks = new List<PointMark>();
        foreach (PointMark pointMark in this.pointMarks)
        {
            if (pointMark.pointType != typeName)
            {
                newPointMarks.Add(pointMark);
            }
        }
        this.pointMarks = newPointMarks;
    }

    private bool isReferenceType(string typeName)
    {
        foreach (PointMark pointMarkTarget in pointMarks)
        {
            foreach (PointMark pointMark in pointMarks)
            {
                if (pointMarkTarget.pointType != pointMark.pointType &&
                    DataUtil.compareList<int>(pointMarkTarget.pointList, pointMark.pointList))
                {
                    return true;
                }
            }
        }
        return false;
    }

}