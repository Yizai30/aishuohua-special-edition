using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RelatedPosElement
{
    public string valName{ get;}
    public List<float> upOffset { get; }

    public List<float> headOffset { get; }
    public List<float> bottomOffset{ get;}

    public RelatedPosElement(string valName, List<float> upOffset, List<float> headOffset, List<float> bottomOffset)
    {
        this.valName = valName;
        this.upOffset = upOffset;
        this.headOffset = headOffset;
        this.bottomOffset = bottomOffset;
    }
}
public class RelatedPosList
{
    public List <RelatedPosElement> related_pos_list { get; set; }

    public RelatedPosList(List<RelatedPosElement> related_pos_list)
    {
        this.related_pos_list = related_pos_list;
    }
    public RelatedPosList()
    {
        this.related_pos_list = new List<RelatedPosElement>();
    }

    public RelatedPosElement getElemByMatname(string matName)
    {
        foreach(RelatedPosElement relatedPosElement in DataMap.relatedPosList.related_pos_list)
        {
            if (relatedPosElement.valName.Equals(matName)) return relatedPosElement;
        }
        throw new Exception("没有找到物体的相对点位信息" + matName);
    }

    public List<float> getRelatedGapByMatnameAndPosName(string matname,string posname)
    {
        List<float> re = new List<float>();
        RelatedPosElement relatedPosElement = DataMap.relatedPosList.getElemByMatname(matname);
        if (posname.Equals("upOffset")) re = relatedPosElement.upOffset;
        else if (posname.Equals("headOffset")) re = relatedPosElement.headOffset;
        else if (posname.Equals("bottomOffset")) re = relatedPosElement.bottomOffset;
        else throw new Exception("未知的位置关系" + posname);
        return re;
    }
}



public class RelatedPosManager
{
     
    /*public static List<float> getRelatedPos(string subName,List<float> subPos,string relatePosName)
    {
        List<float> re = new List<float>();
        List<float> gapPos = DataMap.relatedPosList.getRelatedGapByMatnameAndPosName(subName, relatePosName);
        for(int i = 0; i < gapPos.Count; i++)
        {
            re.Add(gapPos[i] + subPos[i]);
        }
        return re;

    }*/
    public static List<float> getRelatedPos(string subName, string obName, List<float> subPos, List<float> subSca, List<float> obSca, string relatePosName)
    {
        List<float> re = new List<float>();
        string relatePosName2=relatePosName;
        if (relatePosName.Equals("upOffset")) relatePosName2="bottomOffset";
        else if (relatePosName.Equals("headOffset")) relatePosName2="bottomOffset";
        else if (relatePosName.Equals("bottomOffset")) relatePosName2="upOffset";
        else throw new Exception("未知的位置关系" + relatePosName);
        List<float> gapPos = DataMap.relatedPosList.getRelatedGapByMatnameAndPosName(subName, relatePosName);
        List<float> posOffset = DataMap.relatedPosList.getRelatedGapByMatnameAndPosName(obName, relatePosName2);
        for(int i = 0; i < gapPos.Count; i++)
        {
            gapPos[i]*=subSca[i];
            posOffset[i]*=obSca[i]*-1f;
            re.Add(gapPos[i] + subPos[i] + posOffset[i]);
        }
        return re;

    }
    // Start is called before the first frame update
  /*
    
    
  */
}
