using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class IpDetail
{
    public string IP;
    public uint ASNum;
    public string IPParent;
    public string country;
    public string name;
    public float lat;
    public float lng;
    public bool isBanned;
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
        IP = info.ip_prefix;
        X = info.X;
        Y = info.Y;
        lat = info.latitude;
        lng = info.longitude;
        ASNum = (uint)info.ASN;
    }

    public static readonly string DEFAULT_IP = "0.0.0.0";
}

[Serializable]
public class FakeIPData
{
   public IpDetail[] IPs;
}

public class IPProxy : MonoBehaviour
{
    public static readonly string fakeTestIp = "89.151.176.13";

    public static readonly string filePath = "UI/Config/IpConfig.json";
    private static IPProxy s_instance;

    [HideInInspector]
    public bool isFileReady;

    private Dictionary<string, IpDetail> fadeIpDetailDic;
    private Dictionary<Vector2Int, IpDetail> m_ipDetailDict = new Dictionary<Vector2Int, IpDetail>();
    private IpDetail[] m_searchResult;

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

    // x ,y 是左上角坐标
    public void GetIpInfoBlock(Action<IpDetail[],MessageRequestIpMap> action,int prefixLen = 20, int x = -1, int y = -1)
    {
        MessageRequestIpMap msg = new MessageRequestIpMap();

        Vector2Int key = new Vector2Int(x,y);
        if(m_ipDetailDict.ContainsKey(key))
        {
            string[] strs = m_ipDetailDict[key].IP.Split('/');
            if(strs != null && strs.Length > 0)
                msg.startIp = strs[0];
        }
        else if(x != -1 || y != -1)
        {
            Debug.LogErrorFormat("Invalid IP pos : {0},{1}", x, y);
        }

        if(prefixLen > 0)
            msg.prefixLen = prefixLen;

        if(msg.prefixLen >= 32)
            msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType3];
        else
            msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType1];

        //msg.startIp = "166.111.9.83";

        NetUtil.Instance.RequestIpMapInfo(msg, OnIpInfoResponse, action);
    }


    void OnIpInfoResponse(IpInfoType1[] array, MessageRequestIpMap message, Action<IpDetail[],MessageRequestIpMap> action)
    {
        if(m_ipDetailDict == null)
            m_ipDetailDict = new Dictionary<Vector2Int, IpDetail>();
        m_ipDetailDict.Clear();

        IpDetail[] ipArray = new IpDetail[array.Length];
        for(int i = 0; i < array.Length; i++)
        {
            ipArray[i] = new IpDetail(array[i]);
            m_ipDetailDict.Add(new Vector2Int(array[i].X, array[i].Y), ipArray[i]);
        }
        if(action != null)
        {
            action(ipArray, message);
        }
    }

    public void GetIpInfoByFilter(string keyword)//, Action<IpDetail[]> action)
    {
        MessageRequestIpMapFilter msg = new MessageRequestIpMapFilter();
        msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType1];
        msg.Other = keyword;
        NetUtil.Instance.RequestIpMapFilterInfo(msg, OnIpInfoFilterResponse);//, action);  
    }

    void OnIpInfoFilterResponse(IpInfoType1[] array)//, Action<IpDetail[]> action)
    {
        m_searchResult = null;
        m_searchResult = new IpDetail[array.Length];
        for(int i = 0; i < array.Length; i++)
        {
            m_searchResult[i] = new IpDetail(array[i]);
        }

        EventManager.SendEvent(EventDefine.OnRecieveSearchResult);
    }

    public IpDetail[] GetSearchResult()
    {
        return m_searchResult;
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
