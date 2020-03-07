using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;
using TMPro;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.EventSystems;

/*
1. 确保 raycastas.cs 中的 Heights贴图与AS地图中用的图片一致
2. 
*/

public class SingleAS : MonoBehaviour
{
    public Transform m_RegmentsRoot;//quad的 根结点
    //public GameObject m_QuadPrefab; // 用于划分区段
    //public SpriteRenderer m_SegmentIPMap; //区段IP图
    public MeshFilter m_mesh; //模拟柱体mesh
    public Transform m_simpleLook;
    public TextMeshPro m_ASName;
    public GameObject m_TestSeletedSegment;
    public BoxCollider m_boxCollider;

    public ASInfo ASData
    {
        get {return m_ASData;}
    }
    public float Height
    {
        get {return m_height;}
    }
    public WanderingASMap WanderingASMap
    {
        set {m_wanderMap = value;}
    }
    private WanderingASMap m_wanderMap;
    private List<ASSegmentItem> m_SegmentList = new List<ASSegmentItem>(); //用于显示IP区段时间等信息
    private ASInfo m_ASData; //AS柱数据
   //private Texture2D m_tempTexture;    //区段IP图
    //private int m_TextureW = 256; //区段IP图宽度,可容纳256*256个ip
    //private int m_curSegment;   //当前选中的区段序号
    private Color m_colorSelected = new Color(0, 167.0f/255, 246.0f/255, 150.0f/255);
    private Color m_colorUnSelected = new Color(248.0f/255, 194.0f/255, 18.0f/255, 150.0f/255);
    private float m_height;
    private bool m_isFocused;
    private float m_focusTime;
    private float m_maxSegmentWidth;
    private bool m_isVisibleInCam;
    private Vector3 m_oldCamPos;
    List<float> m_radiusList = new List<float>();
    List<float> m_heightList = new List<float>();

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void OnBecameVisible()
    {
        if(!Application.isPlaying)
            return;
        m_isVisibleInCam = true;
        InitASLooking();
    }

    void OnBecameInvisible()
    {
        m_isVisibleInCam = false;
    }

    public void InitASData(ASInfo asData)
    {
        m_TestSeletedSegment.SetActive(false);
        m_simpleLook.gameObject.SetActive(false);
        m_mesh.gameObject.SetActive(false);

        m_ASData = asData;
        
        if(m_ASData != null)
            m_height = m_ASData.Height;
        else
            return;
        
        //生成每一层
        m_maxSegmentWidth = InitSegmentsSimple();
        m_boxCollider.enabled = true;
        m_boxCollider.center = Vector3.zero;
        m_boxCollider.size = new Vector3(m_maxSegmentWidth, m_height, m_maxSegmentWidth); 
    }
    public void InitASLooking()
    {
        float d;
        if(CheckDistance(out d,5))
        {
            m_ASName.text = transform.name;
            m_ASName.rectTransform.localPosition = new Vector3(0, m_height/2 + 1, 0);

            if(m_wanderMap.m_targetCamera != null)
            {
                m_ASName.transform.parent.LookAt(m_wanderMap.m_targetCamera.transform);
                m_ASName.transform.parent.localEulerAngles = new Vector3(0, m_ASName.transform.parent.localEulerAngles.y,m_ASName.transform.parent.localEulerAngles.z);
            }
        }
        else
        {
            m_ASName.text = "";
        }

        //生成外围不规则mesh
        InitMesh();

        m_oldCamPos = m_wanderMap.m_targetCamera.transform.position;
    }

    public void RefreshAS(ASInfo data)
    {
        InitASData(data);

        InitASLooking();
    }

    public void SetSelected(bool value)
    {
        StopAllCoroutines();
        StartCoroutine(InitSegments(value));

        m_boxCollider.enabled = !value;

        m_TestSeletedSegment.SetActive(false);

        m_isFocused = value;
        m_focusTime = Time.time;

        m_mesh.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", value ? m_colorSelected : m_colorUnSelected);
        m_simpleLook.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", value ? m_colorSelected : m_colorUnSelected);
        
    }

    // true : nearly enough
    bool CheckDistance(out float distance, int multip = 10)
    {
        Vector2 b = new Vector2(transform.position.x, transform.position.z);
        Vector2 a = new Vector2(m_wanderMap.m_targetCamera.transform.position.x,m_wanderMap.m_targetCamera.transform.position.z);
        distance = Vector2.Distance(a,b);
        if(distance < m_wanderMap.BaseCellWidth * multip)
        {
            return true;
        }
        return false;
    }

    float InitSegmentsSimple()
    {
        float maxWith = 0;
        m_radiusList.Clear();
        m_heightList.Clear();
        for(int i = 0; i < m_ASData.ASSegment.Length; i++)
        {
            m_radiusList.Add(m_ASData.ASSegment[i].GetRadius());
            m_heightList.Add(i*2 - m_height/2);
            maxWith = Mathf.Max(maxWith, m_radiusList[i]);
        }

        return maxWith + 0.1f;
    }
    IEnumerator InitSegments(bool select)
    {
        for(int i = 0; i < m_SegmentList.Count; i++)
        {
            SegmentPool.Instance.ReturnBackSegment(m_SegmentList[i]);
        }
        m_SegmentList.Clear();

        if(select)
        {
            for(int i = 0; i < m_ASData.ASSegment.Length; i++)
            {
                ASSegmentItem seg =  SegmentPool.Instance.GetSegment();
                seg.transform.SetParent(m_RegmentsRoot);
                seg.transform.localPosition = new Vector3(0, i*2 - m_height/2, 0);
                seg.SetSegment(m_ASData.ASSegment[i]);
                seg.transform.name = m_SegmentList.Count.ToString();
                seg.SetText(select);
                seg.SetIPMap(select);
                seg.SetCollider(select);

                m_SegmentList.Add(seg);

                yield return null;
                //yield return new WaitForEndOfFrame();
            }
        }
        
    }

    void InitMesh()
    {
        if(m_ASData != null )
        {
            float distance ;
            bool isnear = CheckDistance(out distance);
            if(isnear && m_ASData.ASSegment.Length > 1)
            {
                GenerateMeshJob(m_ASData.ASSegment.Length);//GenerateMesh(m_SegmentList,m_ASData.Segments.Length);
                
                m_simpleLook.gameObject.SetActive(false);
                m_mesh.gameObject.SetActive(true);
            }
            else
            {
                m_mesh.gameObject.SetActive(false);

                Color color = m_isFocused ? m_colorSelected : m_colorUnSelected;
                if(!isnear)//distance > m_wanderMap.BaseCellWidth * 5)
                {
                    float weak = (m_wanderMap.BaseCellWidth*15-  distance)/(m_wanderMap.BaseCellWidth*5);
                    weak = weak < 0 ? 0 : (weak > 1 ? 1 : weak);
                    weak = 0.5f + weak*0.5f;
                    //Debug.Log( distance+" , "+weak);
                    m_simpleLook.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", new Vector4(color.r*weak, color.g*weak, color.b*weak, color.a*weak));
                }
                else
                {
                    m_simpleLook.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", color);
                }    
                m_simpleLook.localScale = new Vector3(m_maxSegmentWidth, m_height, m_maxSegmentWidth);
                m_simpleLook.gameObject.SetActive(true);
            }
        }
        else
        {
            m_simpleLook.gameObject.SetActive(false);
            m_mesh.gameObject.SetActive(false);
        }
    }

    Mesh GenerateMeshJob(int n)
    {
        int quadN = 4*n;
        var vect = new NativeArray<Vector3>(quadN*4, Allocator.Persistent);
        var uvss = new NativeArray<Vector2>(quadN*4, Allocator.Persistent);
        var triangles = new NativeArray<int>(quadN*6, Allocator.Persistent);
        var scls = new NativeArray<float>(n, Allocator.Persistent);
        var poss = new NativeArray<float>(n, Allocator.Persistent);
        for(int i = 0; i < n; i++)
        {
            scls[i] = m_radiusList[i];
            poss[i] = m_heightList[i];
        }

        var job = new MeshGenerater.MeshJob()
        {
            deltaTime = Time.deltaTime,
            vertices = vect,
            uvs = uvss,
            tris = triangles,
            scales = scls,
            positions = poss
        };

        JobHandle jobHandle = job.Schedule();
        jobHandle.Complete();

        Mesh mesh = m_mesh.mesh;//new Mesh();
        mesh.Clear();
        mesh.vertices = vect.ToArray();
        mesh.uv = uvss.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        

        vect.Dispose();
        uvss.Dispose();
        triangles.Dispose();
        scls.Dispose();
        poss.Dispose();

        return mesh;
    }

    

    //暂时去掉对柱体的点击
    void Update()
    {
        if(m_isFocused && (Time.time - m_focusTime) > Time.deltaTime && m_wanderMap.m_camController != null && m_wanderMap.m_camController.currentView == ViewType.ViewWanderingAS)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = m_wanderMap.m_targetCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray,out hitInfo))
                {
                    Vector3 v = hitInfo.point - hitInfo.transform.position;
                    Debug.Log(v+ " , Hit : ----- " + hitInfo.transform.name);
                    
                    OnClickIp(hitInfo.transform.name, v);
                }
            }
        }

        if(m_isVisibleInCam && m_simpleLook.gameObject.activeSelf)
        {
            float d;
            float distance = Vector3.Distance(m_wanderMap.m_targetCamera.transform.position, m_oldCamPos);
            if(distance > m_wanderMap.BaseCellWidth * 4 && CheckDistance(out d))
            {
                InitASLooking();
            }
        }
    }

    void OnClickIp(string segName, Vector2 pos)
    {
        if(string.IsNullOrEmpty(segName))
            return;

        int segIndex = 0;
        if(!int.TryParse(segName,out segIndex))
            return;

        if(segIndex < m_SegmentList.Count)
        {
            Vector3 targetPos = m_SegmentList[segIndex].transform.position - m_wanderMap.m_targetCamera.transform.forward * 8; 
            m_wanderMap.m_camController.raycastas.FocusCamera(targetPos);

            int ipIndex = m_SegmentList[segIndex].GetIPIndexByPos(pos);
            ASProxy.instance.GetIpInfoFromAS(new Vector2(m_ASData.X,m_ASData.Y), segIndex, ipIndex, ShowIPDetail);
        }
    }

    void ShowIPDetail(IpDetail ipDetail)
    {
        UIEventDispatcher.OpenIPDetailPanel(ipDetail.IP);
    }

    // Mesh GenerateMesh(List<ASSegmentItem> list, int n)
    // {
    //     int quadN = 4*n-1;
    //     Vector3[] vect = new Vector3[quadN*4];
    //     Vector2[] uvs = new Vector2[quadN*4];
    //     int[] tris = {0,3,2,2,1,0};//{ 0, 1, 2, 2, 3, 0 };
    //     List<Vector3> vertices = new List<Vector3>();
    //     int count = 0;
    //     int trisCount = 0;
    //     int[] triangles = new int[quadN*6];
    //     for(int i =0; i< n - 1;)
    //     {
    //         if(i+1 == n)
    //             break;

    //         float radius = m_radiusList[i]/2;
    //         float radius2 = m_radiusList[i+1]/2;
    //         float upY = m_heightList[i];
    //         float downY = m_heightList[i+1];

    //         //front
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(-radius,upY,-radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(radius,upY,-radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(radius2,downY,-radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(-radius2,downY,-radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //right 
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(radius,upY,-radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(radius,upY,radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(radius2,downY,radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(radius2,downY,-radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //back
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(radius,upY,radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(-radius,upY,radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(-radius2,downY,radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(radius2,downY,radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //left
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(-radius,upY,radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(-radius,upY,-radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(-radius2,downY,-radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(-radius2,downY,radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //top
    //             if(i == n - 2)
    //             {
    //                 uvs[count] = new Vector2(0,0);
    //                 vertices[count++] = new Vector3(-radius2,downY,radius2);
    //                 uvs[count] = new Vector2(0,1);
    //                 vertices[count++] = new Vector3(-radius2,downY,-radius2);
                
    //                 uvs[count] = new Vector2(1,1);
    //                 vertices[count++] = new Vector3(radius2,downY,-radius2);
    //                 uvs[count] = new Vector2(1,0);
    //                 vertices[count++] = new Vector3(radius2,downY,radius2);

    //                 for(int j =0; j<6; j++)
    //                 {
    //                     triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //                 }
    //             }
            
    //         i++;
    //     }

    //     Mesh mesh = new Mesh();
    //     mesh.vertices = vect;
    //     mesh.uv = uvs;
    //     mesh.triangles = triangles;
    //     mesh.RecalculateNormals();

    //     return mesh;
    // }
}
