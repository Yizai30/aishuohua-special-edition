using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttrRecord
{
    public List<float> scale { set; get; }

    public List<float> color { set; get; }

    public AttrRecord(List<float> scale, List<float> color)
    {
        this.scale = scale;
        this.color = color;
    }

    public AttrRecord()
    {
        this.scale = new List<float>();
        this.color = new List<float>();
    }


}
