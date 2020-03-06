using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentViewName : MonoBehaviour
{
    private Text nameText = null;
    private void Awake()
    {
        nameText = this.GetComponentInChildren<Text>();
        EventManager.RegistEvent(EventDefine.OnSceneViewChange, (Action)ChangeCurrentViewName);
    }

    private void ChangeCurrentViewName()
    {
        switch (SceneMananger.Instance.CurrentSceneView)
        {
            case SceneView.ASView:
                nameText.text = "AS地图";
                break;
            case SceneView.MapView:
                nameText.text = "Map地图";
                break;
            case SceneView.IPView:
                nameText.text = "IP地图";
                break;
        }
    }
}
