using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class ASAppearMonitor : MonoBehaviour
{
    public Vector2Int Location {get;set;}
    public WanderingASMapV2 Wanderingmap {get; set;}
    public Action<SingleASV2> OnASBecameInvisible; 
    SingleASV2 m_asObject;
    private bool m_focused;

    public void SetFocus(bool value)
    {
        m_focused = value;
        if(m_asObject!=null && m_focused != m_asObject.Focused)
        {
            m_asObject.SetSelected(m_focused);
        }
    }

    void OnBecameVisible()
    {
        ASInfo data = ASProxy.instance.GetASByPosition(Location.x, Location.y);
        if(data != null && ASPool.Instance != null)
        {
            if(m_asObject == null)
                m_asObject = ASPool.Instance.GetASSave();
            m_asObject.transform.name = name;
            m_asObject.WanderingASMap = Wanderingmap;
            m_asObject.AppearRoot = this;
            m_asObject.transform.SetParent(transform);
            m_asObject.transform.localPosition = new Vector3(0, data.Height/2, 0);
            m_asObject.RefreshAS(data);
            if(m_focused != m_asObject.Focused)
            {
                SetFocus(m_focused);
            }
        }
        else
        {
            ReturnASObject();
        }
    }

    void OnBecameInvisible()
    {
        if(m_asObject != null && !m_asObject.Focused)
            ReturnASObject();
    }

    public void ReturnASObject()
    {
        if(m_asObject != null)
        {
            OnASBecameInvisible(m_asObject);
            m_asObject = null;
        }
    }
}
