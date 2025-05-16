using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatSettingList
{
   public List<MatSettingElement> matSettingList { get; set; }

    public MatSettingElement getMatSettingMap(string matName)
    {
        MatSettingElement tmpSizeElement = new MatSettingElement();
        foreach (MatSettingElement sizeElement in matSettingList)
        {
            if (sizeElement.matName.Equals(matName))
            {
                tmpSizeElement = sizeElement;
            }
        }
        if (tmpSizeElement.matName.Equals(""))
        {
            throw new System.Exception("没有找到素材的初始信息"+matName);
        }
        return tmpSizeElement;
    }

    public float getMatDefaultSpeed(string matName)
    {
        MatSettingElement matSettingElement = getMatSettingMap(matName);
        return matSettingElement.speed;
    }

    public MatSettingList(List<MatSettingElement> sizeList)
    {
        this.matSettingList = sizeList;
    }

    public MatSettingList()
    {
        this.matSettingList = new List<MatSettingElement>();
    }

}


public class MatSettingElement
{
    public string matName { set; get; }
    //public List<float> origin_size { set; get; }
    public List<float> initScale { set; get; }

    public List<float> initRotation { set; get; }//素材的初始朝向角度，即在本项目中，初始的朝向

    public List<string> classList { set; get; }

    public float width { set; get; }
    public float height { set; get; }

    public float speed;

    //public List<float> origin_forward { set; get; }//素材原始朝向向量


    public MatSettingElement() { }

    public MatSettingElement(string matName, List<float> initScale, List<float> initRotation, List<string> classList, float width, float height, float speed)
    {
        this.matName = matName;
        this.initScale = initScale;
        this.initRotation = initRotation;
        this.classList = classList;
        this.width = width;
        this.height = height;
        this.speed = speed;
    }
}
