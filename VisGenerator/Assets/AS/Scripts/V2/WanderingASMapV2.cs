using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

public class WanderingASMapV2 : MonoBehaviour
{
    //public Texture2D m_Heights;
    //public GameObject m_SingleASPrefab;
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

    private UnityEvent OnCameraMovedEnough = new UnityEvent();
    private UnityEvent OnLeavedWandering = new UnityEvent();
    private UnityEvent OnGetSearchResult = new UnityEvent();
    private UnityEvent OnClearSearchResult = new UnityEvent();
    private float m_baseWith = 12;
    private int m_mapWidth = 256;
    private Vector2Int m_curSelected;
    private const int m_lineCount = 64;
    private int m_cacheCount = m_lineCount * m_lineCount; //缓存n个
    private Vector3 m_oldCamPos;
    private Vector3 m_enterCamPos;
    private bool m_initFinished = false;
    private Vector2Int m_enterMomentASLocation;
    private ASAppearMonitor[][] m_array;
    private Dictionary<int,Vector2Int> m_ASN_Pos_Dict = new Dictionary<int, Vector2Int>();
    private Coroutine m_initMapCorotine;

    void Awake()
    {
        EventManager.RegistEvent(EventDefine.OnSceneViewChange, (Action)OnViewChange);
        EventManager.RegistEvent(EventDefine.OnRecieveSearchResult, (Action)RecieveSearchResult);
        EventManager.RegistEvent(EventDefine.OnClearSearchResult, (Action)ClearSearchResult);
        SegmentPool.Instance.Prepare();
        ASPool.Instance.Prepare();
        m_oldCamPos = m_targetCamera.transform.position;
        ASProxy.instance.GetASInfoOriginal(null);
        StartCoroutine(CreateASCubes());
    }
    void OnViewChange()
    {
        // switch (SceneMananger.Instance.CurrentSceneView)
        // {
        //     case SceneView.MapView:
        //     case SceneView.IPView:
        //         break;
        //     case SceneView.ASView:
        //         break;
        // }
    }
    void RecieveSearchResult()
    {
        OnGetSearchResult.Invoke();
    }

    void ClearSearchResult()
    {
        OnClearSearchResult.Invoke();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        OnLeavedWandering.RemoveAllListeners();
        Debug.Log(" OnDestroy WanderingMapV2 .......");
    }
    public void IntoWanderingMap(int x, int y)
    {
        InWanderingState = true;

        //取消上次focus的柱子
        if(m_curSelected != Vector2Int.zero)
            m_array[m_curSelected.x][m_curSelected.y].SetFocus(false);
        m_curSelected = Vector2Int.zero;

        m_enterMomentASLocation = new Vector2Int(x,y);
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
        //m_targetCamera.farClipPlane = ASProxy.instance.HeightMax;

        if(m_initMapCorotine != null)
            StopCoroutine(m_initMapCorotine);

        m_initMapCorotine = StartCoroutine(InitMap(m_enterMomentASLocation.x, m_enterMomentASLocation.y));

        SetFocusAS(m_lineCount/2, m_lineCount/2);
    }
    
    IEnumerator InitMap(int x, int y)
    {
        m_ASN_Pos_Dict.Clear();
        Debug.Log("Start init map : "+Time.time);
        m_initFinished = false;
        m_oldCamPos = m_targetCamera.transform.position;
        m_enterCamPos = m_targetCamera.transform.position;
        Vector3 centerPos = Vector3.zero;
        Vector3 transferVect = Vector3.zero;
        float centerHeight = 0;
        int count = 0;
        int n = 0;
        int cx = m_lineCount/2;
        int cy = m_lineCount/2;
        Vector2Int offset = new Vector2Int(x - cx, y - cy);
        //m_ASArrayDict.Clear();
        while(count < m_cacheCount)
        {
            if(n == 0)
            {
                ASInfo data = ASProxy.instance.GetASByPosition(x, y);
                if(data != null)
                {
                    m_ASN_Pos_Dict.Add(data.ASN, new Vector2Int(cx, cy));
                    centerHeight = data.Height;
                    centerPos = new Vector3(x * 640.0f / 256.0f, -centerHeight/2, y * 640.0f / 256.0f) + m_targetCamera.transform.forward * (12 + centerHeight/2);
                    transferVect = centerPos - m_array[cx][cy].transform.position;
                    m_array[cx][cy].Location = new Vector2Int(x, y);
                    m_array[cx][cy].transform.position = centerPos;
                    m_array[cx][cy].NewPosReady = true;
                    m_array[cx][cy].Reawake();
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
                    int px = cx + i;
                    //float posx = centerPos.x + m_baseWith * i;
                    for(int j = -n; j <= n && count < m_cacheCount; )
                    {
                        Vector2Int xy = new Vector2Int(x + i, y + j);
                        int py = cy + j;
                        if(IsInMap(xy.x, xy.y) && px >=0 && px < m_lineCount && py >=0 && py < m_lineCount)
                        {
                            m_array[px][py].Location = new Vector2Int(px + offset.x, py + offset.y);
                            m_array[px][py].transform.Translate(transferVect.x, transferVect.y, transferVect.z);
                            m_array[px][py].NewPosReady = true;
                            m_array[px][py].Reawake();
                            count++;
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
                    if(n > 10 && n%2 ==0)
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
            ASProxy.instance.ManageCacheSize();
            
            m_array[m_curSelected.x][m_curSelected.y].SetFocus(false);
            m_array[x][y].SetFocus(true);
            
            m_curSelected = v;

            if(moveCam)
            {
                ASInfo data = ASProxy.instance.GetASByPosition(m_array[x][y].Location.x, m_array[x][y].Location.y);
                Vector3 targetPos = m_array[x][y].transform.position - m_targetCamera.transform.forward * (12 + data.Height/2) + new Vector3(0,data.Height/2,0);//m_targetCamera.transform.position + m_targetCamera.transform.forward * (12 + m_ASArray[v.x][v.y].Height/2f);
                m_camController.raycastas.FocusCamera(targetPos);
            }
        }
    }

    IEnumerator CreateASCubes()
    {
        Debug.Log("Start : "+ Time.time);
        //int gap = 10;
        m_array = new ASAppearMonitor[m_lineCount][];
        for(int i =0;i<m_lineCount;i++)
        {
            m_array[i] = new ASAppearMonitor[m_lineCount];
            for(int j =0;j<m_lineCount;j++)
            {
                m_array[i][j] = new GameObject(string.Format("{0}_{1}",i,j)).AddComponent<ASAppearMonitor>();
                m_array[i][j].transform.SetParent(m_root);
                m_array[i][j].OnASBecameInvisible = OnASBecameInvisible;
                m_array[i][j].Wanderingmap = this;
                m_array[i][j].gameObject.AddComponent<MeshRenderer>();
                m_array[i][j].transform.position = new Vector3((i - m_lineCount/2)*m_baseWith, 0, (j - m_lineCount/2)*m_baseWith);

                RigistLeaveWandering((UnityEngine.Events.UnityAction)m_array[i][j].OnLeaveWanderingMap);
                RegistSearchEvent((UnityEngine.Events.UnityAction)m_array[i][j].OnShowSearchResult);
                RegistClearSearchEvent((UnityEngine.Events.UnityAction)m_array[i][j].OnClearSearchResult);
            }

            yield return 0;
        }

        Debug.Log("FInish : " + Time.time);
    }

    public void  RegistCameraMoveEvent(UnityAction act)
    {
        OnCameraMovedEnough.AddListener(act);
    }

    public void UnregistCameraMoveEvent(UnityAction act)
    {
        OnCameraMovedEnough.RemoveListener(act);
    }

    public void RigistLeaveWandering(UnityAction act)
    {
        OnLeavedWandering.AddListener(act);
    }

    public void UnregistLeaveWandering(UnityAction act)
    {
        OnLeavedWandering.RemoveListener(act);
    }

    public void RegistSearchEvent(UnityAction act)
    {
        OnGetSearchResult.AddListener(act);
    }

    public void RegistClearSearchEvent(UnityAction act)
    {
        OnClearSearchResult.AddListener(act);
    }

    void OnASBecameInvisible(SingleASV2 a)
    {
        if(ASPool.Instance != null)
            ASPool.Instance.ReturnBackAS(a);
    }

    void LeaveWanderingToAS()
    {
        //按比例重设camera位置
        Vector3 curPos = m_targetCamera.transform.position;
        Vector3 targetPos = m_enterCamPos + (curPos - m_enterCamPos)/(m_baseWith/2.5f);
        targetPos.y = Mathf.Max(ASProxy.instance.HeightMax/4, targetPos.y);
        m_targetCamera.transform.position = targetPos;

        //退回鸟瞰view
        InWanderingState = false;
        m_camController.ViewAS();

        //
        StopCoroutine(m_initMapCorotine);
        OnLeavedWandering.Invoke();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(InWanderingState && SceneMananger.Instance.CurrentSceneView == SceneView.ASView && ASProxy.instance.OriginalDataReady)
        {
            if(m_targetCamera.transform.position.y > 150)//ASProxy.instance.HeightMax)
            {
                LeaveWanderingToAS();
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

            
            float distance = Vector2.Distance(new Vector2(m_targetCamera.transform.position.x, m_targetCamera.transform.position.z), new Vector2(m_oldCamPos.x, m_oldCamPos.z));
            if(distance > (float)m_baseWith)
            {
                m_oldCamPos = m_targetCamera.transform.position;
                OnCameraMovedEnough.Invoke();
            }
        }
    }

    // float GetASHeight(float x, float y)
    // {
    //     return (m_Heights.GetPixelBilinear(x/m_mapWidth, y/m_mapWidth).r* 0.3f)*100.0f;
    // }

    bool IsInMap(int x, int y)
    {
        if(x >=0 && x < m_mapWidth && y >=0 && y < m_mapWidth)
            return true;

        return false;
    }
}
