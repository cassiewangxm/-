using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controller;
using UnityEngine.EventSystems;

public class raycastas : MonoBehaviour
{

    public Vector3 Position;
    public Vector2 Size;

    public Vector3 IPPosition;
    public Vector2 IPSize;

    public Texture2D Heights;
    public Texture2D Ids;
    public Texture2D Nums;
    public float HScale;
    public GameObject Cube;
    public Text Text;

    private Plane[] planes;

    private float[] pairs;

    public Camera Camera;
    private CameraController CameraController;

    private bool m_HasHit;

    #region LIPENGYUE
    // public GameObject textPrefab;
    // public Transform textParent;
    // private TextMesh[] nearTexts; //附近AS柱体名称
    // private bool titlesDirty; //当前 TextMesh[] nearTexts 是否有title在显示
    // private Vector3 oldCamPos;
    public WanderingASMap wanderingASMap;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        planes = new Plane[256];
        for (int i = 0; i < planes.Length; i ++)
        {
            planes[i] = new Plane(Vector3.up, new Vector3(0.0f, i * HScale / 256.00f, 0.0f));
        }
        CameraController = Camera.GetComponent<CameraController>();

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
        if (CameraController.IsAS)
        {
            //Detect when there is a mouse click
            if (Input.GetMouseButton(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                m_HasHit = false;

                for (int i = planes.Length - 1; i >= 0; i--)
                {
                    float enter = 0.0f;
                    if (planes[i].Raycast(ray, out enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        Debug.Log(hitPoint);
                        if (IsInside(hitPoint, Position, Size))
                        {
                            Vector2 hitPointT = WorldtoUVHit(hitPoint, 256.0f, Position, Size);
                            float height = Heights.GetPixel((int)(hitPointT.x), (int)(hitPointT.y)).r * 256.00f;
                            if (hitPoint.y <= height * HScale / 256.00f + 0.005f)
                            {
                                // hitPoint.x = (int)(hitPointT.x) * 1.00f / 256 * Size.x;
                                // hitPoint.z = (int)(hitPointT.y) * 1.00f / 256 * Size.y;
                                // hitPoint.y = height * HScale / 256.00f; //
                                //Cube.transform.position = hitPoint;
                                //Color32 color = Nums.GetPixel((int)(hitPointT.x), (int)(hitPointT.y));
                                //uint num = (uint)(color.r) * (uint)(1 << 24) + (uint)(color.g) * (uint)(1 << 16) + (uint)(color.b) * (1 << 8) + (uint)(color.a);
                                //Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y) + " = " + height + ", " + num);
                                //Debug.Log(Input.mousePosition);
                                // Text.transform.position = Input.mousePosition;
                                // Text.text = "IP number: " + num.ToString();
                                // Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                                // UIEventDispatcher.OpenIpMenuPanel(num.ToString(), screenPos);
                                // Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y));
                                Text.transform.position = Input.mousePosition;
                                Text.text = "IP number: " + num.ToString();
                                Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                                //UIEventDispatcher.OpenIpMenuPanel(num.ToString(), screenPos);
                                Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y));
                                //PPPS: 这里可以显示单独柱体
                                ShowSingleAS((int)(hitPointT.x), (int)(hitPointT.y), height);
                                m_HasHit = true;
                                break;
                            }
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

            //PPPS: 显示附近AS柱体名称
            //DetectNearAS();
        }
        else if(CameraController.currentView == ViewType.ViewIP)
        {
            //Detect when there is a mouse click
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float enter = 0.0f;
                if (planes[0].Raycast(ray, out enter))
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
#region LIPENGYUE
    void ShowSingleAS(int x, int y, float height)
    {
        Debug.Log("!");
        wanderingASMap.IntoWanderingMap(x,y);
        CameraController.ViewSingleAS();
    }
    // void InitTextsForNearlyAS()
    // {
    //     oldCamPos = Camera.transform.position;
    //     nearTexts = new TextMesh[100];
    //     for (int i = 0; i < nearTexts.Length; i++)
    //     {
    //         nearTexts[i] = Instantiate(textPrefab,textParent).GetComponent<TextMesh>();
    //     }
    // }
    float Select2(float v, float textureSize)
    {
        if (v < 0.0f)
        {
            return 0;
        }
        if (v > textureSize)
        {
            return (float)textureSize;
        }
        return (float)v;
    }
    Vector2 WorldtoUVHit2(Vector3 point, float textureSize, Vector3 position, Vector2 size)
    {
        Vector3 newPoint = point;
        newPoint.x = (newPoint.x - position.x) / size.x * textureSize ;
        newPoint.z = (newPoint.z - position.z) / size.y * textureSize ;
        return new Vector2(Select2(newPoint.x, textureSize), Select2(newPoint.z, textureSize));
    }
    // void DetectNearAS()
    // {
    //     //相机超出范围后 清空 
    //     if(Camera.transform.position.y > 70)
    //     {
    //         ClearASTitles();
    //         return;
    //     }    

    //         float camMovD = Vector3.Distance(Camera.transform.position, oldCamPos);
    //         if(camMovD > 2)
    //         {
    //             oldCamPos = Camera.transform.position;

    //             // world space position 视野中心点
    //             Vector3 camPos = Camera.transform.position + Camera.transform.forward * Mathf.Abs(Camera.transform.position.y/Camera.transform.forward.y);
    //             Vector3 bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, Vector3.Distance(Camera.transform.position,camPos)));
                
                
    //             int viewWidth = 10;
    //             int viewDepth = -(int)((camPos.z - bottomBorder.z)/2.5f);

    //             //矫正位置
    //             camPos.x = ((int)(camPos.x/2.5f) - 0.5f)*2.5f;
    //             camPos.z = ((int)(camPos.z/2.5f) - 0.5f)*2.5f;
    //             if(IsInside(camPos, Position, Size))
    //             {
    //                 titlesDirty = true;
    //                 int usedText = 0;
    //                 for (int i = 0; i<viewWidth*3; i++)
    //                 {
    //                     for (int j = viewDepth; ; j--)
    //                     {
    //                         Vector3 hitPoint = new Vector3(camPos.x + i*2.5f, 0, camPos.z + j*2.5f);
    //                             if(IsInside(hitPoint, Position, Size))
    //                             {
    //                                 Vector2 hitPointT = WorldtoUVHit2(hitPoint, 1.0f, Position, Size);
    //                                 float height = (Heights.GetPixelBilinear((hitPointT.x), (hitPointT.y)).r * 0.3f + 0.01f)*200.0f;
    //                                 if (height > 0 )
    //                                 {
    //                                         hitPoint.y = height;
    //                                         if(!IsInViewport(hitPoint))
    //                                             continue;

    //                                         // PPPS : 这里可以调整距离范围
    //                                         if(Vector3.Distance(hitPoint,Camera.transform.position) > 40)
    //                                             continue;

    //                                         if(usedText < nearTexts.Length)
    //                                         {
    //                                             nearTexts[usedText].transform.position = hitPoint;
    //                                             nearTexts[usedText].text = hitPoint.ToString();
    //                                             usedText++;
    //                                         }
    //                                         else
    //                                         {
    //                                             break;
    //                                         }
    //                                 }
    //                             }
    //                             else
    //                             {
    //                                 break;
    //                             }
    //                     }
    //                 }

    //                 for (int i = usedText; i < nearTexts.Length; i++)
    //                 {
    //                     nearTexts[i].text = "";
    //                 }
    //             }
    //         }
    // }

    bool IsInViewport(Vector3 v)
    {
        Vector3 vv = Camera.WorldToViewportPoint(v);
        if(vv.x >0 && vv.x<1 && vv.y >0 && vv.y<1)
            return true;
        
        return false;
    }

    // void ClearASTitles()
    // {
    //     if(!titlesDirty)
    //         return;

    //     for (int i = 0; i < nearTexts.Length; i++)
    //     {
    //         nearTexts[i].text = "";
    //     }
    //     titlesDirty = false;
    // }
#endregion

}
