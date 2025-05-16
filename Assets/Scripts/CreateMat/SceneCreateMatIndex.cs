using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneCreateMatIndex : MonoBehaviour
{

    public Button backBtn, createBkgBtn, createPropBtn, createActorBtn, userMatListBtn;

    // Start is called before the first frame update
    void Start()
    {
        backBtn.onClick.AddListener(()=> { SceneManager.LoadScene("SceneMainMenu"); });
        createActorBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneCreateActor"); SceneMsgToMatCreate.MatType = 1; });
        createPropBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneCreateActor"); SceneMsgToMatCreate.MatType = 2; });
        createBkgBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneCreateActor"); SceneMsgToMatCreate.MatType = 3; });

        userMatListBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneMyMat"); });
        //createBkgBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneMainMenu"); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
