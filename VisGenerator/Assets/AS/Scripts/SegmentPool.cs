using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentPool : MonoBehaviour
{
    public static SegmentPool Instance
    {
        get {
                if(m_instance == null)
                {
                    m_instance = new GameObject("SegmentPool").AddComponent<SegmentPool>();
                    GameObject parent = GameObject.Find("Singleton");
                    if (parent != null)
                        m_instance.transform.parent = parent.transform;
                }
                return m_instance;
            }
    }
    private GameObject m_segmentPrefab;
    private static SegmentPool m_instance;
    private const int m_textureW = 256;
    private const int m_originalCount = 20;
    private Stack<Texture2D> m_freeTextures = new Stack<Texture2D>();
    private Dictionary<int,Texture2D> m_inUsingTextures = new Dictionary<int,Texture2D>();
    private Stack<ASSegmentItem> m_freeSegments = new Stack<ASSegmentItem>();
    private Dictionary<int,ASSegmentItem> m_inUsingSegments = new Dictionary<int,ASSegmentItem>();

    public void Prepare()
    {
        m_segmentPrefab = Resources.Load("SegmentItem") as GameObject;
        transform.position = new Vector3(0,1000,0);
        StartCoroutine(GenerateObjects());
    }
    
    IEnumerator GenerateObjects()
    {
        if(m_freeTextures.Count == 0 && m_freeSegments.Count == 0)
        {
            for(int i = 0; i < m_originalCount; i++)
            {
                m_freeTextures.Push(new Texture2D(m_textureW, m_textureW));
                m_freeSegments.Push(Instantiate(m_segmentPrefab, transform).GetComponent<ASSegmentItem>());
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public ASSegmentItem GetSegment()
    {
        ASSegmentItem seg;
        if(m_freeSegments.Count > 0)
        {
            seg = m_freeSegments.Pop();
        }
        else
        {
            seg = Instantiate(m_segmentPrefab, transform).GetComponent<ASSegmentItem>();
        }
        m_inUsingSegments.Add(seg.GetInstanceID(), seg);
        
        return seg;
    }

    // ASSegmentItem GetSegmentFormStask()
    // {
    //     ASSegmentItem seg;
    //     if(m_freeSegments.Count > 0)
    //     {
    //         seg = m_freeSegments.Pop();
    //     }
    //     else
    //     {
    //         seg = Instantiate(m_segmentPrefab).GetComponent<ASSegmentItem>();
    //     }
    //     m_inUsingSegments.Add(seg.GetInstanceID(), seg);
        
    //     return seg;
    // }

    public void ReturnBackSegment(ASSegmentItem seg)
    {
        if(seg == null)
        {
            Debug.LogWarning("Somthing is wrong ??");
            return;
        }
        if(m_inUsingSegments.ContainsKey(seg.GetInstanceID()))
        {
            m_inUsingSegments.Remove(seg.GetInstanceID());
        }
        else
        {
            Debug.LogWarning("Somthing is wrong ??");
        }
        seg.transform.SetParent(transform);
        seg.transform.position = Vector3.zero;
        m_freeSegments.Push(seg);
    }

    public Texture2D GetTexture(ASSegmentInfo segData, out int lineCount)
    {
        Texture2D tex = GetTextureFormStask();
        //int needSize = (int)Mathf.Sqrt(segData.IPCount) + 1;
        
        int pixelsize = (int)Mathf.Sqrt((m_textureW * m_textureW) / segData.IPCount) ;
        pixelsize = pixelsize > 0 ? pixelsize : 1;
        lineCount = m_textureW/pixelsize + 1;

        int curCount = 0;
        for(int i = 0; i < m_textureW; i++)
        {  
            for(int j = 0; j < m_textureW; j++)
            {
                if(j/pixelsize >= lineCount)
                {
                    tex.SetPixel(i, j,Color.black);
                }
                else
                {
                    curCount = i/pixelsize * lineCount + j/pixelsize;
                    tex.SetPixel(j, i, segData.GetIPColor(curCount));
                }
            }
        }
        tex.Apply();

        return tex;
    }

    Texture2D GetTextureFormStask()
    {
        Texture2D tex;
        if(m_freeTextures.Count > 0)
        {
            tex = m_freeTextures.Pop();
        }
        else
        {
            tex = new Texture2D(m_textureW, m_textureW);
        }
        m_inUsingTextures.Add(tex.GetInstanceID(), tex);
        
        return tex;
    }

    public void ReturnBackTexture(Texture2D tex)
    {
        if(tex == null)
        {
            Debug.LogWarning("Somthing is wrong ??");
            return;
        }
        if(m_inUsingTextures.ContainsKey(tex.GetInstanceID()))
        {
            m_inUsingTextures.Remove(tex.GetInstanceID());
            m_freeTextures.Push(tex);
        }
        else
        {
            Debug.LogWarning("Somthing is wrong ??");
            m_freeTextures.Push(tex);
        }
    }
}
