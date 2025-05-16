using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class fixName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath+"/Resources/Animation/cuteAnimal";
        var dirlist=Directory.GetDirectories(path); 
     
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
