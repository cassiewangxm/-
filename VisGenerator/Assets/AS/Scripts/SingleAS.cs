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
    public GameObject m_QuadPrefab; // 用于划分区段
    //public SpriteRenderer m_SegmentIPMap; //区段IP图
    public MeshFilter m_mesh; //模拟柱体mesh
    public Transform m_simpleLook;
    public TextMeshPro m_ASName;
    public GameObject m_TestSeletedSegment;
    public BoxCollider m_boxCollider;

    public ASDetail ASData
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
    private ASDetail m_ASData; //AS柱数据
   //private Texture2D m_tempTexture;    //区段IP图
    private int m_TextureW = 256; //区段IP图宽度,可容纳256*256个ip
    private int m_curSegment;   //当前选中的区段序号
    private Color m_colorSelected = new Color(0, 167.0f/255, 246.0f/255, 100.0f/255);
    private Color m_colorUnSelected = new Color(250.0f/255, 184.0f/255, 6.0f/255, 100.0f/255);
    private float m_height;
    private bool m_isFocused;
    private float m_maxSegmentWidth;
    private bool m_isVisibleInCam;
    private Vector3 m_oldCamPos;

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void OnBecameVisible()
    {
        if(!Application.isPlaying)
            return;
        m_isVisibleInCam = true;
        InitASLooking(true);
    }

    void OnBecameInvisible()
    {
        m_isVisibleInCam = false;
    }

    public void InitASData(int x, int y, float height)
    {
        m_height = height;

        ASProxy.instance.GetASByPosition(x,y,height,out m_ASData);
        m_TestSeletedSegment.SetActive(false);

        //生成每一层
        m_maxSegmentWidth = InitSegments();
        m_boxCollider.enabled = true;
        m_boxCollider.center = Vector3.zero;
        m_boxCollider.size = new Vector3(m_maxSegmentWidth, m_height, m_maxSegmentWidth); 
    }
    public void InitASLooking(bool job = false)
    {
        m_ASName.text = transform.name;
        m_ASName.rectTransform.localPosition = new Vector3(0, m_height/2 + 2, 0);

        if(m_wanderMap.m_targetCamera != null)
        {
            m_ASName.transform.parent.LookAt(m_wanderMap.m_targetCamera.transform);
            m_ASName.transform.parent.localEulerAngles = new Vector3(0, m_ASName.transform.parent.localEulerAngles.y,m_ASName.transform.parent.localEulerAngles.z);
        }

        //生成外围不规则mesh
        InitMesh(job);

        m_oldCamPos = m_wanderMap.m_targetCamera.transform.position;
    }

    public void RefreshAS(int x, int y, float height)
    {
        InitASData(x, y, height);

        InitASLooking(true);
    }

    public void SetSelected(bool value)
    {
        m_boxCollider.enabled = !value;

        m_TestSeletedSegment.SetActive(false);
        m_isFocused = value;
        m_mesh.GetComponent<MeshRenderer>().material.SetVector("_BaseColor", value ? m_colorSelected : m_colorUnSelected);
        
        StopAllCoroutines();
        StartCoroutine(SetSelectedByStep(value));
    }

    IEnumerator SetSelectedByStep(bool value)
    {
        for(int i = 0; i < m_SegmentList.Count; i++)
        {
            m_SegmentList[i].SetText(value);
            m_SegmentList[i].SetIPMap(value);
            m_SegmentList[i].SetCollider(value);

            if(value)
                yield return new WaitForEndOfFrame();
        }
    }

    // true : nearly enough
    bool CheckDistance()
    {
        Vector2 b = new Vector2(transform.position.x, transform.position.z);
        Vector2 a = new Vector2(m_wanderMap.m_targetCamera.transform.position.x,m_wanderMap.m_targetCamera.transform.position.z);
        if(Vector2.Distance(a,b) < m_maxSegmentWidth * 20)
        //if(Vector2Int.Distance(m_wanderMap.CurSelectedLct, m_ASData.Location) < 9)
        {
            return true;
        }
        return false;
    }

    float InitSegments()
    {
        float maxWith = 0;
        int count = Mathf.Max(m_ASData.Segments.Length, m_SegmentList.Count);
        for(int i = 0; i < count; i++)
        {
            if(i < m_ASData.Segments.Length)
            {
                if(i < m_SegmentList.Count)
                {
                    m_SegmentList[i].SetSegment(m_ASData.Segments[i]);
                    m_SegmentList[i].transform.localPosition = new Vector3(0, i + 0 - m_ASData.Segments.Length/2, 0);
                }
                else
                {
                    ASSegmentItem seg =  Instantiate(m_QuadPrefab, m_RegmentsRoot).GetComponent<ASSegmentItem>();
                    seg.transform.localPosition = new Vector3(0, i + 0 - m_ASData.Segments.Length/2, 0);
                    seg.SetSegment(m_ASData.Segments[i]);
                    seg.transform.name = m_SegmentList.Count.ToString();
                    
                    m_SegmentList.Add(seg);
                }
                maxWith = Mathf.Max(maxWith, m_SegmentList[i].transform.localScale.x);
            }
            else if(i < m_SegmentList.Count)
            {
                m_SegmentList[i].HideSelf();
            }
        }

        return maxWith;
    }

    void InitMesh(bool usejob)
    {
        if(m_ASData != null && m_ASData.Segments.Length > 1)
        {
            if(CheckDistance())
            {
                if(usejob)
                    m_mesh.mesh = GenerateMeshJob(m_ASData.Segments.Length);//GenerateMesh(m_SegmentList,m_ASData.Segments.Length);
                else
                    m_mesh.mesh = GenerateMesh(m_SegmentList,m_ASData.Segments.Length);

                m_simpleLook.gameObject.SetActive(false);
                m_mesh.gameObject.SetActive(true);
            }
            else
            {
                m_simpleLook.localScale = new Vector3(4,m_ASData.Segments.Length, m_maxSegmentWidth);
                m_simpleLook.gameObject.SetActive(true);
                m_mesh.gameObject.SetActive(false);
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
        int quadN = 4*n-1;
        var vect = new NativeArray<Vector3>(quadN*4, Allocator.Persistent);
        var uvss = new NativeArray<Vector2>(quadN*4, Allocator.Persistent);
        var triangles = new NativeArray<int>(quadN*6, Allocator.Persistent);
        var scls = new NativeArray<Vector3>(n, Allocator.Persistent);
        var poss = new NativeArray<Vector3>(n, Allocator.Persistent);
        for(int i = 0; i < n; i++)
        {
            scls[i] = m_SegmentList[i].transform.localScale;
            poss[i] = m_SegmentList[i].transform.localPosition;
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

        Mesh mesh = new Mesh();
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
        if(m_isFocused && m_wanderMap.m_camController != null && m_wanderMap.m_camController.currentView == ViewType.ViewSingleAS)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = m_wanderMap.m_targetCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray,out hitInfo))
                {
                    ShowIPMap(hitInfo.transform.name);
                }
            }
        }

        if(m_isVisibleInCam && m_simpleLook.gameObject.activeSelf)
        {
            float distance = Vector3.Distance(m_wanderMap.m_targetCamera.transform.position, m_oldCamPos);
            if(distance > m_maxSegmentWidth*4 && CheckDistance())
            {
                m_oldCamPos = m_wanderMap.m_targetCamera.transform.position;
                InitMesh(true);
            }
        }
    }

    void ShowIPMap(string name)
    {
        if(string.IsNullOrEmpty(name))
            return;

        int index = 0;
        if(!int.TryParse(name,out index))
            return;

        //Color[] colors = {Color.red,Color.green,Color.yellow,Color.blue};
        //m_SegmentIPMap.gameObject.SetActive(true);
        if(index < m_SegmentList.Count)
        {
            m_curSegment = index;
            if(!m_TestSeletedSegment.activeInHierarchy)
                m_TestSeletedSegment.SetActive(true);
            m_TestSeletedSegment.transform.position = new Vector3(m_TestSeletedSegment.transform.position.x,m_SegmentList[index].transform.position.y - 1,m_TestSeletedSegment.transform.position.z);
       
            // int pixelsize, lineCount;
            // GetTextureAreaInfo(index,out pixelsize,out lineCount);

            // if(m_tempTexture == null)
            // {
            //     m_tempTexture = new Texture2D(m_TextureW,m_TextureW);
            //     m_SegmentIPMap.sprite =  Sprite.Create(m_tempTexture,new Rect(0,0,m_tempTexture.width,m_tempTexture.height),new Vector2(0,0));
            // }
            // int curCount = 0;
            // for(int i = 0; i < m_TextureW; i++)
            // {  
            //     for(int j = 0; j < m_TextureW; j++)
            //     {
            //         if(j/pixelsize >= lineCount)
            //         {
            //             m_tempTexture.SetPixel(i, j,Color.black);
            //         }
            //         else
            //         {
            //             curCount = i/pixelsize * lineCount + j/pixelsize;
            //             m_tempTexture.SetPixel(j, i, m_SegmentList[index].m_SegemntData.GetIPColor(curCount));// colors[(i/pixelsize + j/pixelsize)%4]);
            //         }
            //     }
            // }
            
           // Debug.Log(pixelsize+","+lineCount);
            
            //m_tempTexture.Apply();

            //m_SegmentIPMap.transform.position = new Vector3(m_SegmentIPMap.transform.position.x,m_SegmentList[index].transform.position.y - 1,m_SegmentIPMap.transform.position.z);
        }
    }

    // void GetTextureAreaInfo(int index, out int pixelsize, out int line)
    // {
    //     pixelsize = (int)Mathf.Sqrt((m_TextureW * m_TextureW) / (int)m_SegmentList[index].m_SegemntData.IPCount) ;
    //     pixelsize = pixelsize > 0 ? pixelsize : 1;
    //     line = m_TextureW/pixelsize + 1;
    // }

    Mesh GenerateMesh(List<ASSegmentItem> list, int n)
    {
        int quadN = 4*n-1;
        Vector3[] vect = new Vector3[quadN*4];
        Vector2[] uvs = new Vector2[quadN*4];
        int[] tris = {0,3,2,2,1,0};//{ 0, 1, 2, 2, 3, 0 };
        List<Vector3> vertices = new List<Vector3>();
        int count = 0;
        int trisCount = 0;
        int[] triangles = new int[quadN*6];
        for(int i =0; i< n - 1;)
        {
            if(i+1 == n)
                break;

            float radius = list[i].transform.localScale.x/2;
            float radius2 = list[i+1].transform.localScale.x/2;
            float upY = list[i].transform.localPosition.y;
            float downY = list[i+1].transform.localPosition.y;

            //front
            {
                uvs[count] = new Vector2(0,0);
                vect[count++] = new Vector3(-radius,upY,-radius);
                uvs[count] = new Vector2(0,1);
                vect[count++] = new Vector3(radius,upY,-radius);
            }
            {
                uvs[count] = new Vector2(1,1);
                vect[count++] = new Vector3(radius2,downY,-radius2);
                uvs[count] = new Vector2(1,0);
                vect[count++] = new Vector3(-radius2,downY,-radius2);
            }

            for(int j =0; j<6; j++)
            {
                triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
            }

            //right 
            {
                uvs[count] = new Vector2(0,0);
                vect[count++] = new Vector3(radius,upY,-radius);
                uvs[count] = new Vector2(0,1);
                vect[count++] = new Vector3(radius,upY,radius);
            }
            {
                uvs[count] = new Vector2(1,1);
                vect[count++] = new Vector3(radius2,downY,radius2);
                uvs[count] = new Vector2(1,0);
                vect[count++] = new Vector3(radius2,downY,-radius2);
            }

            for(int j =0; j<6; j++)
            {
                triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
            }

            //back
            {
                uvs[count] = new Vector2(0,0);
                vect[count++] = new Vector3(radius,upY,radius);
                uvs[count] = new Vector2(0,1);
                vect[count++] = new Vector3(-radius,upY,radius);
            }
            {
                uvs[count] = new Vector2(1,1);
                vect[count++] = new Vector3(-radius2,downY,radius2);
                uvs[count] = new Vector2(1,0);
                vect[count++] = new Vector3(radius2,downY,radius2);
            }

            for(int j =0; j<6; j++)
            {
                triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
            }

            //left
            {
                uvs[count] = new Vector2(0,0);
                vect[count++] = new Vector3(-radius,upY,radius);
                uvs[count] = new Vector2(0,1);
                vect[count++] = new Vector3(-radius,upY,-radius);
            }
            {
                uvs[count] = new Vector2(1,1);
                vect[count++] = new Vector3(-radius2,downY,-radius2);
                uvs[count] = new Vector2(1,0);
                vect[count++] = new Vector3(-radius2,downY,radius2);
            }

            for(int j =0; j<6; j++)
            {
                triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
            }
            
            i++;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vect;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
