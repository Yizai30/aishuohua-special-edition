using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathTransform
{
    public List<float> startPosition { set; get; }
    public List<float> endPosition { set; get; }

    public List<float> startRotation { set; get; }

    public List<float> endRotation { set; get; }

    public List<float> startScale { set; get; }
    public List<float> endScale { set; get; }

    public PathTransform(List<float> startPosition, List<float> endPosition, List<float> startRotation, List<float> endRotation, List<float> startScale, List<float> endScale)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.startRotation = startRotation;
        this.endRotation = endRotation;
        this.startScale = startScale;
        this.endScale = endScale;
    }

    public void init()
    {
        
    }

    public PathTransform() {
        this.startPosition = new List<float> { 0, 0, 0 };
        this.endPosition = new List<float> { 0, 0, 0 };
        this.startRotation = new List<float> { 0, 0, 0 };
        this.endRotation = new List<float> { 0, 0, 0 };
        this.startScale = new List<float> { 0, 0, 0 };
        this.endScale = new List<float> { 0, 0, 0 };
    }

}
