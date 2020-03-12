using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;
using UnityEngine.EventSystems;
using System;

public class WanderingASMap : MonoBehaviour
{
    public Texture2D m_Heights;
    public GameObject m_SingleASPrefab;
    public CameraController m_camController;
    public Camera m_targetCamera;
    public Transform m_root;
    public bool InWanderingState
    {
        get; set;
    }

    public float BaseCellWidth
    {
        get {return m_baseWith;}
    }

    private float m_baseWith = 10;//640.0f / 256 * 6;
    private int m_mapWidth = 256;
    private SingleAS[] m_ASArraySingle;
    private Dictionary<Vector2Int, SingleAS> m_ASArrayDict = new Dictionary<Vector2Int, SingleAS>();
    private Vector2Int m_curSelected;
    private const int m_lineCount = 24;
    private int m_cacheCount = m_lineCount * m_lineCount; //缓存n个
    private Vector3 m_oldCamPos;
    private int m_MLR = 0; // most Left Row
    private int m_MDL = 0; // most Down Line
    private bool m_initFinished = false;
    private Vector2Int m_curSelectedASLocation;

    void Awake()
    {
        EventManager.RegistEvent(EventDefine.OnRecieveSearchResult, (Action)OnRecieveSearchResult);
        SegmentPool.Instance.Prepare();
        m_oldCamPos = m_targetCamera.transform.position;
        ASProxy.instance.GetASInfoOriginal(null);
        StartCoroutine(CreateASCubes());
    }
    void OnViewChange()
    {
        switch (SceneMananger.Instance.CurrentSceneView)
        {
            case SceneView.MapView:
            case SceneView.IPView:
                InWanderingState = false;
                break;
        }
    }
    void OnRecieveSearchResult()
    {
        IpDetail[] results = IPProxy.instance.GetSearchResult();

        //TODO 
    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    public void IntoWanderingMap(int x, int y)
    {
        InWanderingState = true;

        //取消上次focus的柱子
        if(m_curSelected != Vector2Int.zero && m_ASArrayDict.ContainsKey(m_curSelected))
            m_ASArrayDict[m_curSelected].SetSelected(false);
        m_curSelected = Vector2Int.zero;

        m_curSelectedASLocation = new Vector2Int(x,y);
        StartCoroutine(GetASAreaInfo());
    }

    IEnumerator GetASAreaInfo()
    {
        while(true)
        {
            if(ASProxy.instance.OriginalDataReady)
            {
                OnRecieveASMapInfo();
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    void OnRecieveASMapInfo()
    {
        StartCoroutine(InitMap(m_curSelectedASLocation.x, m_curSelectedASLocation.y));

        SetFocusAS(m_lineCount/2, m_lineCount/2);
    }
    
    IEnumerator InitMap(int x, int y)
    {
        Debug.Log("Start init map : "+Time.time);
        m_initFinished = false;
        m_oldCamPos = m_targetCamera.transform.position;
        Vector3 centerPos = Vector3.zero;
        float centerHeight = 0;
        int count = 0;
        int n = 0;
        int arrayc = m_lineCount/2;
        int arraycY = m_lineCount/2;
        m_ASArrayDict.Clear();
        while(count < m_cacheCount)
        {
            if(n == 0)
            {
                ASInfo data = ASProxy.instance.GetASByPosition(x, y);
                if(data != null)
                {
                    centerHeight = data.Height;
                    //centerPos = m_targetCamera.transform.position + m_targetCamera.transform.forward * (12 + centerHeight/2f);
                    centerPos = new Vector3(x * 640.0f / 256.0f, 0.0f, y * 640.0f / 256.0f) + m_targetCamera.transform.forward * (12 + centerHeight/2f);
                    Vector2Int k = new Vector2Int(arrayc,arraycY);
                    m_ASArraySingle[count].transform.position = centerPos;
                    m_ASArraySingle[count].name = string.Format("{0}_{1}", arrayc, arraycY);
                    m_ASArraySingle[count].InitASData(data);
                    m_ASArrayDict.Add(k,m_ASArraySingle[count]);
                    count++;
                }
                else
                {
                    Debug.LogFormat("There is no AS in location : {0},{1}", x, y);
                }
            }
            else
            {
                for(int i = -n; i <= n && count < m_cacheCount; i++)
                {
                    int arrayx = arrayc + i;
                    float posx = centerPos.x + m_baseWith * i;
                    for(int j = -n; j <= n && count < m_cacheCount; )
                    {
                        Vector2Int xy = new Vector2Int(x + i, y + j);
                        int arrayy = arraycY + j;
                        if(IsInMap(xy.x, xy.y))
                        {
                            ASInfo data = ASProxy.instance.GetASByPosition(xy.x, xy.y);
                            if(data != null)
                            {
                                float height = data.Height;
                                float posz = centerPos.z + m_baseWith * j;
                                float posy = centerPos.y + (height - centerHeight)/2;
                                Vector2Int k = new Vector2Int(arrayx,arrayy);
                                m_ASArraySingle[count].transform.position = new Vector3(posx, posy, posz);
                                m_ASArraySingle[count].name = string.Format("{0}_{1}", arrayx,arrayy);
                                m_ASArraySingle[count].InitASData(data);
                                m_ASArrayDict.Add(k,m_ASArraySingle[count]);
                                count++;
                            }
                        }
                        if(i == -n || i == n)
                        {
                            j++;
                        }
                        else
                        {
                            if(j == -n)
                                j = n;
                            else if(j == n)
                                break;
                        }
                    }
                    if(n > 9 && n%2==0)
                        yield return null;

                }
            }
            n++;
        }
        m_initFinished = true;
        Debug.LogFormat("Finish init map in : {0}, with n = {1}",Time.time,n);
    }
    void SetFocusAS(int x, int y, bool moveCam = false)
    {
        Vector2Int v = new Vector2Int(x,y);
        if(v != m_curSelected )
        {
            if(m_ASArrayDict.ContainsKey(m_curSelected))
                m_ASArrayDict[m_curSelected].SetSelected(false);

            m_ASArrayDict[v].SetSelected(true);
            m_curSelected = v;

            if(moveCam)
            {
                Vector3 targetPos = m_ASArrayDict[v].transform.position - m_targetCamera.transform.forward * (12 + m_ASArrayDict[v].Height/2f);//m_targetCamera.transform.position + m_targetCamera.transform.forward * (12 + m_ASArray[v.x][v.y].Height/2f);
                m_camController.raycastas.FocusCamera(targetPos);
            }
        }
    }

    IEnumerator CreateASCubes()
    {
        m_ASArraySingle = new SingleAS[m_cacheCount];
        for(int i = 0; i < m_cacheCount; i++)
        {
            m_ASArraySingle[i] = Instantiate(m_SingleASPrefab, m_root).GetComponent<SingleAS>();
            m_ASArraySingle[i].WanderingASMap = this;
            yield return null;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(InWanderingState)
        {
            if(m_targetCamera.transform.position.y > 70)
            {
                //退回鸟瞰view
                InWanderingState = false;
                m_camController.ViewAS();
                return;
            }
            if(Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = m_targetCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray,out hitInfo))
                {
                    string hitname = hitInfo.transform.name;
                    if(string.IsNullOrEmpty(hitname))
                        return;

                    string[] strs = hitname.Split('_');
                    if(strs.Length < 2)
                        return;

                    int x,y = 0;
                    if(!int.TryParse(strs[0],out x) || !int.TryParse(strs[1],out y))
                        return;

                    SetFocusAS(x,y,true);
                }
            }

            // //移动填充视野
            // Vector3 distance = m_targetCamera.transform.position - m_oldCamPos;
            // if(m_initFinished && Mathf.Abs(distance.x) > m_baseWith )
            // {
            //     //Debug.Log("Moving x ...");
            //     m_oldCamPos.x = m_targetCamera.transform.position.x;
            //     m_oldCamPos.y = m_targetCamera.transform.position.y;

            //     SingleAS[] srcA;
            //     SingleAS[] cmpA;
            //     int mR = m_MLR == 0 ? m_ASArray.Length - 1 : m_MLR - 1;
            //     int neg = 1;
            //     int nextMLR = 0;
            //     int xx = 0;
            //     if(distance.x > 0)
            //     {
            //         srcA = m_ASArray[m_MLR];
            //         cmpA = m_ASArray[mR];
            //         nextMLR = (m_MLR + 1) % m_ASArray.Length;
            //         xx = m_MLR;
            //     }
            //     else
            //     {
            //         srcA = m_ASArray[mR];
            //         cmpA = m_ASArray[m_MLR];
            //         neg = -1;
            //         nextMLR = mR;
            //         xx = mR;
            //     }

            //     {
            //         bool moved = false;
            //         for(int i = 0; i < srcA.Length; i++)
            //         {
            //             Vector2Int v = new Vector2Int(neg + cmpA[i].Location.x, cmpA[i].Location.y);
            //             if(IsInMap(v.x, v.y))
            //             {
            //                 ASInfo data = ASProxy.instance.GetASByPosition(v.x, v.y);//ASProxy.instance.GetASByPosition(v.x, v.y, GetASHeight(v.x, v.y));//
            //                 if(data == null)
            //                 {
            //                     srcA[i].gameObject.SetActive(false);
            //                     continue;
            //                 }
            //                 else
            //                 {
            //                     moved = true;
            //                     float height = data.Height;
            //                     Vector3 pos =  cmpA[i].transform.position;
            //                     float posY = srcA[i].transform.position.y + (height - srcA[i].Height)/2;
            //                     srcA[i].transform.position = new Vector3(pos.x + neg * m_baseWith, posY, pos.z);
            //                     srcA[i].RefreshAS(data);
            //                     if(m_curSelected.x == xx && m_curSelected.y == i)
            //                     {
            //                         srcA[i].SetSelected(false);
            //                     }
            //                 }
            //             }
            //             else
            //             {
            //                 break;
            //             }
            //         }
            //         if(moved)
            //             m_MLR = nextMLR;
            //     }
            //     //return;
            // }

            // if(m_initFinished &&  Mathf.Abs(distance.z) > m_baseWith)
            // {
            //     //Debug.Log("Moving z ..." + m_MDL);
            //     m_oldCamPos.y = m_targetCamera.transform.position.y;
            //     m_oldCamPos.z = m_targetCamera.transform.position.z;

            //     int mU = m_MDL == 0 ? m_ASArray.Length - 1 : m_MDL - 1;
            //     int nextMDL = 0;
            //     int neg = 0;
            //     int src = 0;
            //     int cmp = 0;
            //     if(distance.z > 0)
            //     {
            //         src = m_MDL;
            //         cmp = mU;
            //         nextMDL = (m_MDL + 1) % m_ASArray.Length;
            //         neg = 1;
            //     }
            //     else
            //     {
            //         src = mU;
            //         cmp = m_MDL;
            //         nextMDL = mU;
            //         neg = -1;
            //     }

            //     {
            //         bool moved = false;
            //         for(int i = 0; i < m_ASArray.Length; i++)
            //         {
            //             Vector2Int v = new Vector2Int(m_ASArray[i][cmp].Location.x, m_ASArray[i][cmp].Location.y + neg);
            //             if(IsInMap(v.x, v.y))
            //             {
            //                 ASInfo data = ASProxy.instance.GetASByPosition(v.x, v.y);//ASProxy.instance.GetASByPosition(v.x, v.y, GetASHeight(v.x, v.y));//
            //                 if(data == null)
            //                 {
            //                     m_ASArray[i][src].gameObject.SetActive(false);
            //                     continue;
            //                 }
            //                 else
            //                 {
            //                     moved = true;
            //                     float height = data.Height;
            //                     Vector3 pos =  m_ASArray[i][cmp].transform.position;
            //                     float posY = m_ASArray[i][src].transform.position.y + (height - m_ASArray[i][src].Height)/2;
            //                     m_ASArray[i][src].transform.position = new Vector3(pos.x, posY, pos.z + neg * m_baseWith);
            //                     m_ASArray[i][src].RefreshAS(data);
            //                     if(m_curSelected.x == m_MDL && m_curSelected.y == i)
            //                     {
            //                         m_ASArray[i][src].SetSelected(false);
            //                     }
            //                 }

            //             }
            //             else
            //             {
            //                 break;
            //             }
            //         }
            //         if(moved)
            //             m_MDL = nextMDL;
            //     }
            // }
        }
    }

    float GetASHeight(float x, float y)
    {
        return (m_Heights.GetPixelBilinear(x/m_mapWidth, y/m_mapWidth).r* 0.3f)*100.0f;
    }

    bool IsInMap(int x, int y)
    {
        if(x >=0 && x < m_mapWidth && y >=0 && y < m_mapWidth)
            return true;

        return false;
    }
}
