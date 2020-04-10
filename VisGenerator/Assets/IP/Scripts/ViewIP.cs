using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTG;
using System.IO;

public class ViewIP : MonoBehaviour
{
    public class IPPixelData
    {
        public int X;
        public int Y;
        public int ASN;
    }

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
    private float heightIconOn;

    private Vector2 IPSize;
    private Vector2 IPPos;

    private Vector2 lastCameraIdx;
    private int lastLv;

    private Vector2 lastCenterPosition = Vector2.zero;

    public Material MaterialTemplate;

    /*
    private Texture2D[] IPTextures28 = new Texture2D[256];
    private Texture2D[] IPTextures24 = new Texture2D[16];
    private Texture2D IPTextures20;
    */

    public Vector2 GetIPFullSize()
    {
        return new Vector2(Consts.IPSize.x * IPLevelScales[IPLevelScales.Length - 1], Consts.IPSize.y * IPLevelScales[IPLevelScales.Length - 1]);
    }

    /*
    void InitIPData()
    {
        for (int i = 0; i < 256; i ++)
        {
            IPTextures28[i] = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            for (int x = 0; x < 1024; x ++)
            {
                for (int y = 0; y < 1024; y ++)
                {
                    IPTextures28[i].SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f));
                }
            }
        }
    }
    */

    /*
    void InitIPData24()
    {
        for (int i = 0; i < 16; i++)
        {
            IPTextures24[i] = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
            for (int x = 0; x < 1024; x++)
            {
                for (int y = 0; y < 1024; y++)
                {
                    IPTextures24[i].SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f));
                }
            }
        }
    }
    */

    /*
    void InitIPData20()
    {
        IPTextures20 = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        for (int x = 0; x < 1024; x++)
        {
            for (int y = 0; y < 1024; y++)
            {
                IPTextures20.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f));
            }
        }
    }
    */

    /*
    Color PixelCompression(Texture2D texture2D, int x, int y)
    {
        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        for (int i = 0; i < 4; i ++)
        {
            for (int j = 0; j < 4; j ++)
            {
                Color color = texture2D.GetPixel(x * 4 + i, y * 4 + j);
                r += color.r;
                g += color.g;
                b += color.b;
            }
        }
        return new Color(r / 16.0f, g / 16.0f, b / 16.0f);
    }
    */

    /*
    void ProcessIPData24()
    {
        InitIPData24();

        Debug.Log("Processing IPs..");

        for (int i = 0; i < 256; i++)
        {
            string texture = "Data/IPTexture/24-" + (i / 16 * 1024).ToString() + "-" + (i % 16 * 1024).ToString() + "";
            IPTextures28[i] = Resources.Load<Texture2D>(texture);
            for (int x = 0; x < 256; x ++)
            {
                for (int y = 0; y < 256; y ++)
                {
                    int IndexX = (i / 64);
                    int IndexY = ((i % 16) / 4);
                    int PixelX = (i / 16 * 256) % 1024 + x;
                    int PixelY = ((i % 16) * 256) % 1024 + y;
                    IPTextures24[IndexX * 4 + IndexY].SetPixel(PixelX, PixelY, PixelCompression(IPTextures28[i], x, y));
                }
            }
        }
        for (int i = 0; i < 16; i ++)
        {
            IPTextures24[i].Apply();
            byte[] bytesip = IPTextures24[i].EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/IP/Data/IPTexture/20-" + (i / 4 * 1024).ToString() + "-" + (i % 4 * 1024).ToString() + ".png", bytesip);
        }
        Debug.Log("IP Processing Finished.");
    }
    */

    /*
    void ProcessIPData20()
    {
        InitIPData20();

        Debug.Log("Processing IPs..");

        for (int i = 0; i < 16; i++)
        {
            string texture = "Data/IPTexture/20-" + (i / 4 * 1024).ToString() + "-" + (i % 4 * 1024).ToString() + "";
            IPTextures24[i] = Resources.Load<Texture2D>(texture);
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    int PixelX = (i / 4 * 256) + x;
                    int PixelY = ((i % 4) * 256) + y;
                    IPTextures20.SetPixel(PixelX, PixelY, PixelCompression(IPTextures24[i], x, y));
                }
            }
        }

        IPTextures20.Apply();
        byte[] bytesip = IPTextures20.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/IP/Data/IPTexture/16-0-0.png", bytesip);
        Debug.Log("IP Processing Finished.");
    }
    */

    /*
    IEnumerator ProcessIPData()
    {
        InitIPData();
        string line;

        StreamReader file = new StreamReader("Assets/prefix_NXY_28.json");

        Debug.Log("Processing IPs..");
        int count = 0;
        while (((line = file.ReadLine()) != null))
        {
            IPPixelData PixelData = JsonUtility.FromJson<IPPixelData>(line);
            int IndexX = PixelData.X / 1024;
            int IndexY = PixelData.Y / 1024;
            int PixelX = PixelData.X % 1024;
            int PixelY = PixelData.Y % 1024;
            IPTextures28[IndexX * 16 + IndexY].SetPixel(PixelX, PixelY, CalculateColor(PixelData.ASN));
            count++;
            if (count % 1048576 == 0)
            {
                Debug.Log("IP Lines Read: " + (count / 1048576).ToString());
                yield return null;
            }
        }

        file.Close();

        for (int i = 0; i < 256; i++)
        {
            Debug.Log("Processing IP Block " + i.ToString() + "..");
            IPTextures28[i].Apply();
            byte[] bytesip = IPTextures28[i].EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/IP/Data/IPTexture/24-" + (i / 16 * 1024).ToString() + "-" + (i % 16 * 1024).ToString() + ".png", bytesip);
        }
        Debug.Log("IP Processing Finished.");
    }
    */

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
        heightIconOn = heightLv32 / 4.0f;
    }

    Vector2 CalculatePosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray cameraRay = new Ray(Camera.transform.position, Camera.transform.forward);
        float cameraEnter = 0.0f;
        plane.Raycast(cameraRay, out cameraEnter);
        Vector3 cameraHitPoint = cameraRay.GetPoint(cameraEnter);
        float x = (cameraHitPoint.x - IPPos.x + IPSize.x / 2.0f);
        float y = (cameraHitPoint.z - IPPos.y + IPSize.y / 2.0f);
        return new Vector2(x, y);
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

    Color CalculateColor(int asnum)
    {
        System.Random rnd = new System.Random();
        int x = (asnum / 256) % 256;
        int y = (asnum % 256) % 256;
        int r, g, b;
        if (((y - x) < 128) && ((x - y) < 128))
        {
            g = x;
            r = y;
            b = 96 + rnd.Next(0, 32);
        }
        else
        {
            g = (x >= 128) ? x : (x + 128);
            r = (y >= 128) ? y : (x + 128);
            b = (x >= 128) ? (32 + rnd.Next(0, 32)) : (64 + rnd.Next(0, 32));
        }
        return new Color(r / 256.0f, g / 256.0f, b / 256.0f);
    }

    void TransformIPdata(IpDetail[] IpDetails, IPLayerInfo info)
    {
        Debug.Log("Generating IP Texture...");
        Texture2D texip = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        for (int xi = 0; xi < 1024; xi++)
        {
            for (int yi = 0; yi < 1024; yi++)
            {
                texip.SetPixel(xi, yi, new Color(0.0f, 0.0f, 0.0f));
            }
        }

        for (int i = 0; i < IpDetails.Length; i ++)
        {
            IpDetail Item = IpDetails[i];
            texip.SetPixel(Item.X - info.x, Item.Y - info.y, CalculateColor((int)Item.ASNum));
        }
        texip.Apply();
        byte[] bytesip = texip.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/IP/Data/IPTexture/" + info.prefixLen + "-" + info.x.ToString() + "-" + info.y.ToString() + ".png", bytesip);
    }

    Texture2D GenerateTmpTexture(Texture2D texture, int xOffset, int yOffset)
    {
        Texture2D NewTexture = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        System.Random rnd = new System.Random();

        for (int i = 0; i < 1024; i ++)
        {
            for (int j = 0; j < 1024; j ++)
            {
                Color color = texture.GetPixel(xOffset * 256 + (i / 4), yOffset * 256 + (j / 4));
                color.r += ((rnd.Next(0, 96) - 48) / 256.0f);
                color.g += ((rnd.Next(0, 96) - 48) / 256.0f);
                color.b += ((rnd.Next(0, 96) - 48) / 256.0f);
                NewTexture.SetPixel(i, j, color);
            }
        }

        return NewTexture;
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
        if (lv == 3)
        {
            /*
            string texture = "Data/IPTexture/" + 24.ToString() + "-" + (LeftIdx / 4 * 1024).ToString() + "-" + (TopIdx / 4 * 1024).ToString() + "";
            Texture2D TmpTexture = GenerateTmpTexture(Resources.Load<Texture2D>(texture), LeftIdx % 4, TopIdx % 4);
            NewQuad.GetComponent<MeshRenderer>().materials[0].SetTexture("_UnlitColorMap", TmpTexture);
            */
        }
        else
        {
            string texture = "Data/IPTexture/" + (lv * 4 + 16).ToString() + "-" + (LeftIdx * 1024).ToString() + "-" + (TopIdx * 1024).ToString() + "";
            
            NewQuad.GetComponent<MeshRenderer>().materials[0].SetTexture("_UnlitColorMap", Resources.Load<Texture2D>(texture));
        }
    }

    void PlaceIcons(IpDetail[] Result, IPLayerInfo LayerInfo)
    {
        return;
    }

    void UpdateIcon()
    {
        Vector2 CenterPosition = CalculatePosition();

        if (lastCenterPosition != CenterPosition)
        {
            IPProxy.instance.GetIpInfoBlock(PlaceIcons, (int)(CenterPosition.x - 8), (int)(CenterPosition.y - 8), 17);
            for (int i = -8; i < 9; i ++)
            {
                for (int j = -8; j < 9; j ++)
                {
                    Vector2 TmpPosition = new Vector2(CenterPosition.x + i, CenterPosition.y + j);

                }
            }
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
        Vector2 Position = CalculatePosition();
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
                    //UpdateIPView(3);
                    UpdateIPView(2);
                    if (Camera.transform.position.y < heightIconOn)
                    {
                        UpdateIcon();
                    }

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
        lastCenterPosition = Position;
    }
}
