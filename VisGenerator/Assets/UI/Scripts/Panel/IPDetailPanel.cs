using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPDetailPanel : UIBasePanel
{
    [SerializeField]
    private Button m_CloseBtn;
    [SerializeField]
    private TMP_Text m_IpText;
    [SerializeField]
    private TMP_Text m_DescText;

    private string m_IP;
    private IpDetail m_IPDetailData;

    private void OnEnable()
    {
        m_CloseBtn.onClick.AddListener(OnClose);
    }

    private void OnDisable()
    {
        m_CloseBtn.onClick.RemoveAllListeners();
    }

    
    public void SetUIData(string ip)
    {
        Clean();
        m_IP = ip;
        m_IPDetailData = IPProxy.instance.GetIpDetail(ip);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (m_IPDetailData == null) 
            return;
        m_IpText.text = m_IPDetailData.IP;
        m_DescText.text = m_IPDetailData.name;
    }

    private void Clean()
    {
        m_IpText.text = string.Empty;
        m_DescText.text = string.Empty;
    }

    
}
