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
    ASMapFilter,
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

[Serializable]
public class IpPortInfo
{
    public int PortID;
    public string PortWork;
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
    public string type = "IPinfotype1";     //请求IP信息基本数据的具体级别 例如：“IPinfotype1”

    public virtual string GetParamString()
    {
        xLen = (xLen == 0) ? (int)Mathf.Pow(2, prefixLen/2) : xLen;
        yLen = (yLen == 0) ? (int)Mathf.Pow(2, prefixLen/2) : yLen;

        return string.Format("/{0}/{1}/{2}/{3}/{4}", prefixLen, startIp, xLen, yLen, type);
    }
}

/// <summary>
/// 请求IP地图 - 返回消息数据结构
/// </summary>
[Serializable]
public class IpMapResponse
{
    public int Status;
    public IpInfoType1[] Result;
}

public class IpMapResponseType2
{
    public int Status;
    public IpInfoType2[] Result;
}

public class IpMapResponseType3
{
    public int Status;
    public IpInfoType3[] Result;
}
public class IpMapResponseType4
{
    public int Status;
    public IpInfoType4[] Result;
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
        
        return string.Format("/{0}/{1}/{2}/{3}/{4}", ASN > 0 ? ASN.ToString() : "None", ISP, Country, Province,Other);
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
    public int startASN = 1; //左上角AS号
    public int xLen = (int)Mathf.Pow(2, 8);        //显示区域的横向大小
    public int yLen = (int)Mathf.Pow(2, 8);         //显示区域的纵向大小
    public string type = "ASinfotype";     //请求AS基本数据类型 例如“ASinfotype”

    public virtual string GetParamString()
    {
        return string.Format("/{0}/{1}/{2}/{3}", startASN, xLen, yLen, type);
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
        return string.Format("/{0}/{1}/{2}/{3}", ASN, HeadIp, TailIp, type);
    }
}
