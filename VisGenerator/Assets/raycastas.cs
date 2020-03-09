using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controller;
using UnityEngine.EventSystems;

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
    public WanderingASMap wanderingASMap;
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

        // #region LIPENGYUE
        //     InitTextsForNearlyAS();
        // #endregion
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
                                    if (isSelected && ((int)(hitPointT.x) == zoomX) && ((int)(hitPointT.y) == zoomY))
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
                            }
                        }
                    }

                    if(!m_HasHit)
                    {
                        UIEventDispatcher.HideIpMenuPanel();
                    }
                }
                if (false)
                {
                    for (int i = 0; i < pairs.Length / 4; i++)
                    {
                        Vector2 hitPointT1 = new Vector2(pairs[i * 2], pairs[i * 2 + 1]);
                        float height1 = Heights.GetPixel((int)(hitPointT1.x), (int)(hitPointT1.y)).r * 256.00f;
                        Vector3 hitPoint1 = new Vector3();
                        hitPoint1.x = (int)(hitPointT1.x) * 1.00f / 256 * Size.x;
                        hitPoint1.z = (int)(hitPointT1.y) * 1.00f / 256 * Size.y;
                        hitPoint1.y = height1 * HScale / 256.00f;

                        Vector2 hitPointT2 = new Vector2(pairs[i * 2 + 2], pairs[i * 2 + 3]);
                        float height2 = Heights.GetPixel((int)(hitPointT2.x), (int)(hitPointT2.y)).r * 256.00f;
                        Vector3 hitPoint2 = new Vector3();
                        hitPoint2.x = (int)(hitPointT2.x) * 1.00f / 256 * Size.x;
                        hitPoint2.z = (int)(hitPointT2.y) * 1.00f / 256 * Size.y;
                        hitPoint2.y = height2 * HScale / 256.00f;
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
            CameraController.ViewWanderingAS(zoomX, zoomY);
            //ShowWanderingMap(zoomX, zoomY, zoomHeight);
            isZooming = false;
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
            Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, cameraHitPoint, 200.0f * Time.deltaTime);
        }
        else
        {
            isZooming = false;
        }
    }
#endregion

}
