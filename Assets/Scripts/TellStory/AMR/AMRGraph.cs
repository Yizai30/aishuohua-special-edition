using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AMRGraph
{

    public string id { set; get; }
    public string input { set; get; }
    public List<AMRNode> nodes { set; get; }
    public List<AMREdge> edges { set; get; }
    public List<int> tops { set; get; }
    public string framework { set; get; }
    public AMRGraph()
    {
        nodes = new List<AMRNode>();
        edges = new List<AMREdge>();
        tops = new List<int>();
    }

    public AMRNode getNodeById(int nodeId)
    {
        AMRNode re = null;
        foreach(AMRNode node in nodes)
        {
            if (node.id == nodeId)
            {
                re = node;
            }
        }
        return re;
    }
    


}

[Serializable]
public class AMRNode
{
    public int id { set; get; }
    public string label { set; get; }
    public List<AMRAnchor> anchors { set; get; }
    public AMRNode()
    {
        anchors = new List<AMRAnchor>();
    }
}

[Serializable]
public class AMREdge
{
    public int source { set; get; }
    public int target { set; get; }
    public string label { set; get; }
}

[Serializable]
public class AMRAnchor
{
    public int from { set; get; }
    public int to { set; get; }
}
