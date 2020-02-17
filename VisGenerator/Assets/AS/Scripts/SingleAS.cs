using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controller;

/*
1. 确保 raycastas.cs 中的 Heights贴图与AS地图中用的图片一致
2. 
*/

public class SingleAS : MonoBehaviour
{
    public CameraController cameraController;
    public GameObject m_ASCube; //模拟柱体
    public Transform m_RegmentsRoot;//quad的 根结点
    public GameObject m_QuadPrefab; // 用于划分区段
    public SpriteRenderer m_SegmentIPMap; //区段IP图
    public Camera m_targetCamera;

    private List<ASSegmentItem> m_SegmentList = new List<ASSegmentItem>(); //用于显示IP区段时间等信息
    private ASDetail m_ASData; //AS柱数据
    private Texture2D m_tempTexture;    //区段IP图
    private int m_TextureW = 256; //区段IP图宽度,可容纳256*256个ip
    private int m_curSegment;   //当前选中的区段序号

    // PPPS: 若要把柱体放到选中的具体为止，则在下面函数中加vector3参数即可
    public void ShowSingleAS(int x, int y, float height)
    {
        m_SegmentIPMap.gameObject.SetActive(false);

        ASProxy.instance.GetASByPosition(x,y,height,out m_ASData);
        
        m_RegmentsRoot.position = m_targetCamera.transform.position + m_targetCamera.transform.forward * (6 + height/5) - new Vector3(3,0,0);

        m_ASCube.transform.position = m_RegmentsRoot.position;
        m_ASCube.transform.localScale = new Vector3(m_ASCube.transform.localScale.x,m_ASData.Segments.Length,m_ASCube.transform.localScale.z);

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
            }
            else if(i < m_SegmentList.Count)
            {
                m_SegmentList[i].HideSelf();
            }
        }
    }
    void Update()
    {
        if(cameraController.currentView == ViewType.ViewSingleAS)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = m_targetCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if(Physics.Raycast(ray,out hitInfo))
                {
                    if(hitInfo.transform.name.CompareTo("SegmentIPs")==0)
                    {
                        Vector3 hitpos = hitInfo.point - m_SegmentIPMap.transform.position;

                        if(hitpos.x < 0 || hitpos.y < 0)
                            return;

                        int x = (int)(hitpos.x*100);
                        int y = (int)(hitpos.y*100);

                        int pixelsize, lineCount;
                        GetTextureAreaInfo(m_curSegment,out pixelsize,out lineCount);
                        int index = y/pixelsize * lineCount + x/pixelsize;

                        //Debug.Log(x+","+y+","+index);
                    }
                    else
                    {
                        ShowIPMap(hitInfo.transform.name);
                    }
                }
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

        Color[] colors = {Color.red,Color.green,Color.yellow,Color.blue};
        m_SegmentIPMap.gameObject.SetActive(true);
        if(index < m_SegmentList.Count)
        {
            m_curSegment = index;
            int pixelsize, lineCount;
            GetTextureAreaInfo(index,out pixelsize,out lineCount);

            if(m_tempTexture == null)
            {
                m_tempTexture = new Texture2D(m_TextureW,m_TextureW);
                m_SegmentIPMap.sprite =  Sprite.Create(m_tempTexture,new Rect(0,0,m_tempTexture.width,m_tempTexture.height),new Vector2(0,0));
            }
            int curCount = 0;
            for(int i = 0; i < m_TextureW; i++)
            {  
                for(int j = 0; j < m_TextureW; j++)
                {
                    if(j/pixelsize >= lineCount)
                    {
                        m_tempTexture.SetPixel(i, j,Color.black);
                    }
                    else
                    {
                        curCount = i/pixelsize * lineCount + j/pixelsize;
                        m_tempTexture.SetPixel(j, i, m_SegmentList[index].segemntData.GetIPColor(curCount));// colors[(i/pixelsize + j/pixelsize)%4]);
                    }
                }
            }
            
           // Debug.Log(pixelsize+","+lineCount);
            
            m_tempTexture.Apply();

            m_SegmentIPMap.transform.position = new Vector3(m_SegmentIPMap.transform.position.x,m_SegmentList[index].transform.position.y - 1,m_SegmentIPMap.transform.position.z);
        }
    }

    void GetTextureAreaInfo(int index, out int pixelsize, out int line)
    {
        pixelsize = (int)Mathf.Sqrt((m_TextureW * m_TextureW) / (int)m_SegmentList[index].segemntData.IPCount) ;
        pixelsize = pixelsize > 0 ? pixelsize : 1;
        line = m_TextureW/pixelsize + 1;
    }
}
