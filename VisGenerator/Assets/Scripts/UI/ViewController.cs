using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    void Start()
    {
        Toggle[] toggleList = this.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggleList.Length; i++)
        {
            Toggle toggle = toggleList[i];
            toggle.onValueChanged.AddListener(delegate { 
                OnToggleValueChanged(toggle); 
            });
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
