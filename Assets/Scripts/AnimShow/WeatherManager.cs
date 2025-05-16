using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    private Material mat;
    public GameObject plant;
    Vector3 hotcolor = new Vector3(116.0f, 142.0f, 91.0f);
    Vector3 winColor = new Vector3(213.0f, 224.0f, 203.0f);
    //Material treeMat;

    private Color toColor( Vector3  vcolor)
    {
        Color color = new Color();
        for(int i = 0; i < 3; i++)
        {
            color[i] = vcolor[i] / 255.0f;
        }
        return color;
    }

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
        GameObject cozyWeather = GameObject.Find("Cozy Weather Sphere");
        mat = plant.GetComponent<Renderer>().material;
        //changeToHot();
    }

    private void AttatchColor()
    {
       
    }

    public void changeToHot()
    {
        
       // mat.color = hotcolor;

        mat.SetColor("_Color", toColor(hotcolor));
        Debug.Log("it is hot");
    }
    public void changeToCold()
    {
        
        mat.SetColor("_Color", toColor(winColor));
        Debug.Log("it is cold");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
