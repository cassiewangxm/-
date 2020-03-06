using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneView
{
    Invliad,
    ASView,
    IPView,
    MapView,
}

public class SceneMananger : MonoBehaviour
{
    private static SceneMananger ms_instance = null;

    public static SceneMananger Instance
    {
        get
        {
            if (ms_instance == null)
            {
                GameObject obj = new GameObject("SceneManager");
                GameObject parent = GameObject.Find("Singleton");
                if (parent != null)
                    obj.transform.parent = parent.transform;
                ms_instance = obj.AddComponent<SceneMananger>();
            }

            return ms_instance;
        }
    }

    private SceneView currentSceneView = SceneView.Invliad;

    public SceneView CurrentSceneView
    {
        get { return currentSceneView; }
    }

    private Dictionary<SceneView, GameObject> viewToObjMapping = null;

    private void Awake()
    {
        ms_instance = this;
        viewToObjMapping = new Dictionary<SceneView, GameObject>();
        viewToObjMapping.Add(SceneView.ASView, GameObject.Find("CameraParent-AS"));
        viewToObjMapping.Add(SceneView.IPView, GameObject.Find("CameraParent-IP"));
        viewToObjMapping.Add(SceneView.MapView, GameObject.Find("CameraParent-Map"));
        
        ChangeScene(SceneView.ASView);
    }

    public void ChangeScene(SceneView targetSceneView)
    {
        if (currentSceneView == targetSceneView)
            return;
        currentSceneView = targetSceneView;
        foreach (var pair in viewToObjMapping)
        { 
            pair.Value.SetActive(pair.Key == currentSceneView);
        }
    }
}