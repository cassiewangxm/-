using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;

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

    public RTFocusCamera RTFocusCamera;

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
    private SceneView lastSceneView = SceneView.Invliad;
    private float changeSceneTweenTime = 0.3f;
    private float curChangeSceneTweenTime = 0f;

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

    public void ChangeScene(SceneView targetSceneView, bool doTween = true)
    {
        if (currentSceneView == targetSceneView)
            return;

        lastSceneView = currentSceneView;
        currentSceneView = targetSceneView;
        curChangeSceneTweenTime = doTween ? 0 : changeSceneTweenTime;
        
        foreach (var pair in viewToObjMapping)
        { 
            pair.Value.SetActive(pair.Key == currentSceneView || pair.Key == lastSceneView);
            if (pair.Key == currentSceneView) 
                RTFocusCamera.SetTargetCamera(pair.Value.GetComponentInChildren<Camera>());
        }
        EventManager.SendEvent(EventDefine.OnSceneViewChange);
    }

    private void Update()
    {
        if (lastSceneView == currentSceneView || lastSceneView == SceneView.Invliad)
            return;
        
        GameObject lastSceneViewObj = viewToObjMapping[lastSceneView];
        Camera lastSceneViewCamera = lastSceneViewObj.GetComponentInChildren<Camera>();
        GameObject curSceneViewObj = viewToObjMapping[currentSceneView];
        Camera curSceneViewCamera = curSceneViewObj.GetComponentInChildren<Camera>();

        curChangeSceneTweenTime += Time.deltaTime;
        bool finishTween = curChangeSceneTweenTime >= changeSceneTweenTime;
        int offset = lastSceneView - currentSceneView;
        if (finishTween)
        {
            curChangeSceneTweenTime = changeSceneTweenTime;
            lastSceneView = currentSceneView;
            lastSceneViewObj.SetActive(false);
        }
        
        float tmp = Mathf.Lerp(0, 1, curChangeSceneTweenTime / changeSceneTweenTime);
        if (offset < 0)
        {
            lastSceneViewCamera.rect = new Rect(0, 0, 1 - tmp, 1);
            curSceneViewCamera.rect = new Rect(1 - tmp, 0, tmp, 1);
        }
        else
        {
            lastSceneViewCamera.rect = new Rect(tmp, 0, 1 - tmp, 1);
            curSceneViewCamera.rect = new Rect(0, 0, tmp, 1);
        }
    }
}