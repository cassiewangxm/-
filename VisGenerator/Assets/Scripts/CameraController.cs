using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using RTG;

namespace Controller
{
    // public enum ViewType
    // {
    //     ViewAS,
    //     ViewIP,
    //     ViewWanderingAS,
    //     ViewMap,
    //     ViewAll,
    // }

    public class CameraController : MonoBehaviour
    {
        public Consts Consts;

        public float Speed;
        public float Step;
        public GameObject Parent;
        public Camera Camera;
        Vector3 dragOrigin = Vector3.zero;

        private float cameraBound0;
        private float cameraBound1;
        private float cameraBound2;
        private float cameraBound3;

        public bool IsNavigation = false;
        public bool IsNavigationUpdate = false;

        public bool IsAS = true;
        public bool IsIP = false;
        public bool IsMap = false;
        public bool IsDefault = true;

        public GameObject RTFocusCamera;
        public Camera CameraAS;
        public Camera CameraIP;
        public Camera CameraMap;

        public Light ASLight;

        public GameObject ASGameObject;

        //public ViewType currentView = ViewType.ViewAS;

        public raycastas raycastas;

        public Filters Filters;

        public GameObject ASLightGO;

        Vector3 PyramidPosition;

        float HighlightFloat = 0.5f;
        bool HighlightBool = false;
        float HighlightSpeed = 0.018f;
        public float HighlightIntensity;

        bool IsInside(Vector3 move)
        {
            if (move.x < cameraBound0)
                return false;
            if (move.x > cameraBound1)
                return false;
            if (move.z < cameraBound2)
                return false;
            if (move.z > cameraBound3)
                return false;
            return true;
        }

        // Start is called before the first frame update
        void Start()
        {
            raycastas = GetComponent<raycastas>();

            cameraBound0 = Consts.ASPos.x;
            cameraBound1 = Consts.ASPos.x + Consts.ASSize.x;
            cameraBound2 = Consts.ASPos.y;
            cameraBound3 = Consts.ASPos.y + Consts.ASSize.y;

            PyramidPosition = Filters.PyramidParent.transform.position;
        }

        public void ViewAS()
        {
            SceneMananger.Instance.UpperBoundAS.y = 270.0f;
            SceneMananger.Instance.LowerBoundAS.y = 20.0f;
            //currentView = ViewType.ViewAS;
            Camera.cullingMask = 1025;
            IsAS = true;
            IsDefault = false;
            SetAS();
            CameraAS.depth = -1;
            CameraIP.depth = CameraMap.depth = -2;
            Rect viewport = GetComponent<Camera>().rect;
            viewport.height = 0.9f;
            GetComponent<Camera>().rect = viewport;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraAS);
            RTFocusCamera.GetComponent<RTFocusCamera>().MoveSettings.AccelerationRate = 15;
        }
        public void ViewWanderingAS(int x, int y)
        {
            if(ASProxy.instance.IsASExistInLocal(x,y))
            {
                raycastas.wanderingASMap.IntoWanderingMap(x,y);
                RTFocusCamera.GetComponent<RTFocusCamera>().MoveSettings.AccelerationRate = 0;
                //currentView = ViewType.ViewWanderingAS;
                Camera.cullingMask = 8193;
                IsAS = false;
                ASLightGO.SetActive(false);
                SceneMananger.Instance.UpperBoundAS.y = 5000.0f;
                SceneMananger.Instance.LowerBoundAS.y = -5000.0f;
            }
            else{
                Debug.LogErrorFormat("There is No AS at location : {0},{1}",x,y);
            }
        }
        public void ViewIP()
        {
            //currentView = ViewType.ViewIP;
            Camera.cullingMask = 2049;
            IsDefault = false;
            SetIP();
            CameraIP.depth = -1;
            CameraAS.depth = CameraMap.depth = -2;
            Rect viewport = CameraIP.GetComponent<Camera>().rect;
            viewport.height = 0.9f;
            viewport.width = 0.75f;
            viewport.x = 0.05f;
            viewport.y = 0.05f;
            CameraIP.GetComponent<Camera>().rect = viewport;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraIP);
        }
        public void ViewMap()
        {
            IsDefault = false;
            SetMap();
            CameraMap.depth = -1;
            CameraAS.depth = CameraIP.depth = -2;
            Rect viewport = CameraMap.GetComponent<Camera>().rect;
            viewport.height = 0.9f;
            viewport.width = 0.75f;
            viewport.x = 0.05f;
            viewport.y = 0.05f;
            CameraMap.GetComponent<Camera>().rect = viewport;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraMap);
        }
        public void ViewDefault()
        {
            IsDefault = true;
            CameraAS.depth = -1;
            CameraIP.depth = CameraMap.depth = -1;
            Rect viewport = GetComponent<Camera>().rect;
            viewport.height = 0.5f;
            viewport.width = 0.75f;
            viewport.x = 0.05f;
            viewport.y = 0.05f;
            GetComponent<Camera>().rect = viewport;
            viewport = CameraIP.GetComponent<Camera>().rect;
            viewport.height = 0.35f;
            viewport.width = 0.25f;
            viewport.x = 0.05f;
            viewport.y = 0.6f;
            CameraIP.GetComponent<Camera>().rect = viewport;
            viewport = CameraMap.GetComponent<Camera>().rect;
            viewport.height = 0.35f;
            viewport.width = 0.45f;
            viewport.x = 0.35f;
            viewport.y = 0.6f;
            CameraMap.GetComponent<Camera>().rect = viewport;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraAS);
        }
        public void SetAS()
        {
            IsAS = true;
            IsIP = false;
            IsMap = false;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraAS);
        }
        public void OnAS()
        {
            if (IsDefault)
            {
                SetAS();
            }
        }
        public void SetIP()
        {
            IsAS = false;
            IsIP = true;
            IsMap = false;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraIP);
        }
        public void OnIP()
        {
            if (IsDefault)
            {
                SetIP();
            }
        }
        public void SetMap()
        {
            IsAS = false;
            IsIP = false;
            IsMap = true;
            RTFocusCamera.GetComponent<RTFocusCamera>().SetTargetCamera(CameraMap);
        }
        public void OnMap()
        {
            if (IsDefault)
            {
                SetMap();
            }
        }
        public void ASNavigation()
        {
            IsNavigation = !IsNavigation;
            IsNavigationUpdate = true;
        }

        void IncreaseDepth()
        {
            Vector3 zoom = Parent.transform.position + Parent.transform.forward * (0.25f) * Step;
            if (IsInside(zoom) && (zoom.y > 25.0f))
            {
                Parent.transform.position = zoom;
            }
            else
            {
                IsNavigationUpdate = false;
            }
        }

        void DecreaseDepth()
        {
            Vector3 zoom = Parent.transform.position + Parent.transform.forward * (-0.25f) * Step;
            if (IsInside(zoom) && (zoom.y < 255.0f))
            {
                Parent.transform.position = zoom;
            }
            else
            {
                IsNavigationUpdate = false;
            }
        }

        public float GetModifiedASScale(float height)
        {
            return ((height / 250.0f * 0.8f + 0.2f) * 0.3f);
        }

        // Update is called once per frame
        void Update()
        {
            float height = CameraAS.transform.position.y;
            float asScale = GetModifiedASScale(height);
            float asGScale = 0.06f * GetModifiedASScale(252.0f) / GetModifiedASScale(height);
            float asItemsize = (height / 250.0f) * 0.4f + 0.6f;
            ASGameObject.GetComponent<VisualEffect>().SetFloat("scale", asScale);
            ASGameObject.GetComponent<VisualEffect>().SetFloat("gscale", asGScale);
            ASGameObject.GetComponent<VisualEffect>().SetFloat("itemsize", asItemsize);
            ASGameObject.GetComponent<VisualEffect>().SetFloat("HighlightFloat", (HighlightFloat > 1.0f) ? 1.0f : HighlightFloat);
            if (!HighlightBool)
            {
                HighlightFloat += HighlightSpeed * Time.deltaTime * 60.0f;
                if (HighlightFloat >= 1.25f)
                {
                    HighlightBool = !HighlightBool;
                }
            }
            else
            {
                HighlightFloat -= HighlightSpeed * Time.deltaTime * 60.0f;
                if (HighlightFloat <= 0.5f)
                {
                    HighlightBool = !HighlightBool;
                }
            }
            Filters.PyramidParent.transform.position = new Vector3(PyramidPosition.x, PyramidPosition.y + (HighlightFloat - 0.5f) * 2.0f, PyramidPosition.z);
            ASLight.intensity = (180.0f / (height + 50.0f)) * HighlightIntensity * (Filters.isHighlight ? 0.7f : 1.0f);
            if (SceneMananger.Instance.CurrentSceneView == SceneView.ASView)
            {
                RTFocusCamera.GetComponent<RTFocusCamera>().MoveSettings.MoveSpeed = CameraAS.transform.position.y / 250.0f * 6.0f;
            }
            else if (SceneMananger.Instance.CurrentSceneView == SceneView.IPView)
            {
                RTFocusCamera.GetComponent<RTFocusCamera>().MoveSettings.MoveSpeed = CameraIP.transform.position.y / 250.0f * 6.0f;
            }
            else if (SceneMananger.Instance.CurrentSceneView == SceneView.IPView)
            {
                RTFocusCamera.GetComponent<RTFocusCamera>().MoveSettings.MoveSpeed = CameraMap.transform.position.y / 250.0f * 6.0f;
            }
            Debug.Log(CameraAS.transform.position);
        }

    }

}
