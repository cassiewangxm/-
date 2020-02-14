using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            IsAS = false;
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
