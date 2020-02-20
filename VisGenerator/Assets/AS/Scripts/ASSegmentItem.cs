using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ASSegmentItem : MonoBehaviour
{
    public MeshRenderer m_SegmentIPMap;
    public TextMeshPro m_SegmentName;
    public Collider m_Collider;
    public ASSegment m_SegemntData;

    private Texture2D m_IPTexture;

    private int m_TextureW = 256; //区段IP图宽度,可容纳256*256个ip
    private Vector3 m_OrifinalScale = Vector3.zero;

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }
    public void SetSegment(ASSegment data)
    {
        m_SegemntData = data;

        float scale = (float)m_SegemntData.IPCount/256*4 + 2;
        transform.localScale = new Vector3(scale,scale,1);

        m_SegmentName.text = "";

        m_Collider.enabled = false;

        gameObject.SetActive(true);
    }
    public void SetText(bool open)
    {
        if(!open)
        {
            m_SegmentName.text = "";
            return;
        }

        m_SegmentName.text = "---" + m_SegemntData.BornDate + " , " + m_SegemntData.IPCount;

        if(m_OrifinalScale == Vector3.zero)
            m_OrifinalScale = m_SegmentName.transform.localScale;

        float scale = transform.localScale.x;
        m_SegmentName.transform.localScale = new Vector3(m_OrifinalScale.x/scale,m_OrifinalScale.y,m_OrifinalScale.z/scale);
    }

    public void SetIPMap(bool open)
    {
        if(!open)
        {
            m_SegmentIPMap.material.SetTexture("_BaseColorMap",null);
            return;
        }

        InitTexture();
        m_SegmentIPMap.material.SetTexture("_BaseColorMap",m_IPTexture);
    }

    public void SetCollider(bool open)
    {
        m_Collider.enabled = open;
    }

    void InitTexture()
    {
        int pixelsize, lineCount;
        GetTextureAreaInfo(out pixelsize,out lineCount);

        if(m_IPTexture == null)
        {
            m_IPTexture = new Texture2D(m_TextureW,m_TextureW);
        }
        int curCount = 0;
        for(int i = 0; i < m_TextureW; i++)
        {  
            for(int j = 0; j < m_TextureW; j++)
            {
                if(j/pixelsize >= lineCount)
                {
                    m_IPTexture.SetPixel(i, j,Color.black);
                }
                else
                {
                    curCount = i/pixelsize * lineCount + j/pixelsize;
                    m_IPTexture.SetPixel(j, i, m_SegemntData.GetIPColor(curCount));
                }
            }
        }
        m_IPTexture.Apply();
    }
    void GetTextureAreaInfo(out int pixelsize, out int line)
    {
        pixelsize = (int)Mathf.Sqrt((m_TextureW * m_TextureW) / (int)m_SegemntData.IPCount) ;
        pixelsize = pixelsize > 0 ? pixelsize : 1;
        line = m_TextureW/pixelsize + 1;
    }
}
