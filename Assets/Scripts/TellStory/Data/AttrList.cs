using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttrList
{
    public AttrList()
    {
        attr_List = new List<AttrElement>();
    }
   public List<AttrElement> attr_List { set; get; }

    public AttrElement getElementByAttrName(string name)
    {
        foreach(AttrElement attrElement in attr_List)
        {
            if (attrElement.attrNameList.Contains(name))
            {
                return attrElement;
            }
        }
        return null;
    }

}

public class AttrElement
{
    public string attrType { get; }
    //public string attrName {  get; }

    public List<string> attrNameList { get; }
    public List<float> attrValue { get; }

    public AttrElement(string attrType, List<string> attrNameList, List<float> attrValue)
    {
        this.attrType = attrType;
        this.attrNameList = attrNameList;
        this.attrValue = attrValue;
    }

    public bool wordContainAttr(string word)
    {
        foreach (string attrName in attrNameList)
        {
            if (word.Contains(attrName)) return true;
        }
        return false;
    }


}
