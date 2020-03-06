using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class IpInfoType1
{
    public string IP;   //IP地址前缀
    public int CoordX;  //IP地址前缀Hilbert横坐标
    public int CoordY;   //IP地址前缀Hilbert纵坐标
    public float Lat;   //IP地址前缀所属经度
    public float Lng;   //IP地址前缀所属纬度
    public string ASN;  //所属AS的AS号
    public string Continent;    //所在大洲
    public string Country_name; //所在国家
    public string Country_code; //所在国家代号
    public string Province;     //所在州/省
    public string City;         //所在城市
    public string Time_zone;    //所在时区
    public string Post_code;    //邮政编码
}

[Serializable]
public class IpInfoType2
{
    public string IP;
    public int CoordX;
    public int CoordY;
    public string ASN;
    public string ISP;      //AS所属的网络服务提供商
    public string ISP_country_code; //网络服务提供商所在国家
    public string Org;      //分配该AS的组织
    public string Time;     //分配该IP到此AS的时间（日期）
}

[Serializable]
public class IpInfoType3
{
    public string IP;
    public int CoordX;
    public int CoordY;
    public string Device;       //IP地址设备类型
    public string OS;           //IP地址操作系统
    public IpPortInfo[] Port;   // 开放的端口及服务
}

[Serializable]
public class IpInfoType4
{
    public string IP;
    public float Lat;
    public float Lng;
    public string Continent;
    public string Country_name;
    public string Country_code;
    public string Province;
    public string City;
    public string Time_zone;
    public string Post_code;
    public string Time;
}   

public class IpPortInfo
{
    public int PortID;
    public string PortWork;
}

// ------------------------------------------------------------------------------

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

    public void GetIpInfoBlockType1(Action<IpInfoType1[]> action, int prefixLen = -1, string startIp = null, int xlen = -1, int ylen = -1)
    {
        MessageRequestIpMap msg = new MessageRequestIpMap();
        if(xlen > 0)
            msg.xLen = xlen;
        if(ylen > 0)
            msg.yLen = ylen;
        if(!string.IsNullOrEmpty(startIp))
            msg.startIp = startIp;
        if(prefixLen > 0)
            msg.prefixLen = prefixLen;

        msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType1];

        NetUtil.Instance.RequestIpMapInfo(msg, OnIpInfoResponse, action);
    }

    public void GetIpInfoFilterType1(Action<IpInfoType1[]> action, MessageRequestIpMapFilter msg)
    {
        msg.type = m_IpInfoTypeDict[IpInfoStructType.InfoType1];
        NetUtil.Instance.RequestIpMapFilterInfo(msg, OnIpInfoFilterResponse, action);  
    }

    void OnIpInfoResponse(string data, Action<IpInfoType1[]> action)
    {
        IpMapResponse response = JsonUtility.FromJson<IpMapResponse>(data);
        if(response.Status != 0)
        {
            Debug.LogError("IpMapLoad Response error code : " + response.Status);
            return;
        }
        if(action != null)
        {
            action(response.Result);
        }
    }
    void OnIpInfoFilterResponse(string data, Action<IpInfoType1[]> action)
    {
        IpMapFilterResponse response = JsonUtility.FromJson<IpMapFilterResponse>(data);
        if(response.Status != 0)
        {
            Debug.LogError("IpMapFilter Response error code : " + response.Status);
            return;
        }
        if(action != null)
        {
            action(response.Result);
        }
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
