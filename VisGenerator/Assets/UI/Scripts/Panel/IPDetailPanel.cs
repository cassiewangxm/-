using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPDetailPanel : UIBasePanel
{
    //[SerializeField]
    //private Button m_CloseBtn;
    [SerializeField]
    private Text m_IpText;
    [SerializeField]
    private Text m_DescText;

    private string m_IP;
    private IpDetail m_IPDetailData;

    // private void OnEnable()
    // {
    //     m_CloseBtn.onClick.AddListener(OnClose);
    // }

    // private void OnDisable()
    // {
    //     m_CloseBtn.onClick.RemoveAllListeners();
    // }
    
    public void SetUIData(string ip)
    {
        Clean();
        m_IP = ip;
        m_IPDetailData = IPProxy.instance.GetIpDetail(ip);
        UpdateUI();
    }

    public void SetUIData(IpDetail ipinfo)
    {
        Clean();
        m_IP = ipinfo.IP;;
        m_IPDetailData = ipinfo;
        UpdateUI();
    }

    public void UpdatePos(Vector2 screenPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 offset = screenPos - new Vector2(Screen.width/2, Screen.height/2);

        if(offset.x > 0)
            offset.x += rect.sizeDelta.x/2;
        else
            offset.x -= rect.sizeDelta.x/2;
        
        //越界检测
        if(offset.x - rect.sizeDelta.x/2 < - Screen.width/2)
            offset.x = rect.sizeDelta.x/2 - Screen.width/2;
        if(offset.x + rect.sizeDelta.x/2 > Screen.width/2)
            offset.x = Screen.width/2 - rect.sizeDelta.x/2;
        if(offset.y - rect.sizeDelta.y/2 < - Screen.height/2)
            offset.y = rect.sizeDelta.y/2 - Screen.height/2;
        if(offset.y + rect.sizeDelta.y/2 > Screen.height/2)
            offset.y = Screen.height/2 - rect.sizeDelta.y/2;
        
        rect.anchoredPosition = offset;
    }

    private void UpdateUI()
    {
        if (m_IPDetailData == null) 
            return;
        m_IpText.text = m_IPDetailData.IP;
        m_DescText.text = m_IPDetailData.GetDesc();
    }

    private void Clean()
    {
        m_IpText.text = string.Empty;
        m_DescText.text = string.Empty;
    }

    
}
