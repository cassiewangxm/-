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
    public bool NewPosReady {   get; set;  }
    SingleASV2 m_asObject;
    private bool m_focused;
    private bool m_visible;
    private bool m_searchShowed = true;

    public void SetFocus(bool value)
    {
        m_focused = value;
        if(m_asObject!=null && m_focused != m_asObject.Focused)
        {
            m_asObject.SetSelected(m_focused);
        }
    }

    // public bool SetFileterMode(bool value)
    // {
    //     if(m_asObject != null)
    //     {
    //         m_asObject.SetFileterMode(value);
    //     }

    //     return m_asObject != null;
    // }

    IEnumerator OnBecameVisible()
    {
        while(!NewPosReady || !Wanderingmap.InWanderingState)
            yield return null;

        m_visible = true;
        ASInfo data = ASProxy.instance.GetASByPosition(Location.x, Location.y);
        if(data != null && ASPool.Instance != null)
        {
            if(m_asObject == null)
                m_asObject = ASPool.Instance.GetASSave();
            if(m_asObject.transform.parent != transform)
                m_asObject.transform.SetParent(transform);
            m_asObject.transform.name = name;
            m_asObject.WanderingASMap = Wanderingmap;
            m_asObject.AppearRoot = this;
            m_asObject.transform.localPosition = new Vector3(0, data.Height/2, 0);
            m_asObject.RefreshAS(data);
            if(m_focused != m_asObject.Focused)
            {
                SetFocus(m_focused);
            }

            if(!m_searchShowed)
            {
                m_asObject.SetFileterMode();
            }
        }
        else
        {
            ReturnASObject();
        }
    }

    void OnBecameInvisible()
    {
        m_visible = false;
        if(m_asObject != null && !m_asObject.IsVisibleInCam && !m_asObject.Focused)
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

    public void OnLeaveWanderingMap()
    {
        NewPosReady = false;
        ReturnASObject();
    }

    public void OnShowSearchResult()
    {
        m_searchShowed = false;
        if(m_asObject != null)
        {
            m_asObject.SetFileterMode();
            m_searchShowed = true;
        }
    }
    public void OnClearSearchResult()
    {
        m_searchShowed = true;
        if(m_asObject != null)
        {
            m_asObject.ClearFilterMode();
        }
    }

    public void Reawake()
    {
        if(m_visible)
        {
            StopAllCoroutines();
            StartCoroutine(OnBecameVisible()); 
        }
    }
}
