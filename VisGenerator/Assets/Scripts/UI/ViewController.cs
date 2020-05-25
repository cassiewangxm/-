using System;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    private Toggle[] toggleList = null;
    void Start()
    {
        toggleList = this.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggleList.Length; i++)
        {
            Toggle toggle = toggleList[i];
            toggle.onValueChanged.AddListener(delegate { 
                OnToggleValueChanged(toggle); 
            });
        }
        EventManager.RegistEvent(EventDefine.OnSceneViewChange, (Action)OnSceneChange);
    }

    private void OnSceneChange()
    {
        switch (SceneMananger.Instance.CurrentSceneView)
        {
            case SceneView.ASView:
                RefreshToggleList("ASView");
                break;
            case SceneView.MapView:
                RefreshToggleList("MapView");
                break;
            case SceneView.IPView:
                RefreshToggleList("IPView");
                break;
        }
    }

    private void RefreshToggleList(string selectedToggle)
    {
        for (int i = 0; i < toggleList.Length; i++)
        {
            Toggle toggle = toggleList[i];
            toggle.isOn = toggle.gameObject.name == selectedToggle;
        }
    }

    private void OnToggleValueChanged(Toggle toggle)
    {
        if (!toggle.isOn)
            return;
        
        switch (toggle.gameObject.name)
        {
            case "ASView":
                SceneMananger.Instance.ChangeScene(SceneView.ASView);
                break;
            case "IPView":
                SceneMananger.Instance.ChangeScene(SceneView.IPView);
                break;
            case "MapView":
                SceneMananger.Instance.ChangeScene(SceneView.MapView);
                break;
        }
    }
}
