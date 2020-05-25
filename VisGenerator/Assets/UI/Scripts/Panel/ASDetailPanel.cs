using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ASDetailPanel : UIBasePanel
{
    //[SerializeField]
    //private Button m_CloseBtn;
    [SerializeField]
    private Text m_IpText;
    [SerializeField]
    private Text m_DescText;

    private string m_ASN;
    private ASInfo m_ASDetailData;

    public Camera Camera;

    // private void OnEnable()
    // {
    //     m_CloseBtn.onClick.AddListener(OnClose);
    // }

    // private void OnDisable()
    // {
    //     m_CloseBtn.onClick.RemoveAllListeners();
    // }
    
    // public void SetUIData(string ip)
    // {
    //     Clean();
    //     m_IP = ip;
    //     m_IPDetailData = IPProxy.instance.GetIpDetail(ip);
    //     UpdateUI();
    // }

    public Vector3 GetLineEndPos(bool left)
    {
        RectTransform rectTrans = GetComponent<RectTransform>();
        float offsetx = - rectTrans.rect.size.x/2 + 5;
        if(!left)
            offsetx = -offsetx;

        Vector3 pos = new Vector3(rectTrans.position.x + offsetx ,rectTrans.position.y ,0);//+ rectTrans.rect.size.y/2 - 5

        return pos;
    }

    public void SetUIData(ASInfo asinfo)
    {
        Clean();
        m_ASN = asinfo.ASN.ToString();
        m_ASDetailData = asinfo;
        UpdateUI();
    }

    public void UpdatePos(Vector2 screenPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 offset = screenPos - new Vector2(Screen.width/2, Screen.height/2);

        if(offset.x > 0)
            offset.x = screenPos.x - 105.0f;
        else
            offset.x = screenPos.x + 105.0f;

        if (offset.y > 0)
            offset.y = screenPos.y - 105.0f;
        else
            offset.y = screenPos.y + 105.0f;

        rect.transform.position = offset;
    }

    private void UpdateUI()
    {
        if (m_ASDetailData == null) 
            return;
        m_IpText.text = m_ASDetailData.ASN.ToString();
        m_DescText.text = GetDesc(m_ASDetailData);
        Vector3 ScreenPoint = Input.mousePosition;
        Debug.Log(ScreenPoint);
        UpdatePos(new Vector2(ScreenPoint.x, ScreenPoint.y));
    }

    private void Clean()
    {
        m_IpText.text = string.Empty;
        m_DescText.text = string.Empty;
    }

    private string GetDesc(ASInfo ASInfo)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("AS : {0}\n", ASInfo.ASN.ToString());
        if (!string.IsNullOrEmpty(ASInfo.org))
            sb.AppendFormat("Org : {0}\n", ASInfo.org);
        if (!string.IsNullOrEmpty(ASInfo.ISP))
            sb.AppendFormat("Region : {0}\n", ASInfo.ISP_country_code);

        return sb.ToString();
    }
}
