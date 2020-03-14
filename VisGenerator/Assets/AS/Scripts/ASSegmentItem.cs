using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ASSegmentItem : MonoBehaviour
{
    public MeshRenderer m_SegmentIPMap;
    public TextMeshPro m_SegmentName;
    public Collider m_Collider;
    public ASSegmentInfo m_SegemntData;
    public int SegmentObjID{
        get {return m_segmentID;}
    }
    private Vector3 m_OrifinalScale = Vector3.zero;
    private int m_segmentID;
    private int m_texturePixelLineCount = 1;

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }
    public void SetSegment(ASSegmentInfo data)
    {
        m_SegemntData = data;

        float scale = m_SegemntData.GetRadius();
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

        m_SegmentName.text = "--- " + m_SegemntData.Time;// + " , " + m_SegemntData.IPCount;

        if(m_OrifinalScale == Vector3.zero)
            m_OrifinalScale = m_SegmentName.transform.localScale;

        float scale = transform.localScale.x;
        m_SegmentName.transform.localScale = new Vector3(m_OrifinalScale.x/scale,m_OrifinalScale.y,m_OrifinalScale.z/scale);
        m_SegmentName.transform.localPosition = Vector3.zero;
        m_SegmentName.transform.rotation = Camera.main.transform.rotation;
        m_SegmentName.transform.position += m_SegmentName.transform.right * scale;
    }

    public void SetIPMap(bool open)
    {
        if(!open)
        {
            SegmentPool.Instance.ReturnBackTexture((Texture2D)m_SegmentIPMap.material.GetTexture("_BaseColorMap"));
            m_SegmentIPMap.material.SetTexture("_BaseColorMap",null);
            return;
        }

        m_SegmentIPMap.material.SetTexture("_BaseColorMap", SegmentPool.Instance.GetTexture(m_SegemntData, out m_texturePixelLineCount));
    }

    public void SetCollider(bool open)
    {
        m_Collider.enabled = open;
    }

    public int GetIPIndexByPos(Vector2 pos)
    {
        float scale = transform.localScale.x;
        pos += new Vector2(scale/2, scale/2);
        //Texture tex = m_SegmentIPMap.material.GetTexture("_BaseColorMap");
        //int width = tex.width;
        int x = (int)(pos.x / scale * m_texturePixelLineCount);
        int y = (int)(pos.y / scale * m_texturePixelLineCount);
        int count = y*m_texturePixelLineCount + x;
        count = count >= 0 ? count : 0;
        if(count >= m_SegemntData.IPCount)
            count = m_SegemntData.IPCount - 1;

        Debug.LogFormat("seg name and ip count : {0},{1}",name,m_SegemntData.IPCount);
        
        return count;
    }
}
