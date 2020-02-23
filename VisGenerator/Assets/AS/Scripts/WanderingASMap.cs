using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;
using UnityEngine.EventSystems;

public class WanderingASMap : MonoBehaviour
{
    public Texture2D m_Heights;
    public GameObject m_SingleASPrefab;
    public CameraController m_camController;
    public Camera m_targetCamera;
    public Transform m_root;

    private float m_baseWith = 640 / 256;
    private int m_mapWidth = 256;
    private SingleAS[][] m_ASArray;
    private Vector2Int m_curSelected;
    private const int m_lineCount = 20;
    private int m_cacheCount = m_lineCount * m_lineCount; //缓存n个
    private Vector3 m_oldCamPos;
    private int m_MLR = 0; // most Left Row
    private int m_MDL = 0; // most Down Line
    private bool m_initFinished = false;

    void Awake()
    {
        m_oldCamPos = m_targetCamera.transform.position;
        StartCoroutine(CreateASCubes());
    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    public void IntoWanderingMap(int x, int y)
    {
        //取消上次focus的柱子
        if(m_curSelected != Vector2Int.zero)
            m_ASArray[m_curSelected.x][m_curSelected.y].SetSelected(false);
        m_curSelected = Vector2Int.zero;

        StartCoroutine(InitMap(x,y,false));
        //StartCoroutine(InitMap(x,y,true));

        SetFocusAS(m_lineCount/2 + m_lineCount/6, m_lineCount/2 - m_lineCount/6);
    }
    
    IEnumerator InitMap(int x, int y, bool corot)
    {
        Debug.Log("Start init map : "+Time.time);
        m_initFinished = false;
        m_oldCamPos = m_targetCamera.transform.position;
        Vector3 centerPos = Vector3.zero;
        float centerHeight = 0;
        int count = 0;
        int n = 0;
        int arrayc = m_lineCount/2 + m_lineCount/6;
        int arraycY = m_lineCount/2 - m_lineCount/6;
        while(count < m_cacheCount)
        {
            if(n == 0)
            {
                count++;
                centerHeight = GetASHeight(x, y);
                //centerPos = m_targetCamera.transform.position + m_targetCamera.transform.forward * (12 + centerHeight/2f);
                centerPos = new Vector3(x * 640.0f / 256.0f, 0.0f, y * 640.0f / 256.0f);
                if (corot)
                {
                    m_ASArray[arrayc][arraycY].InitASLooking(true);
                    if(centerHeight > 0.0f)
                        yield return new WaitForEndOfFrame();
                    SetFocusAS(arrayc,arraycY);
                }
                else
                {
                    m_ASArray[arrayc][arraycY].transform.position = centerPos;
                    m_ASArray[arrayc][arraycY].name = string.Format("{0}_{1}", arrayc, arraycY);
                    m_ASArray[arrayc][arraycY].InitASData(x, y, centerHeight, m_camController, m_targetCamera);
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
                        float height = GetASHeight(xy.x, xy.y);
                        int arrayy = arraycY + j;
                        if((arrayx >= 0 && arrayx < m_ASArray.Length && arrayy >= 0 && arrayy < m_ASArray[arrayx].Length))
                        {
                            count++;
                            
                            if(corot)
                            {
                                m_ASArray[arrayx][arrayy].InitASLooking(true);
                                if(height > 0.0f)
                                    yield return new WaitForEndOfFrame();
                            }
                            else
                            {
                                float posz = centerPos.z + m_baseWith * j;
                                float posy = centerPos.y + (height - centerHeight)/2;
                                m_ASArray[arrayx][arrayy].transform.position = new Vector3(posx, posy, posz);
                                m_ASArray[arrayx][arrayy].name = string.Format("{0}_{1}", arrayx, arrayy);
                                m_ASArray[arrayx][arrayy].InitASData(xy.x, xy.y, height, m_camController, m_targetCamera);
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
                    if(n > 6)
                        yield return new WaitForEndOfFrame();

                }
            }
            n++;
        }
        m_initFinished = true;
        Debug.Log("Finish init map : "+Time.time);
    }
    void SetFocusAS(int x, int y)
    {
        Vector2Int v = new Vector2Int(x,y);
        if(v != m_curSelected && (v.x >= 0 && v.x < m_ASArray.Length && v.y >= 0 && v.y < m_ASArray[v.x].Length))
        {
            if(m_curSelected != Vector2Int.zero)
                m_ASArray[m_curSelected.x][m_curSelected.y].SetSelected(false);

            m_ASArray[v.x][v.y].SetSelected(true);
            m_curSelected = v;
        }
    }
    IEnumerator CreateASCubes()
    {
        m_ASArray = new SingleAS[m_lineCount][];
        for(int i = 0; i < m_lineCount; i++)
        {
            m_ASArray[i] = new SingleAS[m_lineCount];
            for(int j = 0; j < m_lineCount; j++)
            {
                GameObject obj = Instantiate(m_SingleASPrefab, m_root);   
                m_ASArray[i][j] = obj.GetComponent<SingleAS>();
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(m_camController != null && m_camController.currentView == ViewType.ViewSingleAS)
        {
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

                    SetFocusAS(x,y);
                }
            }

            //移动填充视野
            Vector3 distance = m_targetCamera.transform.position - m_oldCamPos;
            if(m_initFinished && Mathf.Abs(distance.x) > m_baseWith )
            {
                //Debug.Log("Moving x ...");
                m_oldCamPos.x = m_targetCamera.transform.position.x;
                m_oldCamPos.y = m_targetCamera.transform.position.y;

                SingleAS[] srcA;
                SingleAS[] cmpA;
                int mR = m_MLR == 0 ? m_ASArray.Length - 1 : m_MLR - 1;
                int neg = 1;
                int nextMLR = 0;
                int xx = 0;
                if(distance.x > 0)
                {
                    srcA = m_ASArray[m_MLR];
                    cmpA = m_ASArray[mR];
                    nextMLR = (m_MLR + 1) % m_ASArray.Length;
                    xx = m_MLR;
                }
                else
                {
                    srcA = m_ASArray[mR];
                    cmpA = m_ASArray[m_MLR];
                    neg = -1;
                    nextMLR = mR;
                    xx = mR;
                }

                {
                    bool moved = false;
                    for(int i = 0; i < srcA.Length; i++)
                    {
                        Vector2Int v = cmpA[i].ASData.Location + new Vector2Int(neg, 0);
                        if(IsInMap(v.x, v.y))
                        {
                            moved = true;
                            float height = GetASHeight(v.x, v.y);
                            Vector3 pos =  cmpA[i].transform.position;
                            float posY = srcA[i].transform.position.y + (height - srcA[i].Height)/2;
                            srcA[i].transform.position = new Vector3(pos.x + neg * m_baseWith, posY, pos.z);
                            srcA[i].RefreshAS(v.x, v.y, height);
                            if(m_curSelected.x == xx && m_curSelected.y == i)
                            {
                                srcA[i].SetSelected(false);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(moved)
                        m_MLR = nextMLR;
                }
                //return;
            }

            if(m_initFinished &&  Mathf.Abs(distance.z) > m_baseWith)
            {
                //Debug.Log("Moving z ..." + m_MDL);
                m_oldCamPos.y = m_targetCamera.transform.position.y;
                m_oldCamPos.z = m_targetCamera.transform.position.z;

                int mU = m_MDL == 0 ? m_ASArray.Length - 1 : m_MDL - 1;
                int nextMDL = 0;
                int neg = 0;
                int src = 0;
                int cmp = 0;
                if(distance.z > 0)
                {
                    src = m_MDL;
                    cmp = mU;
                    nextMDL = (m_MDL + 1) % m_ASArray.Length;
                    neg = 1;
                }
                else
                {
                    src = mU;
                    cmp = m_MDL;
                    nextMDL = mU;
                    neg = -1;
                }

                {
                    bool moved = false;
                    for(int i = 0; i < m_ASArray.Length; i++)
                    {
                        Vector2Int v = m_ASArray[i][cmp].ASData.Location + new Vector2Int(0, neg);
                        if(IsInMap(v.x, v.y))
                        {
                            moved = true;
                            float height = GetASHeight(v.x, v.y);
                            Vector3 pos =  m_ASArray[i][cmp].transform.position;
                            float posY = m_ASArray[i][src].transform.position.y + (height - m_ASArray[i][src].Height)/2;
                            m_ASArray[i][src].transform.position = new Vector3(pos.x, posY, pos.z + neg * m_baseWith);
                            m_ASArray[i][src].RefreshAS(v.x, v.y, height);
                            if(m_curSelected.x == m_MDL && m_curSelected.y == i)
                            {
                                m_ASArray[i][src].SetSelected(false);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(moved)
                        m_MDL = nextMDL;
                }
            }
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
