using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPTopologyPanel : UIBasePanel
{
    [SerializeField]
    private Transform m_FirstLevel;
    [SerializeField]
    private Transform m_SecondLevel;
    [SerializeField]
    private Transform m_ThirdLevel;
    [SerializeField]
    private Button m_CloseBtn;
    [SerializeField]
    private TMP_Text m_IPText;
    [SerializeField]
    private GameObject m_boxPrefabe;
    [SerializeField]
    private GameObject m_linePrefab;

    private IpDetail m_FirstLevelIp;
    private IpDetail m_SecondLevelIp;
    private IpDetail m_ThirdLevelIp;

    private List<IpTopologyBox> m_BoxList;
    private List<UIline> m_LinkList;
    //private Queue<> m_BoxPool;

    private void Awake()
    {
        m_BoxList = new List<IpTopologyBox>();
        m_LinkList = new List<UIline>();
    }

    private void OnEnable()
    {
        m_CloseBtn.onClick.AddListener(OnClose);
    }

    private void OnDisable()
    {
        m_CloseBtn.onClick.RemoveAllListeners();
        Clean();
    }

    public void SetUIData(string ip)
    {
        Clean();
        m_FirstLevelIp = IPProxy.instance.GetIpDetail(ip);
        if(m_FirstLevelIp == null)
        {
            Debug.LogErrorFormat("Could not found ip {0}", ip);
            OnClose();
            return;
        }

        if (m_FirstLevelIp.IPParent != IpDetail.DEFAULT_IP)
        {
            m_SecondLevelIp = IPProxy.instance.GetIpDetail(m_FirstLevelIp.IPParent);
            if(m_SecondLevelIp == null)
            {
                Debug.LogErrorFormat("Could not found ip {0}", m_SecondLevelIp.IP);
                OnClose();
                return;
            }
        }

        if(m_SecondLevelIp.IPParent != IpDetail.DEFAULT_IP)
        {
            m_ThirdLevelIp = IPProxy.instance.GetIpDetail(m_SecondLevelIp.IPParent);
            if (m_ThirdLevelIp == null)
            {
                Debug.LogErrorFormat("Could not found ip {0}", m_ThirdLevelIp.IP);
                OnClose();
                return;
            }
        }

        m_IPText.text = ip;

        CreateBox(m_FirstLevelIp, m_FirstLevel);
        CreateBox(m_SecondLevelIp, m_SecondLevel);
        CreateBox(m_ThirdLevelIp, m_ThirdLevel);

        CreateLink();

    }

    private void Clean()
    {
        for(int i = 0, len = m_BoxList.Count; i < len; i++)
        {
            Destroy(m_BoxList[i].gameObject);
        }
        m_BoxList.Clear();

        for (int i = 0, len = m_LinkList.Count; i < len; i++)
        {
            Destroy(m_LinkList[i].gameObject);
        }
        m_LinkList.Clear();

        m_IPText.text = string.Empty;

        m_FirstLevelIp = null;
        m_SecondLevelIp = null;
        m_ThirdLevelIp = null;
    }

    private void CreateBox(IpDetail data, Transform objParent)
    {
        if (m_boxPrefabe == null)
        {
            Debug.LogError("could not found the m_boxPrefabe!!!");
            return;
        }

        IpTopologyBox box = Instantiate(m_boxPrefabe, objParent).GetComponent<IpTopologyBox>();
        box.SetData(data.IP);
        m_BoxList.Add(box);
    }

    private void CreateLink()
    {
        if (m_linePrefab == null)
            return;
        IpTopologyBox cur;
        IpTopologyBox next;
        UIline line;
        for (int i = 0, len = m_BoxList.Count - 1; i < len; i++)
        {
            cur = m_BoxList[i];
            next = m_BoxList[i + 1];
            line = Instantiate(m_linePrefab, cur.NextPoint).GetComponent<UIline>();
            line.SetTarget(next.PrePoint.position, cur.NextPoint.position);

            m_LinkList.Add(line);
        }
    }

    
}
