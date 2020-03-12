using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;
using System.IO;

public class ViewIP : MonoBehaviour
{
    public Consts Consts;

    public Camera Camera;
    public RTFocusCamera RTFocusCamera;

    public GameObject IP20;
    public GameObject IP24;
    public GameObject IP28;
    public GameObject IP32;

    public GameObject IPQuadPrefab;

    private GameObject[] IPLevels = new GameObject[4];
    private int[] IPLevelScales = { 1, 4, 16, 64 };

    public float heightLv20;
    private float heightLv24;
    private float heightLv28;
    private float heightLv32;

    private Vector2 IPSize;
    private Vector2 IPPos;

    private Vector2 lastCameraIdx;
    private int lastLv;

    public Vector2 GetIPFullSize()
    {
        return new Vector2(Consts.IPSize.x * IPLevelScales[IPLevelScales.Length - 1], Consts.IPSize.y * IPLevelScales[IPLevelScales.Length - 1]);
    }

    // Start is called before the first frame update
    void Start()
    {
        IPSize.x = Consts.IPSize.x * IPLevelScales[IPLevelScales.Length - 1];
        IPSize.y = Consts.IPSize.y * IPLevelScales[IPLevelScales.Length - 1];
        IPPos = Consts.IPPos;

        IPLevels[0] = IP20;
        IPLevels[1] = IP24;
        IPLevels[2] = IP28;
        IPLevels[3] = IP32;

        lastCameraIdx = Vector2.zero;
        lastLv = -1;

        heightLv24 = heightLv20 / 4.0f;
        heightLv28 = heightLv24 / 4.0f;
        heightLv32 = heightLv28 / 4.0f;
    }

    Vector2 CalculateIdx(int scale)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray cameraRay = new Ray(Camera.transform.position, Camera.transform.forward);
        float cameraEnter = 0.0f;
        plane.Raycast(cameraRay, out cameraEnter);
        Vector3 cameraHitPoint = cameraRay.GetPoint(cameraEnter);
        float x = ((cameraHitPoint.x - IPPos.x + IPSize.x / 2.0f) / IPSize.x) * scale;
        float y = ((cameraHitPoint.z - IPPos.y + IPSize.y / 2.0f) / IPSize.y) * scale;
        return new Vector2(x, y);
    }

    void TransformIPdata(IpDetail[] IpDetails, MessageRequestIpMap message)
    {
        Texture2D texip = new Texture2D(256, 256, TextureFormat.RGB24, false);
        for (int xi = 0; xi < 1024; xi++)
        {
            for (int yi = 0; yi < 256; yi++)
            {
                texip.SetPixel(xi, yi, new Color(0.0f, 0.0f, 0.0f));
            }
        }

        for (int i = 0; i < IpDetails.Length; i ++)
        {
            IpDetail Item = IpDetails[i];

            texip.SetPixel(Item.X, Item.Y, new Color(0.0f, 0.0f, 0.0f));

        }
        texip.Apply();
        byte[] bytesip = texip.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/IP/Data/IPTexture" + 1 + ".png", bytesip);
    }

    void GetIPData(int lv, int TopIdx, int LeftIdx)
    {
        float x = (LeftIdx + 0.5f) * Consts.IPSize.x * IPLevelScales[IPLevelScales.Length - lv - 1] - IPSize.x / 2.0f;
        float y = (TopIdx + 0.5f) * Consts.IPSize.y * IPLevelScales[IPLevelScales.Length - lv - 1] - IPSize.y / 2.0f;
        Vector3 NewQuadPos = new Vector3(x, 0.0f, y);
        GameObject NewQuad = Instantiate(IPQuadPrefab, NewQuadPos, Quaternion.identity);
        NewQuad.transform.SetParent(IPLevels[lv].transform);
        NewQuad.transform.localScale = Vector3.one;
        NewQuad.transform.position = NewQuadPos;
        NewQuad.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        // Apply IP data here
        if (lv == 0)
        {
            IPProxy.instance.GetIpInfoBlock(TransformIPdata);
        }
    }

    void UpdateIPView(int lv)
    {
        Vector2 CameraIdx = CalculateIdx(IPLevelScales[lv]);
        int x, y;
        // Center
        x = (int)CameraIdx.x;
        y = (int)CameraIdx.y;

        Debug.Log("IP View: " + x.ToString() + " - " + y.ToString() + " at LV " + lv.ToString());

        if ((new Vector2(x, y) == lastCameraIdx) && (lv == lastLv))
        {
            return;
        }
        else
        {
            lastCameraIdx = new Vector2(x, y);
            if (lastLv != -1)
            {
                for (int i = IPLevels[lastLv].transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = IPLevels[lastLv].transform.GetChild(i);
                    child.parent = null;
                    Destroy(child.gameObject);
                }
            }
            lastLv = lv;
            for (int i = IPLevels[lv].transform.childCount - 1; i >= 0; i --)
            {
                Transform child = IPLevels[lv].transform.GetChild(i);
                child.parent = null;
                Destroy(child.gameObject);
            }
        }

        GetIPData(lv, x, y);
        // Left
        x -= 1;
        if (x >= 0)
        {
            GetIPData(lv, x, y);
        }
        // Top Left
        y -= 1;
        if ((x >= 0) && (y >= 0))
        {
            GetIPData(lv, x, y);
        }
        // Top
        x += 1;
        if (y >= 0)
        {
            GetIPData(lv, x, y);
        }
        // Top Right
        x += 1;
        if ((y >= 0) && (x < IPLevelScales[lv]))
        {
            GetIPData(lv, x, y);
        }
        // Right
        y += 1;
        if (x < IPLevelScales[lv])
        {
            GetIPData(lv, x, y);
        }
        // Bottom Right
        y += 1;
        if ((x < IPLevelScales[lv]) && (y < IPLevelScales[lv]))
        {
            GetIPData(lv, x, y);
        }
        // Bottom
        x -= 1;
        if (y < IPLevelScales[lv])
        {
            GetIPData(lv, x, y);
        }
        // Bottom Left
        x -= 1;
        if ((y < IPLevelScales[lv]) && (x >= 0))
        {
            GetIPData(lv, x, y);
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
                    UpdateIPView(3);

                    IP32.SetActive(true);
                }
                else
                {
                    // Request data 28
                    UpdateIPView(2);

                    IP28.SetActive(true);
                }
            }
            else
            {
                // Request data 24
                UpdateIPView(1);

                IP24.SetActive(true);
            }
        }
        else
        {
            // Request data 20
            UpdateIPView(0);

            IP20.SetActive(true);
        }
    }
}
