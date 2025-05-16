using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAMR : MonoBehaviour
{
    public ServerPoster serverPoster;
    List<AMRGraph> amrList = new List<AMRGraph>();
    // Start is called before the first frame update
    void Start()
    {
        string line = "���� �� �� ���� �� ɭ����";
       
        StartCoroutine(serverPoster.getAMR(line, amrList, printAmr));
    }

    void printAmr()
    {
        print(amrList[0].edges.Count);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
