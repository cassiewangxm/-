using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controller;
using UnityEngine.EventSystems;
using System.IO;

// The raycastas is used to capture mouse clicks on View AS
// A single click on screen will be calculated to get an AS index (not accurate)
// If the index exists that AS will be selected
// A single click on a selected AS switches the view to Wandering Mode centering that AS
// Double click on screen is processed as two single clicks

public class raycastas : MonoBehaviour
{
    public Consts Consts;

    private Vector3 Position;
    private Vector2 Size;

    public Vector3 IPPosition;
    public Vector2 IPSize;

    public Texture2D Heights;
    public Texture2D Ids;
    public Texture2D Nums;
    public float HScale;
    public GameObject Cube;
    public Text Text;

    //private Plane[] planes;

    public SceneMananger SceneMananger;

    private float[] pairs;

    public Camera Camera;
    public GameObject CameraParent;
    private CameraController CameraController;
    public Filters ASFilter;
    public ASDetailPanel ASDetailPanel;

    private bool m_HasHit;

    int zoomX;
    int zoomY;
    float zoomHeight;
    Vector3 cameraHitPoint;
    bool isZooming = false;
    bool isSelected = false;

    #region LIPENGYUE
    // public GameObject textPrefab;
    // public Transform textParent;
    // private TextMesh[] nearTexts; //附近AS柱体名称
    // private bool titlesDirty; //当前 TextMesh[] nearTexts 是否有title在显示
    // private Vector3 oldCamPos;
    public WanderingASMapV2 wanderingASMap;
    #endregion

    public void ExitZooming()
    {
        isZooming = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        planes = new Plane[256];
        for (int i = 0; i < planes.Length; i ++)
        {
            planes[i] = new Plane(Vector3.up, new Vector3(0.0f, i * HScale / 256.00f, 0.0f));
        }
        */
        CameraController = Camera.GetComponent<CameraController>();

        Position = new Vector3(Consts.ASPos.x, 0.0f, Consts.ASPos.y);
        Size = Consts.ASSize;

        //NetUtil.Instance.RequestASSegmentsInfo(new MessageRequestASSegments(), null, Vector3Int.zero, null);

        //IPProxy.instance.GetIpInfoByFilter("Merlin",OnGetIPBlockBack);

        // #region LIPENGYUE
        //     InitTextsForNearlyAS();
        // #endregion

        Dictionary<Vector2Int, ASInfo> ASDict = ASProxy.instance.ASDict;

        Debug.Log("Generating AS Texture...");
        //Texture2D texas = new Texture2D(256, 256, TextureFormat.RGB24, false);

        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                //texas.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f));
                ASFilter.ASHeightsArray[i * 256 + j] = 0.0f;
            }
        }

        foreach (KeyValuePair<Vector2Int, ASInfo> entry in ASDict)
        {
            ASInfo ASInfo = entry.Value;
            int x = entry.Key.x;
            int y = entry.Key.y;
            if (ASInfo != null)
            {
                int count = ASInfo.ASSegment.Length;
                float v = Mathf.Sqrt(Mathf.Sqrt(count)) * 70.0f / 256.0f;
                //texas.SetPixel(x, y, new Color(0.0f, 0.0f, v));
                ASFilter.ASHeightsArray[x * 256 + y] = v * HScale;
            }
        }

        //texas.Apply();
        //byte[] bytesas = texas.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/AS/Data/ASTexture.png", bytesas);
    }

    void OnGetIPBlockBack(IpDetail[] a)
    {
        Debug.Log(a.Length+" ,got IPs hhhhaaaa");
        if(a.Length > 0)
            Debug.Log(a[0].IP);
    }
    int Select(float v, float textureSize)
    {
        if (v < 0.0f)
        {
            return 0;
        }
        if (v > textureSize)
        {
            return (int)textureSize;
        }
        return (int)v;
    }

    Vector2 WorldtoUVHit(Vector3 point, float textureSize, Vector3 position, Vector2 size)
    {
        Vector3 newPoint = point;
        newPoint.x = (newPoint.x - position.x) / size.x * textureSize + 0.5f;
        newPoint.z = (newPoint.z - position.z) / size.y * textureSize + 0.5f;
        return new Vector2(Select(newPoint.x, textureSize), Select(newPoint.z, textureSize));
    }
    bool IsInside(Vector3 point, Vector3 position, Vector2 size)
    {
        if (((point.x - position.x) > -0.5f) && ((point.z - position.z) > -0.5f))
        {
            if (((point.x - position.x) < size.x + 0.5f) && ((point.z - position.z) < size.y + 0.5f))
            {
                return true;
            }
        }
        return false;
    }

    void ShowInfoByAS(int x, int y)
    {
        if(wanderingASMap.InWanderingState)
        {
            ASDetailPanel.gameObject.SetActive(false);
            return;
        }
        Debug.Log("Showing AS");
        ASInfo ASInfo = ASProxy.instance.GetASByPosition(x, y);
        ASDetailPanel.SetUIData(ASInfo);
        ASDetailPanel.gameObject.SetActive(true);
    }

    void HideASInfoPanel()
    {
        Debug.Log("Hiding AS");
        ASDetailPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if (CameraController.currentView == ViewType.ViewAS)
        //if (CameraController.IsAS)
        if (SceneMananger.CurrentSceneView == SceneView.ASView)
        {
            if(wanderingASMap.InWanderingState)
            {
                //漫游状态下zooming
                if(isZooming)
                {
                    MoveCamera();
                }
            }
            else
            {
                if (isZooming)
                {
                    MoveASCamera();
                    return;
                }
                //Get AS that a mouse is on
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;
                    Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

                    for (int i = 255; i >= 0; i--)
                    {
                        Plane plane = new Plane(Vector3.up, new Vector3(0.0f, i * CameraController.GetModifiedASScale(HScale) / 256.00f, 0.0f));
                        float enter = 0.0f;
                        if (plane.Raycast(ray, out enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            if (IsInside(hitPoint, Position, Size))
                            {
                                Vector2 hitPointT = WorldtoUVHit(hitPoint, 1024.0f, Position, Size);
                                float height = Heights.GetPixel((int)(hitPointT.x), (int)(hitPointT.y)).r * 256.00f;
                                hitPointT = new Vector2(hitPointT.x / 4.0f, hitPointT.y / 4.0f);
                                if (hitPoint.y <= height * HScale / 256.00f + 0.005f)
                                {
                                    
                                    // Move the camera forward
                                    Vector3 targetBasePos = new Vector3((hitPointT.x) / 256.0f * Size.x + Position.x, 0.0f, (hitPointT.y) / 256.0f * Size.y + Position.z);
                                    Vector3 cameraPos = Camera.transform.position;
                                    Ray cameraRay = new Ray(cameraPos, Vector3.Normalize(targetBasePos - cameraPos));
                                    float cameraEnter = 0.0f;
                                    plane.Raycast(cameraRay, out cameraEnter);
                                    cameraHitPoint = cameraRay.GetPoint(cameraEnter);
                                    Debug.Log("Mouse on");
                                    if (ASProxy.instance.IsASExistInLocal((int)(hitPointT.x), (int)(hitPointT.y)))
                                    {
                                        ShowInfoByAS((int)(hitPointT.x), (int)(hitPointT.y));
                                    }
                                    else
                                    {
                                        HideASInfoPanel();
                                    }
                                }
                            }
                        }
                    }
                }
                //Detect when there is a mouse click
                if (Input.GetMouseButtonUp(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;
                    Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                    m_HasHit = false;

                    for (int i = 255; i >= 0; i--)
                    {
                        Plane plane = new Plane(Vector3.up, new Vector3(0.0f, i * CameraController.GetModifiedASScale(HScale) / 256.00f, 0.0f));
                        float enter = 0.0f;
                        if (plane.Raycast(ray, out enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            //Debug.Log(hitPoint);
                            if (IsInside(hitPoint, Position, Size))
                            {
                                Vector2 hitPointT = WorldtoUVHit(hitPoint, 1024.0f, Position, Size);
                                float height = Heights.GetPixel((int)(hitPointT.x), (int)(hitPointT.y)).r * 256.00f;
                                hitPointT = new Vector2(hitPointT.x / 4.0f, hitPointT.y / 4.0f);
                                if (hitPoint.y <= height * HScale / 256.00f + 0.005f)
                                {
                                    // hitPoint.x = (int)(hitPointT.x) * 1.00f / 256 * Size.x;
                                    // hitPoint.z = (int)(hitPointT.y) * 1.00f / 256 * Size.y;
                                    // hitPoint.y = height * HScale / 256.00f; //
                                    //Cube.transform.position = hitPoint;
                                    Color32 color = Nums.GetPixel((int)(hitPointT.x), (int)(hitPointT.y));
                                    uint num = (uint)(color.r) * (uint)(1 << 24) + (uint)(color.g) * (uint)(1 << 16) + (uint)(color.b) * (1 << 8) + (uint)(color.a);
                                    //Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y) + " = " + height + ", " + num);
                                    //Debug.Log(Input.mousePosition);
                                    // Text.transform.position = Input.mousePosition;
                                    // Text.text = "IP number: " + num.ToString();
                                    // Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                                    // UIEventDispatcher.OpenIpMenuPanel(num.ToString(), screenPos);
                                    // Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y));
                                    //Text.transform.position = Input.mousePosition;
                                    //Text.text = "IP number: " + num.ToString();
                                    //Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                                    //UIEventDispatcher.OpenIpMenuPanel(num.ToString(), screenPos);

                                    // Move the camera forward
                                    Vector3 targetBasePos = new Vector3((hitPointT.x) / 256.0f * Size.x + Position.x, 0.0f, (hitPointT.y) / 256.0f * Size.y + Position.z);
                                    Vector3 cameraPos = Camera.transform.position;
                                    Ray cameraRay = new Ray(cameraPos, Vector3.Normalize(targetBasePos - cameraPos));
                                    float cameraEnter = 0.0f;
                                    plane.Raycast(cameraRay, out cameraEnter);
                                    cameraHitPoint = cameraRay.GetPoint(cameraEnter);
                                    Debug.Log(cameraHitPoint);
                                    if (isSelected && ASProxy.instance.IsASExistInLocal(zoomX,zoomY) && ((int)(hitPointT.x) == zoomX) && ((int)(hitPointT.y) == zoomY))
                                    {
                                        isZooming = true;
                                    }
                                    else
                                    {
                                        isZooming = false;
                                        ASFilter.FilterBySelectedAS((int)(hitPointT.x), (int)(hitPointT.y));
                                    }
                                    zoomX = (int)(hitPointT.x);
                                    zoomY = (int)(hitPointT.y);
                                    zoomHeight = height;
                                    isSelected = true;

                                    //PPPS: 这里可以显示单独柱体
                                    m_HasHit = true;
                                    break;
                                }
                            }
                            else
                            {
                                isSelected = false;
                                ASFilter.FilterBySelectedAS(-1, -1);
                            }
                        }
                    }

                    if(!m_HasHit)
                    {
                        UIEventDispatcher.HideIpMenuPanel();
                    }
                }
            }
            

            //PPPS: 显示附近AS柱体名称
            //DetectNearAS();
        }
        else if(SceneMananger.CurrentSceneView == SceneView.IPView)
        {
            //Detect when there is a mouse click
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                Plane plane = new Plane(Vector3.up, Vector3.zero);

                float enter = 0.0f;
                if (plane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    if (IsInside(hitPoint, IPPosition, IPSize))
                    {
                        Vector2 hitPointT = WorldtoUVHit(hitPoint, 1024.0f, IPPosition, IPSize);
                        Color idColor = Ids.GetPixel((int)(hitPointT.x), (int)(hitPointT.y));
                        float id = idColor.r * 256.00f + idColor.g;
                        Text.transform.position = Input.mousePosition;
                        Text.text = "AS number: " + ((int)id).ToString();
                        Debug.Log((int)(hitPointT.x) + ", " +(int)(hitPointT.y));
                    }
                }
            }
            HideASInfoPanel();
        }
        else
        {
            HideASInfoPanel();
        }
    }

    void MoveASCamera()
    {
        if (Vector3.Distance(Camera.transform.position, cameraHitPoint) >= 1.0f)
        {
            Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, cameraHitPoint, 200.0f * Time.deltaTime);
        }
        else
        {
            HideASInfoPanel();
            CameraController.ViewWanderingAS(zoomX, zoomY);
            //ShowWanderingMap(zoomX, zoomY, zoomHeight);
            isZooming = false;
        }
    }

    long xy2d(int n, int x, int y)
    {
        long d = 0;
        int s = n / 2;
        int rx = 0;
        int ry = 0;
        while (s > 0)
        {
            rx = (x & s) > 0 ? 1 : 0;
            ry = (y & s) > 0 ? 1 : 0;
            d += s * s * ((3 * rx) ^ ry);
            rot(s, ref x, ref y, rx, ry);
            s /= 2;
        }
        return d;
    }

    void rot(int n, ref int x, ref int y, int rx, int ry)
    {
        if (ry == 0)
        {
            if (rx == 1)
            {
                x = n - 1 - x;
                y = n - 1 - y;
            }
            int t = x;
            x = y;
            y = t;
        }
    }

    #region LIPENGYUE
    // void ShowWanderingMap(int x, int y, float height)
    // {
    //     wanderingASMap.IntoWanderingMap(x,y);
    //     CameraController.ViewWanderingAS(x,y);
    // }

    public void FocusCamera(Vector3 target)
    {
        ExitZooming();
        cameraHitPoint = target;
        isZooming = true;
    }

    void MoveCamera()
    {
        if (Vector3.Distance(Camera.transform.position, cameraHitPoint) >= 1.0f)
        {
            Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, cameraHitPoint, 60.0f * Time.deltaTime);
        }
        else
        {
            isZooming = false;
        }
    }
#endregion

}
