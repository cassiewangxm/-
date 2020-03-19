using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPMenuPanel : UIBasePanel
{
    [SerializeField]
    private TMP_Text m_IPText;

    [SerializeField]
    private Button m_DetailBtn;
    [SerializeField]
    private Button m_TopologyBtn;
    [SerializeField]
    private Button m_AttackBtn;
    [SerializeField]
    private Button m_BanBtn;
    [SerializeField]
    private RectTransform m_background;

    private string m_IP;

    private void OnEnable()
    {
        m_DetailBtn.onClick.AddListener(ShowDetail);
        m_TopologyBtn.onClick.AddListener(ShowTopology);
    }

    private void OnDisable()
    {
        m_DetailBtn.onClick.RemoveAllListeners();

    }

    public void SetUIData(string _IP, Vector2 pos)
    {
        m_IP = _IP;
        m_IPText.text = m_IP;
        UpdatePos(pos);
    }

    private void UpdatePos(Vector2 pos)
    {
        Vector2 position;
        float x = Mathf.Max(pos.x, m_background.rect.width + UIPadding);
        float y = Mathf.Min(pos.y, Screen.height - UIPadding);
        x = Mathf.Min(pos.x, Screen.width - m_background.rect.width - UIPadding);
        y = Mathf.Max(pos.y, m_background.rect.height + UIPadding);
        position = new Vector2(x, y);
        m_background.position = position;
    }
    
    private void ShowDetail()
    {
        //UIEventDispatcher.OpenIPDetailPanel(m_IP);
        //gameObject.SetActive(false);
    }

    private void ShowTopology()
    {
        UIEventDispatcher.OpenIPTopologyPanel(m_IP);
        gameObject.SetActive(false);
    }

    private void AttackIp()
    {

    }

    private void BanIp()
    {

    }
}
