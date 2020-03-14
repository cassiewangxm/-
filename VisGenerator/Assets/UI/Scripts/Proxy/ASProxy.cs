﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class ASInfo
{
    public int ASN;       //AS号
    public int X;      //ASN Hilbert横坐标
    public int Y;      //ASN Hilbert纵坐标
    public string org;      //分配该AS的组织
    public Dictionary<string, string[]> IP_segments;
    public string ISP;      //AS所属的网络服务提供商
    public string ISP_country_code;     //网络服务提供商所在国家
    public ASSegmentInfo[] ASSegment;   //ASN 下IP地址分配数组
    public int Height = 1;

    public void Transfer()
    {
        if(IP_segments != null && ASSegment == null)
        {
            int i = 0;
            ASSegment = new ASSegmentInfo[IP_segments.Count];
            foreach(var pair in IP_segments)
            {
                ASSegment[i++] = new ASSegmentInfo(pair.Key, pair.Value);
            }

            Height = IP_segments.Count * 2;
        }
    }

    // TEST ----
    public void FakeInit(int x, int y, int segNum)
    {
        Debug.Log("Fake ASInfo ...... ");//+x + ","+y);
        ASN = UnityEngine.Random.Range(1,99999);
        Y = y;
        X = x;

        org = "name";
        
        segNum = segNum > 0 ? segNum : 1;

        if(segNum > 0)
        {
            ASSegment = new ASSegmentInfo[segNum];
            for(int i = 0; i < segNum; i++)
            {
                ASSegment[i] = new ASSegmentInfo();
            }

            Height = segNum * 2;
        }
    }
}

#if NOT_DEBUG
[Serializable]
#endif
public class ASSegmentInfo
{
    public string HeadIP;   //起始IP
    public string TailIP;   //终止IP 
    public int IPCount;     //IP数量
    public string Time;     //分配时间
    //public string[] IPList;

    private Dictionary<int, Color> IPColorDict = new Dictionary<int, Color>();

    public ASSegmentInfo(string time, string[] other)
    {
        Time = time;

        if(other.Length < 3)
        {
            Debug.Log("Segment info form error");
            return;
        }

        HeadIP = other[0];
        TailIP = other[1];
        IPCount = int.Parse(other[2]);
    }
    public ASSegmentInfo()
    {
        FakeInit();
    }

    //制造假数据
    void FakeInit()
    {
        HeadIP = "0.0.0.0";
        TailIP = "99.88.77.66";
        Time = "1990";//UnityEngine.Random.Range(1990,2019).ToString();
        IPCount = UnityEngine.Random.Range(99,233);
        // IPList = new string[IPCount];
        // for(int i =0; i<IPList.Length; i++)
        // {
        //     IPList[i] = UnityEngine.Random.Range(16,255).ToString()+"."+UnityEngine.Random.Range(0,155).ToString()+"."+UnityEngine.Random.Range(0,133).ToString()+"."+UnityEngine.Random.Range(0,255).ToString();;
        // }
    }

    public Color GetIPColor(int index)
    {
        if(!IPColorDict.ContainsKey(index))
            IPColorDict.Add(index, UnityEngine.Random.ColorHSV(0,1,0,1,0,1,0.5f,1));
        
        return IPColorDict[index];//UnityEngine.Random.ColorHSV(0,1,0,1,0,1,0.5f,1);
        //return Color.black;
    }

    public string GetIpStringByIndex(int index)
    {
        string[] heads = HeadIP.Split('.');
        string[] tails = TailIP.Split('.');
        int[] ips = new int[heads.Length];
        for(int i = 0; i < heads.Length ; i++)
        {
            if(heads[i].CompareTo(tails[i]) != 0)
            {
                int head = int.Parse(heads[i]);
                int tail = int.Parse(tails[i]);
                ips[i] = head + index/(int)Mathf.Pow(255, 3-i);
            }
            else
            {
                ips[i] = int.Parse(heads[i]);
            }
        }

        return string.Format("{0}.{1}.{2}.{3}",ips[0],ips[1],ips[2],ips[3]);
    }

    public float GetRadius()
    {
        return Mathf.Min((float)IPCount/256*2 + 2, 6);
    }
}

public class ASProxy : MonoBehaviour
{
    private Dictionary<Vector2,ASInfo> m_ASDict = new Dictionary<Vector2, ASInfo>();
    //(x,y) : x AS号，y 层序号
    private Dictionary<Vector2Int,IpDetail[]> m_SegmentCache = new Dictionary<Vector2Int, IpDetail[]>();

    private static ASProxy s_instance;
    public static ASProxy instance
    {
        get {
            if(s_instance == null)
            {
                s_instance = new GameObject("ASProxy").AddComponent<ASProxy>();

                GameObject parent = GameObject.Find("Singleton");
                if (parent != null)
                    s_instance.transform.parent = parent.transform;
            }
            return s_instance;
        }
    }
    public int HeightMax    {   get {return m_heightMax;}   }
    public bool OriginalDataReady {get {return m_originalDataReady;}}
    private bool m_originalDataReady;
    private int m_heightMax;
    public void GetASAreaInfo(int asN, int xlen, int ylen, Action action)
    {
        MessageRequestASMap msg = new MessageRequestASMap();
        msg.startASN = asN;
        msg.xLen = xlen;
        msg.yLen = ylen;
        NetUtil.Instance.RequestASMapInfo(msg, OnReciveASInfo, action);
    }
    void OnReciveASInfo(ASInfo[] response, Action action)
    {
        // for(int i = 0; i < response.Length; i++)
        // {
        //     response[i].Transfer();
        //     m_ASDict.Add(new Vector2Int(response[i].X, response[i].Y), response[i]);
        // }

        // m_originalDataReady = true;

        if(action != null)
            action();

    }
    public void GetASInfoOriginal(Action action)
    {
        if(m_ASDict.Count > Mathf.Pow(2,8))
            return;

        MessageRequestASMap msg = new MessageRequestASMap();
        NetUtil.Instance.RequestASMapInfo(msg, OnRecieveOriginalInfo, action);
    }


    void OnRecieveOriginalInfo(ASInfo[] response, Action action)
    {
        for(int i = 0; i < response.Length; i++)
        {
            if(i==0 || i == response.Length - 1)
            {
                Debug.Log(response[i].X + " , " + response[i].X );
            }
            if(response[i].IP_segments.Count > 0)
            {
                response[i].Transfer();
                m_heightMax = Mathf.Max(m_heightMax, response[i].Height);
                m_ASDict.Add(new Vector2Int(response[i].X, response[i].Y), response[i]);
            }
        }
        
        Debug.LogFormat("Get AS Count : {0}/{1}, Max height : {2}", response.Length,  m_ASDict.Count, m_heightMax);

        m_originalDataReady = true;
    }

    // TEST --------
    public ASInfo GetASByPosition(int x, int y, float height)
    {
        Vector2 v = new Vector2(x,y);
        if(m_ASDict.ContainsKey(v))
        {
            Debug.Log("Found AS : ");
            return m_ASDict[v];
        }    
        else
        {
            ASInfo asDetail = new ASInfo();
            asDetail.FakeInit(x,y,(int)height);
            m_ASDict.Add(v,asDetail);

            return asDetail;
        }
    }

    public ASInfo GetASByPosition(int x, int y)
    {
        Vector2 v = new Vector2(x,y);
        if(m_ASDict.ContainsKey(v))
        {
            return m_ASDict[v];
        }    
        else
        {
            return null;
        }
    }

    public bool IsASExistInLocal(int x, int y)
    {
        Debug.LogFormat("AS ({0},{1}) exist ? : {2}",x,y, m_ASDict.ContainsKey(new Vector2(x,y)));
        return m_ASDict.ContainsKey(new Vector2(x,y));
    }

    //获取 某坐标的AS柱体某一层的某个IP信息
    public void GetASSegmentIPInfo(Vector2 asPos, int segmentIndex, int ipIndex, Action<IpDetail> action)
    {
        if(m_ASDict.ContainsKey(asPos))
        {
            ASInfo asinfo = m_ASDict[asPos];
            Vector2Int p = new Vector2Int(asinfo.ASN, segmentIndex);
            if(m_SegmentCache.ContainsKey(p))
            {
                if(ipIndex < m_SegmentCache[p].Length)
                {
                    action(m_SegmentCache[p][ipIndex]);
                }
                else
                {
                    Debug.LogErrorFormat("IpIndex overflow : {0},{1},{2}" ,p.x, p.y, ipIndex);
                }
            }
            else
            {
                if(segmentIndex < asinfo.ASSegment.Length)
                {
                    ASSegmentInfo segInfo = asinfo.ASSegment[segmentIndex];
                    MessageRequestASSegments msg = new MessageRequestASSegments();
                    msg.ASN = asinfo.ASN;
                    msg.HeadIp = segInfo.GetIpStringByIndex(ipIndex);//segInfo.HeadIP;
                    msg.TailIp = msg.HeadIp;
                    msg.type = "IPinfotype4";

                    NetUtil.Instance.RequestASSegmentsInfo(msg, OnRecieveSegmentInfo, new Vector3Int(asinfo.ASN, segmentIndex, ipIndex), action);
                    return;
                }
                else
                {
                    Debug.LogError("segmentIndex overflow : " + segmentIndex);
                }
            }
        }
        else
        {
            Debug.LogError("AS form location : " + asPos + " not found");
        }

        action(null);
    }


    void OnRecieveSegmentInfo(IpInfoType1[] array, Vector3Int key, Action<IpDetail> action)
    {
        ManageCacheSize();
        
        IpDetail[] ips = new IpDetail[array.Length];
        for(int i = 0; i < array.Length; i++)
        {
            ips[i] = new IpDetail(array[i]);
        }

        Vector2Int vi = new Vector2Int(key.x, key.y);
        if(!m_SegmentCache.ContainsKey(vi))
        {
            m_SegmentCache.Add(vi, ips);
            Debug.LogFormat("Get {0},{1} : {2}", key.x, key.y, ips.Length);
        }

        if(action == null)
            return;

        if(key.z < ips.Length)
            action(ips[key.z]);
        else
            action(null);
    }

    void ManageCacheSize()
    {
        if(m_SegmentCache.Count > 20)
        {
            m_SegmentCache.Clear();
        }
    }
}
