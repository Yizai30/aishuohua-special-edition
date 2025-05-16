using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public List<GameObject> gos;
    // Start is called before the first frame update
    void Start()
    {
       

        foreach(GameObject i in gos)
        {
           gos.Remove(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
