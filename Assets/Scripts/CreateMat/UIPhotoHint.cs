using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotoHint : MonoBehaviour
{
    public Button confirmButton;
    // Start is called before the first frame update
    void Start()
    {
        confirmButton.onClick.AddListener(() =>{ this.gameObject.SetActive(false); });
    }

  
}
