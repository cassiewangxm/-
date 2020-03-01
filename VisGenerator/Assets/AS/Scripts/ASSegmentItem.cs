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
    public int SegmentObjID{
        get {return m_segmentID;}
    }

    //private Texture2D m_IPTexture;

    //private int m_TextureW = 256; //区段IP图宽度,可容纳256*256个ip
    private Vector3 m_OrifinalScale = Vector3.zero;
    private int m_segmentID;

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }
    public void SetSegment(ASSegment data)
    {
        m_SegemntData = data;

        float scale = (float)m_SegemntData.IPCount/256*2 + 2;
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
            SegmentPool.Instance.ReturnBackTexture((Texture2D)m_SegmentIPMap.material.GetTexture("_BaseColorMap"));
            m_SegmentIPMap.material.SetTexture("_BaseColorMap",null);
            return;
        }

        m_SegmentIPMap.material.SetTexture("_BaseColorMap", SegmentPool.Instance.GetTexture(m_SegemntData));//m_IPTexture);
    }

    public void SetCollider(bool open)
    {
        m_Collider.enabled = open;
    }
}
