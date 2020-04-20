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

    public GameObject ASLightGO;

    public Vector3 UpperBoundAS;
    public Vector3 UpperBoundIP;
    public Vector3 UpperBoundMap;
    public Vector3 LowerBoundAS;
    public Vector3 LowerBoundIP;
    public Vector3 LowerBoundMap;

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

    private Dictionary<SceneView, GameObject> viewTypeToCameraObjMapping = null;
    private Dictionary<SceneView, GameObject> viewTypeToViewObjMapping = null;

    private void Awake()
    {
        ms_instance = this;
        viewTypeToCameraObjMapping = new Dictionary<SceneView, GameObject>();
        viewTypeToCameraObjMapping.Add(SceneView.ASView, GameObject.Find("CameraParent-AS"));
        viewTypeToCameraObjMapping.Add(SceneView.IPView, GameObject.Find("CameraParent-IP"));
        viewTypeToCameraObjMapping.Add(SceneView.MapView, GameObject.Find("CameraParent-Map"));
        
        viewTypeToViewObjMapping = new Dictionary<SceneView, GameObject>();
        viewTypeToViewObjMapping.Add(SceneView.ASView, GameObject.Find("ViewAS"));
        viewTypeToViewObjMapping.Add(SceneView.IPView, GameObject.Find("ViewIP"));
        viewTypeToViewObjMapping.Add(SceneView.MapView, GameObject.Find("ViewMap"));
        
        ChangeScene(SceneView.ASView);
    }

    public void ChangeScene(SceneView targetSceneView, bool doTween = true)
    {
        if (currentSceneView == targetSceneView)
            return;

        lastSceneView = currentSceneView;
        currentSceneView = targetSceneView;
        curChangeSceneTweenTime = doTween ? 0 : changeSceneTweenTime;
        
        foreach (var pair in viewTypeToCameraObjMapping)
        {
            bool show = pair.Key == currentSceneView || pair.Key == lastSceneView;
            pair.Value.SetActive(show);
            viewTypeToViewObjMapping[pair.Key].SetActive(show);
            if (pair.Key == currentSceneView) 
                RTFocusCamera.SetTargetCamera(pair.Value.GetComponentInChildren<Camera>());
        }
        EventManager.SendEvent(EventDefine.OnSceneViewChange);

        if (targetSceneView == SceneView.ASView)
        {
            RTFocusCamera.UpperBound = UpperBoundAS;
            RTFocusCamera.LowerBound = LowerBoundAS;
            RTFocusCamera.LookAroundSettings.IsLookAroundEnabled = true;
        }
        else if (targetSceneView == SceneView.IPView)
        {
            RTFocusCamera.UpperBound = UpperBoundIP;
            RTFocusCamera.LowerBound = LowerBoundIP;
            RTFocusCamera.LookAroundSettings.IsLookAroundEnabled = false;
        }
        else if (targetSceneView == SceneView.MapView)
        {
            RTFocusCamera.UpperBound = UpperBoundMap;
            RTFocusCamera.LowerBound = LowerBoundMap;
            RTFocusCamera.LookAroundSettings.IsLookAroundEnabled = false;
        }
    }

    private void Update()
    {
        if (lastSceneView == currentSceneView || lastSceneView == SceneView.Invliad)
            return;
        
        GameObject lastSceneViewObj = viewTypeToCameraObjMapping[lastSceneView];
        Camera lastSceneViewCamera = lastSceneViewObj.GetComponentInChildren<Camera>();
        GameObject curSceneViewObj = viewTypeToCameraObjMapping[currentSceneView];
        Camera curSceneViewCamera = curSceneViewObj.GetComponentInChildren<Camera>();

        curChangeSceneTweenTime += Time.deltaTime;
        bool finishTween = curChangeSceneTweenTime >= changeSceneTweenTime;
        int offset = lastSceneView - currentSceneView;
        if (finishTween)
        {
            curChangeSceneTweenTime = changeSceneTweenTime;
            lastSceneViewObj.SetActive(false);
            viewTypeToViewObjMapping[lastSceneView].SetActive(false);
            lastSceneView = currentSceneView;
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