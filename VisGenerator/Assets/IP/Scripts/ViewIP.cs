using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;

public class ViewIP : MonoBehaviour
{
    public Camera Camera;
    public RTFocusCamera RTFocusCamera;

    public GameObject IP20;
    public GameObject IP24;
    public GameObject IP28;
    public GameObject IP32;

    private float heightLv24;
    private float heightLv28;
    private float heightLv32;

    public Vector2 IPSize;
    public Vector2 IPPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector2 CalculateIdx(int level, Vector3 cameraPos)
    {
        float x = ((cameraPos.x - IPPos.x) / IPSize.x) * level;
        float y = ((cameraPos.z - IPPos.y) / IPSize.y) * level;
        return new Vector2(x, y);
    }

    void GetIPData(int level, int TopIdx, int LeftIdx)
    {

    }

    void UpdateIPView(int level, Vector3 cameraPos)
    {
        Vector2 CameraIdx = CalculateIdx(level, cameraPos);
        int x, y;
        // Center
        x = (int)CameraIdx.x;
        y = (int)CameraIdx.y;
        GetIPData(level, x, y);
        // Left
        x -= 1;
        if (x >= 0)
        {
            GetIPData(level, x, y);
        }
        // Top Left
        y -= 1;
        if ((x >= 0) && (y >= 0))
        {
            GetIPData(level, x, y);
        }
        // Top
        x += 1;
        if (y >= 0)
        {
            GetIPData(level, x, y);
        }
        // Top Right
        x += 1;
        if ((y >= 0) && (x < level))
        {
            GetIPData(level, x, y);
        }
        // Right
        y -= 1;
        if (x < level)
        {
            GetIPData(level, x, y);
        }
        // Bottom Right
        y -= 1;
        if ((x < level) && (y < level))
        {
            GetIPData(level, x, y);
        }
        // Bottom
        x -= 1;
        if (y < level)
        {
            GetIPData(level, x, y);
        }
        // Bottom Left
        x -= 1;
        if ((y < level) && (x >= 0))
        {
            GetIPData(level, x, y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (RTFocusCamera.TargetCamera == Camera)
        {
            RTFocusCamera.LookAroundSettings.IsLookAroundEnabled = false;
        }
        else
        {
            RTFocusCamera.LookAroundSettings.IsLookAroundEnabled = true;
        }
        Debug.Log(Camera.transform.position.y);
        if (Camera.transform.position.y < heightLv24)
        {
            IP20.SetActive(false);
            if (Camera.transform.position.y < heightLv28)
            {
                IP24.SetActive(false);
                if (Camera.transform.position.y < heightLv32)
                {
                    IP28.SetActive(false);

                    // Request data 32
                    UpdateIPView(64, Camera.transform.position);

                    IP32.SetActive(true);
                }
                else
                {
                    // Request data 28
                    UpdateIPView(16, Camera.transform.position);

                    IP28.SetActive(true);
                }
            }
            else
            {
                // Request data 24
                UpdateIPView(4, Camera.transform.position);

                IP24.SetActive(true);
            }
        }
        else
        {
            // Request data 20
            UpdateIPView(1, Camera.transform.position);

            IP20.SetActive(true);
        }
    }
}
