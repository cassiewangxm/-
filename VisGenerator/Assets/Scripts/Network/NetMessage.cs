using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngineInternal;
using System;
using System.Text;

public enum NetMessageType
{
    IpMapLoad,
    IpMapFilter,
    ASMapLoad,
    ASMapQuery,
    AttackLoad,
    Count,
}

public enum IpInfoStructType
{
    InfoType1,
    InfoType2,
    InfoType3,
    InfoType4
}


[Serializable]
public class IpInfoType1
{
    public string ip_prefix;   //IP地址前缀
    public string ip;
    public int X;  //IP地址前缀Hilbert横坐标
    public int Y;   //IP地址前缀Hilbert纵坐标
    public float latitude;   //IP地址前缀所属经度
    public float longitude;   //IP地址前缀所属纬度
    public int ASN;  //所属AS的AS号
    public string continent;    //所在大洲
    public string country_name; //所在国家
    public string country_code; //所在国家代号
    public string provience;     //所在州/省
    public string city;         //所在城市
    public string time_zone;    //所在时区
    public string post_code;    //邮政编码

    //type 2
    public string ISP;      //AS所属的网络服务提供商
    public string ISP_country_code; //网络服务提供商所在国家
    public string org;      //分配该AS的组织
    public string time;     //分配该IP到此AS的时间（日期）

    //type 3
    public string device;       //IP地址设备类型
    public string os;           //IP地址操作系统
    //public IpPortInfo[] port;   // 开放的端口及服务
}

[Serializable]
public class IpPortInfo
{
    public int PortID;
    public string PortWork;
}

[Serializable]
public class AttackData
{
    public string time; //攻击开始的时间戳
    public string desip;
    public int desAS;
    public int attackTime;  //攻击持续的时间，单位 - 秒
    public AttackPortData portInfo;
    public AttackSrcData[] srcInfo; //攻击源信息

    public Color GetColor()
    {
        return  AttackColorMap.GetColor(portInfo.attackKind, portInfo.attackType);
    }
}

[Serializable]
public class AttackSrcData
{
    public string srcip;
    public int srcAS;
    public int flow;    //该ip地址发送的流的个数
}

[Serializable]
public class AttackPortData
{
    public string attackKind; //攻击类型
    public string attackType; //攻击类型的子类型
    public int port;    //端口号
}

public class AttackColorMap
{
    private static Dictionary<string,Color> colorMap = new Dictionary<string, Color>();
    private static Dictionary<string,float> colorLayer = new Dictionary<string, float>();

    public static Color GetColor(string kind, string type)
    {
        Color basecolor = Color.blue;
        if(colorMap.Count == 0)
        {
            colorMap.Add("web",Color.red);
            colorMap.Add("email",Color.blue);
            colorMap.Add("management",Color.green);
            colorMap.Add("database",Color.yellow);
            colorMap.Add("infrastructure",Color.white);
        }

        if(colorMap.ContainsKey(kind))
            basecolor = colorMap[kind];

        if(colorLayer.Count == 0)
        {
            colorLayer.Add("http",1.0f);
            colorLayer.Add("https",0.8f);

            colorLayer.Add("smtp",1.0f);
            colorLayer.Add("imap",0.8f);
            colorLayer.Add("pop3",0.6f);

            colorLayer.Add("ssh",1.0f);
            colorLayer.Add("telnet",0.8f);

            colorLayer.Add("mysql",1.0f);
            colorLayer.Add("oracle",0.8f);
            colorLayer.Add("mongodb",0.6f);

            colorLayer.Add("dns",1.0f);
            colorLayer.Add("ftp",0.8f);
            colorLayer.Add("rpc",0.6f);
        }

        if(colorLayer.ContainsKey(type))
            basecolor *= colorLayer[type];

        basecolor.a = 1.0f;

        return basecolor;
    }
}

/// <summary>
/// 请求IP地图 - 参数数据结构
/// </summary>
public class MessageRequestIpMap 
{
    public int prefixLen = 20;   //当前IP地图层级的前缀长度
    public string startIp = "0.0.0.0";  //需要显示的左上角IP地址
    public int xLen;        //显示区域的横向大小
    public int yLen;        //显示区域的纵向大小
    public string type = "[IPinfotype1]";     //请求IP信息基本数据的具体级别 例如：“IPinfotype1”
    public int IPx;
    public int IPy;

    public virtual string GetParamString()
    {
        xLen = (xLen == 0) ? (int)Mathf.Pow(2, prefixLen/2) : xLen;
        yLen = (yLen == 0) ? (int)Mathf.Pow(2, prefixLen/2) : yLen;

        //return string.Format("/PrefixLen={0},startIP={1},xLen={2},yLen={3},type={4},IPx={5},IPy={6}", prefixLen, startIp, xLen, yLen, type, IPx, IPy);
        return string.Format("/PrefixLen={0},xLen={1},yLen={2},type={3},IPx={4},IPy={5}", prefixLen, xLen, yLen, type, IPx, IPy);
    }
}

/// <summary>
/// 在IP地图上Filter - 参数数据结构
/// </summary>
[Serializable]
public class MessageRequestIpMapFilter : MessageRequestIpMap
{
    public int ASN;         //检索AS编号
    public string ISP = "None";      //检索ISP编号
    public string Country = "None";  //检索国家
    public string Province = "None"; //检索省份
    public string Other = "None";    //支持模糊查询

    public override string GetParamString()
    {
        if(string.Compare(ISP, "None") != 0)
            ISP = ISP.Replace(' ', '_');

        if(string.Compare(Country, "None") != 0)
            Country = Country.Replace(' ', '_');

        if(string.Compare(Province, "None") != 0)
            Province = Province.Replace(' ', '_');

        if(string.Compare(Other, "None") != 0)
            Other = Other.Replace(' ', '_');

        return string.Format("{0},ASN={1},ISP={2},Country={3},Province={4},Other={5}", base.GetParamString(), ASN > 0 ? ASN.ToString() : "None", ISP, Country, Province,Other);
    }
}

/// <summary>
/// IP地图Filter - 返回消息数据结构
/// </summary>
[Serializable]
public class IpMapFilterResponse
{
    public int Status;
    public IpInfoType1[] Result;
}

/// <summary>
/// 请求AS地图 - 参数数据结构
/// </summary>
public class MessageRequestASMap 
{
    public int startASN = 2; //左上角AS号
    public int xLen = (int)Mathf.Pow(2, 8);        //显示区域的横向大小
    public int yLen = (int)Mathf.Pow(2, 8);        //显示区域的纵向大小
    public string type = "ASinfotype";     //请求AS基本数据类型 例如“ASinfotype”

    public virtual string GetParamString()
    {
        //http://166.111.9.83:3000/ASMAP_Loading/StartASN=48,xLen=21,yLen=21,type=ASinfotype
        
        //http://166.111.9.83:3000/ASMAP_Loading/StartASN=1,xLen=8,yLen=8,type=ASinfotype

        return string.Format("/StartASN={0},xLen={1},yLen={2},type={3}", startASN, xLen, yLen, type);
    }
}

/// <summary>
/// AS地图 - 返回消息数据结构
/// </summary>
[Serializable]
public class ASMapResponse
{
    public int Status;
    public ASInfo[] Result;
}

/// <summary>
/// 请求AS IP块 - 参数数据结构
/// </summary>
public class MessageRequestASSegments
{
    public int ASN = 1; //AS号
    public string HeadIp = "";        
    public string TailIp = "";   
    public string type = "IPinfotype4";     //请求AS基本数据类型 例如“ASinfotype”

    public virtual string GetParamString()
    {
        //http://166.111.9.83:3000/ASMAP_Query/ASN=2,HeadIP=91.143.144.0,TailIP=91.143.144.25,type=IPinfotype4
        
        return string.Format("/ASN={0},HeadIP={1},TailIP={2},type={3}", ASN, HeadIp, TailIp, type);
    }
}

public class MessageRequestAttackInfo
{
    public virtual string GetParamString()
    {
        //http://166.111.9.83:3000/Attack_Loading/attackinfotype
        
        return "/attackinfotype";
    }
}
