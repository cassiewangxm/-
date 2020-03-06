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
/// <summary>
/// AS IP块  - 返回消息数据结构
/// </summary>
[Serializable]
public class ASSegmentsResponse
{
    public int Status;
}
