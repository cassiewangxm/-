using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Text;


[Serializable]
public class IpDetail
{
    public string IP;
    public int ASNum;
    public string IPParent;
    public string country;
    public string province;
    public string city;
    public float lat;
    public float lng;
    public int X;
    public int Y;

    private Vector2 mapCoordinate;

    public Vector2 MapCoordinate
    {
        get
        {
            mapCoordinate.x = lat;
            mapCoordinate.y = lng;
            return mapCoordinate;
        }
    }

    public IpDetail(IpInfoType1 info)
    {
        if(string.IsNullOrEmpty(info.ip))
        {
            if(info.ip_prefix.Contains("/"))
            {
                IP = info.ip_prefix.Substring(0,info.ip_prefix.IndexOf('/'));
            }
            else
            {
                IP = info.ip_prefix;
            }
        }
        else
        {
            IP = info.ip;
        }
        X = info.X;
        Y = info.Y;
        lat = info.latitude;
        lng = info.longitude;
        ASNum = info.ASN;
        country = info.country_name;
        province = info.provience;
        city = info.city;
    }

    public string GetDesc()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("AS : {0}\n", ASNum);
        if(!string.IsNullOrEmpty(country))
            sb.AppendFormat("Country : {0}\n", country);
        if(!string.IsNullOrEmpty(province))
            sb.AppendFormat("Province : {0}\n", province);
        if(!string.IsNullOrEmpty(city))
            sb.AppendFormat("City : {0}\n", city);
        
        sb.AppendFormat("Coordinate : {0}\n", MapCoordinate);

        return sb.ToString();
        //return string.Format("AS : {0}\nCountry : {1}\nProvince : {2}\nCity : {3}\nCoordinate : {4}", ASNum, country, province, city, MapCoordinate);
    }

    public static readonly string DEFAULT_IP = "0.0.0.0";
}

[Serializable]
public class FakeIPData
{
   public IpDetail[] IPs;
}

//前缀 、 左上角坐标 x,y
public struct IPLayerInfo
{
    public int prefixLen;
    public int x;
    public int y;
}

public class AttackInfo
{
    public string time; //攻击开始的时间戳
    public int attackTime;  //攻击持续的时间，单位 - 秒
    public string destip;
    public IpDetail destIpInfo;
    public int destAS;
    public Vector2Int destASPos;
    public List<AttackSrcInfo> srcInfo; //攻击源信息
    public bool matchData; //是否与本地数据匹配，不匹配即弃

    public AttackInfo(AttackData data)
    {
        destIpInfo = IPProxy.instance.TryGetIpDetail(data.desip, 32);
        ASInfo asinfo = ASProxy.instance.GetASByNumber(data.desAS);
        if(destIpInfo != null && asinfo != null)
        {
            destASPos = new Vector2Int(asinfo.X, asinfo.Y);
            srcInfo = new List<AttackSrcInfo>();
            for(int i = 0; i < data.srcInfo.Length; i++)
            {
                IpDetail detail = IPProxy.instance.TryGetIpDetail(data.srcInfo[i].srcip, 32);
                ASInfo ainfo = ASProxy.instance.GetASByNumber(data.srcInfo[i].srcAS);
                if(detail != null && ainfo != null)
                {
                    AttackSrcInfo info = new AttackSrcInfo(data.srcInfo[i]);
                    info.srcIpInfo = detail;
                    info.srcASPos = new Vector2Int(ainfo.X, ainfo.Y);
                    srcInfo.Add(info);
                }
            }

            if(srcInfo.Count > 0)
            {
                matchData = true;
            }
        }

        if(matchData)
        {
            time = data.time;
            attackTime = data.attackTime;
            destAS = data.desAS;
            destip = data.desip;
        }
    }

    public Vector2Int GetASPosition(int asNum)
    {
        ASInfo info = ASProxy.instance.GetASByNumber(asNum);
        if(info != null)
            return new Vector2Int(info.X, info.Y);
        else
            return Vector2Int.zero;
    }   
}

public class AttackSrcInfo
{
    public string srcip;
    public IpDetail srcIpInfo;
    public int srcAS;
    public Vector2Int srcASPos;
    public int flow;    //该ip地址发送的流的个数

    public AttackSrcInfo(AttackSrcData data)
    {
        srcAS = data.srcAS;
        srcip = data.srcip;
        flow = data.flow;
    }
}

public class IPProxy : MonoBehaviour
{
    public static readonly string fakeTestIp = "89.151.176.13";

    public static readonly string filePath = "UI/Config/IpConfig.json";
    
    private static IPProxy s_instance;

    [HideInInspector]
    public bool isFileReady;

    private Dictionary<string, IpDetail> fadeIpDetailDic;
    //private Dictionary<Vector2Int, IpDetail> m_ipDetailDict = new Dictionary<Vector2Int, IpDetail>();
    //目前只存prefixLen=20的数据
    private Dictionary<string, IpDetail> m_ipDetailCache = new Dictionary<string, IpDetail>();
    private List<IpDetail> m_searchResult = new List<IpDetail>();
    private List<int> m_searchResultASN = new List<int>();
    private List<AttackInfo> m_attackInfo;
    private UnityEvent OnAttackDataReady = new UnityEvent();
    //private bool m_requestedPrefix20;   //最上次IPloading消息是否已发（避免重发）

    public static IPProxy instance
    {
        get
        {
            if(s_instance == null)
            {
                s_instance = Instantiate(new GameObject("IPProxy")).AddComponent<IPProxy>();
            }
            return s_instance;
        }
    }

    private void Awake()
    {
        fadeIpDetailDic = new Dictionary<string, IpDetail>();
        ReadFakeFile();

        s_instance = this;
    }

    public Dictionary<string, IpDetail> GetDictionary()
    {
        return fadeIpDetailDic;
    }

    public IpDetail GetIpDetail(string IP)
    {
        if (fadeIpDetailDic.ContainsKey(IP))
            return fadeIpDetailDic[IP];
        return null;
    }

    private void ReadFakeFile()
    {
        string path = Path.Combine(Application.dataPath, filePath);
        Debug.Log(path);
        StreamReader sr = new StreamReader(path);
        string data = sr.ReadToEnd();

        FakeIPData fakeIPData = JsonUtility.FromJson<FakeIPData>(data);

        Debug.Log(fakeIPData.IPs.Length);

        IpDetail curDetail;
        for (int i = 0, len = fakeIPData.IPs.Length; i < len; i++)
        {
            curDetail = fakeIPData.IPs[i];
            if (fadeIpDetailDic.ContainsKey(curDetail.IP))
            {
                Debug.LogErrorFormat("IP {0} has existed!!", curDetail.IP);
                continue;
            }
            fadeIpDetailDic.Add(curDetail.IP, curDetail);
        }
        
    }

    //-------------------------------- NEW Func--------------------------------------------------
    private Dictionary<IpInfoStructType, string> m_IpInfoTypeDict = new Dictionary<IpInfoStructType, string> {
        { IpInfoStructType.InfoType1, "IPinfotype1"},
        { IpInfoStructType.InfoType2, "IPinfotype2"},
        { IpInfoStructType.InfoType3, "IPinfotype3"},
        { IpInfoStructType.InfoType4, "IPinfotype4"}
    };

    void Update()
    {
        if(ASProxy.instance.OriginalDataReady && m_attackInfo == null)
        {
            StartCoroutine(PrepareAttackData());
        }
    }

    // x ,y 是左上角坐标
    public void GetIpInfoBlock(Action<IpDetail[],IPLayerInfo> action,int prefixLen = 20, int x = -1, int y = -1, string startIp = "")
    {
        MessageRequestIpMap msg = new MessageRequestIpMap();

        if(x == -1 && y == -1)
        {
            x = 0;
            y = 0;
        }

        if(!string.IsNullOrEmpty(startIp))
        {
            if(startIp.Contains("/"))
            {
                msg.startIp = startIp.Substring(0, startIp.IndexOf('/'));
            }
            else
            {
                msg.startIp = startIp;
            }
        }    

        if(prefixLen > 0)
            msg.prefixLen = prefixLen;

        if(msg.prefixLen >= 32)
            msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType3];
        else
            msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType1];

        msg.IPy = y;
        msg.IPx = x;

        IPLayerInfo info = new IPLayerInfo();
        info.prefixLen = prefixLen;
        info.x = x;
        info.y = y;

        NetUtil.Instance.RequestIpMapInfo(msg,info, OnIpInfoResponse, action);
    }


    void OnIpInfoResponse(IpInfoType1[] array, IPLayerInfo info, Action<IpDetail[],IPLayerInfo> action)
    {
        IpDetail[] ipArray = new IpDetail[array.Length];
        for(int i = 0; i < array.Length; i++)
        {
            ipArray[i] = new IpDetail(array[i]);

            if(info.prefixLen == 20 && !m_ipDetailCache.ContainsKey(ipArray[i].IP))
                m_ipDetailCache.Add(ipArray[i].IP, ipArray[i]);
        }

        if(action != null)
        {
            action(ipArray, info);
        }

    }

    public void GetIpInfoByFilter(string keyword)//, Action<IpDetail[]> action)
    {
        MessageRequestIpMapFilter msg = new MessageRequestIpMapFilter();
        msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType2];
        msg.Other = keyword;
        NetUtil.Instance.RequestIpMapFilterInfo(msg, OnIpInfoFilterResponse);//, action);  
    }

    public void RegistAttackDataCallback(UnityAction action)
    {
        OnAttackDataReady.AddListener(action);
    }
    void OnIpInfoFilterResponse(IpInfoType1[] array)//, Action<IpDetail[]> action)
    {
        m_searchResultASN.Clear();
        m_searchResult.Clear();
        for(int i = 0; i < array.Length; i++)
        {
            m_searchResult.Add(new IpDetail(array[i]));
            m_searchResultASN.Add(array[i].ASN);
        }

        EventManager.SendEvent(EventDefine.OnRecieveSearchResult);
    }
    public List<AttackInfo> GetAttackInfo()
    {
        return m_attackInfo;
    }
    IEnumerator PrepareAttackData()
    {
        m_attackInfo = new List<AttackInfo>();
        if(m_ipDetailCache.Count <= 0)
        {
            GetIpInfoBlock(null);

            while(m_ipDetailCache.Count <= 0)
                yield return null;
        }
        
        MessageRequestAttackInfo msg = new MessageRequestAttackInfo();
        NetUtil.Instance.RequestAttackInfo(msg, OnAttackInfoResponse);
        
    }

    void OnAttackInfoResponse(AttackData[] data)
    {
        m_attackInfo.Clear();
        int found = 0;
        for(int i = 0; i < data.Length; i++)
        {
            AttackInfo info = new AttackInfo(data[i]);
            if(info.matchData)
            {
                m_attackInfo.Add(info);
                found += info.srcInfo.Count + 1;
            }
        }

        OnAttackDataReady.Invoke();

        Debug.LogFormat("Got Attack num:  {0}, Valid Num : {1} , IP num : {2} !!!!!!!", data.Length, m_attackInfo.Count, found);
    }

    public IpDetail TryGetIpDetail(string ip, int prefixLen)
    {
        string topip = GetTopIP(ip, prefixLen);
        if(m_ipDetailCache.ContainsKey(topip))
            return m_ipDetailCache[topip];
        else
            return null;
    }

    public bool IsIpExistInLocal(string ip, int prefixLen)
    {
        return m_ipDetailCache.ContainsKey(GetTopIP(ip, prefixLen));
    }

    string GetTopIP(string ip, int preLen)
    {
        int cha = preLen - 20;
        char[] spliter = {'.'};
        string[] strp = ip.Split(spliter);
        int[] parts = new int[strp.Length];
        for(int i = strp.Length - 1; i >= 0; i--)
        {
            parts[i] = int.Parse(strp[i]);
            if(cha > 0)
            {
                int n = cha > 8 ? 8 : cha;
                parts[i] = parts[i] >> n;
                parts[i] = parts[i] << n;
                cha -= n;
            }
        }

        return string.Format("{0}.{1}.{2}.{3}", parts[0], parts[1], parts[2], parts[3]);
    }

    MessageRequestIpMap GetBaseMessage()
    {
        MessageRequestIpMap msg = new MessageRequestIpMap();
        msg.prefixLen = 32;
        msg.xLen = 1;
        msg.yLen = 1;
        msg.type = "IPinfotype4";
        return msg;
    }

    public void ClearSearchResult()
    {
        m_searchResult.Clear();
        m_searchResultASN.Clear();

        EventManager.SendEvent(EventDefine.OnClearSearchResult);
    }

    public IpDetail[] GetSearchResult()
    {
        return m_searchResult.ToArray();
    }

    public bool IsASInSearchResult(int ASN)
    {
        return m_searchResultASN.Contains(ASN);
    }

    // public void GetIpInfoBlock<T>(int prefixLen, string startIp, int xlen, int ylen, IpInfoStructType type, Action<T[]> action)
    // {
    //     MessageRequestIpMap msg = new MessageRequestIpMap();
    //     msg.xLen = xlen;
    //     msg.yLen = ylen;
    //     msg.startIp = startIp;
    //     msg.prefixLen = prefixLen;
    //     msg.type = m_IpInfoTypeDict[type];

    //     NetUtil.Instance.RequestIpMapInfo(msg, OnIpInfoResponse, action);
    // }

    
}
