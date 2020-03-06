using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToMyPos : MonoBehaviour
{
    private void Awake()
    {
        Button btn = this.GetComponentInChildren<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        //test code
        SceneMananger.Instance.ChangeScene(SceneView.MapView);
    }
}
