using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTG;

namespace Controller
{
    public enum ViewType
    {
        ViewAS,
        ViewIP,
        ViewSingleAS,
        ViewMap,
        ViewAll,
    }

    public class CameraController : MonoBehaviour
    {
        public float Speed;
        public float Step;
        public GameObject Parent;
        public Camera Camera;
        Vector3 dragOrigin = Vector3.zero;

        public float cameraBound0;
        public float cameraBound1;
        public float cameraBound2;
        public float cameraBound3;

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

        public ViewType currentView = ViewType.ViewAS;

        public ViewType currentView = ViewType.ViewAS;

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
        }

        public void ViewAS()
        {
            currentView = ViewType.ViewAS;
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
        }
        public void ViewSingleAS()
        {
            currentView = ViewType.ViewSingleAS;
            Camera.cullingMask = 8193 + 1025;
            IsAS = false;
        }
        public void ViewSingleAS()
        {
            currentView = ViewType.ViewSingleAS;
            Camera.cullingMask = 4097 + 1025;
            IsAS = false;
        }
        public void ViewIP()
        {
            currentView = ViewType.ViewIP;
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

        // Update is called once per frame
        void Update()
        {
            /*
            if (!IsNavigationUpdate)
            {
                Vector3 zoom = Parent.transform.position + Parent.transform.forward * Input.GetAxis("Mouse ScrollWheel") * Step;
                if (IsInside(zoom))
                {
                    if ((!IsNavigation) && (zoom.y < 255.0f) && (zoom.y > 25.0f))
                    Parent.transform.position = zoom;
                }

                if (!Input.GetMouseButton(2))
                {
                    return;
                }


                if (Input.GetMouseButtonDown(2))
                {
                    dragOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                    return;
                }


                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - dragOrigin;
                Vector3 move = new Vector3(pos.x * (IsNavigation ? Speed / 2 : Speed), 0, pos.y * (IsNavigation ? Speed / 2 : Speed));

                Vector3 des = Parent.transform.position + Parent.transform.TransformDirection(move);
                if (IsInside(des))
                {
                    Parent.transform.position = des;
                }
            }
            else
            {
                if (IsNavigation)
                {
                    IncreaseDepth();
                }
                else
                {
                    DecreaseDepth();
                }
            }
            */
        }

    }

}
