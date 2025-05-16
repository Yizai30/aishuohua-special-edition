using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//获取gameobject的长宽
public class BoxBoundSampler : MonoBehaviour
{
    GameObject go;
    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D boxCollider2D = go.GetComponent<BoxCollider2D>();
        Debug.Log(boxCollider2D.size.x);
        Debug.Log(boxCollider2D.size.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
