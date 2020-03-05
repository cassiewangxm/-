using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private IPMenuPanel m_IPMenu;
    [SerializeField] private IPDetailPanel m_IPDetailPanel;
    [SerializeField] private IPTopologyPanel m_IPTopologyPanel;


    private void Start()
    {
        UIEventDispatcher.showIPMenuPanel += ShowIPMenu;
        UIEventDispatcher.hideIPMenuPanel += HideIpMenu;
        UIEventDispatcher.showIPDetail += ShowIPDetail;
        UIEventDispatcher.showIPTopology += ShowTopology;
    }

    public void ShowIPMenu(string _IP, Vector2 screenPos)
    {
        if (m_IPMenu != null)
        {
            m_IPMenu.gameObject.SetActive(true);
#if true
            _IP = IPProxy.fakeTestIp;
#endif
            m_IPMenu.SetUIData(_IP, screenPos);
        }
    }

    public void HideIpMenu()
    {
        if (m_IPMenu != null)
            m_IPMenu.gameObject.SetActive(false);
    }

    public void ShowIPDetail(string _IP)
    {
        if (m_IPDetailPanel != null)
        {
            m_IPDetailPanel.gameObject.SetActive(true);
            m_IPDetailPanel.SetUIData(_IP);
        }
    }

    public void ShowTopology(string _IP)
    {
        if (m_IPTopologyPanel != null)
        {
            m_IPTopologyPanel.gameObject.SetActive(true);
            m_IPTopologyPanel.SetUIData(_IP);
        }
    }
}