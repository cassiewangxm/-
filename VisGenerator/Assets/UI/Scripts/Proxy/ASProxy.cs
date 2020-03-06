using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class ASInfo
{
    public string ASN;       //AS号
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
        if(IP_segments != null)
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
        //Debug.Log("Fake ASInfo ...... ");//+x + ","+y);
        ASN = UnityEngine.Random.Range(1,99999).ToString();
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

    private string[] IPList;

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
        Time = "1990";//UnityEngine.Random.Range(1990,2019).ToString();
        IPCount = UnityEngine.Random.Range(99,233);
        IPList = new string[IPCount];
        for(int i =0; i<IPList.Length; i++)
        {
            IPList[i] = UnityEngine.Random.Range(16,255).ToString()+"."+UnityEngine.Random.Range(0,155).ToString()+"."+UnityEngine.Random.Range(0,133).ToString()+"."+UnityEngine.Random.Range(0,255).ToString();;
        }
    }

    public Color GetIPColor(int index)
    {
        return UnityEngine.Random.ColorHSV(0,1,0,1,0,1,0.5f,1);
        //return Color.black;
    }

    public float GetRadius()
    {
        return Mathf.Min((float)IPCount/256*2 + 2, 6);
    }
}

public class ASProxy : MonoBehaviour
{
    private Dictionary<Vector2,ASInfo> m_ASDict = new Dictionary<Vector2, ASInfo>();

    private static ASProxy s_instance;
    public static ASProxy instance
    {
        get {
            if(s_instance == null)
            {
                s_instance = new GameObject("ASProxy").AddComponent<ASProxy>();
            }
            return s_instance;
        }
    }
    public void GetASInfoAll()
    {
        if(m_ASDict.Count > Mathf.Pow(2,8))
            return;

        NetUtil.Instance.RequestASMapInfo(new MessageRequestASMap(), OnReciveASInfo);
    }

    void OnReciveASInfo(ASMapResponse response)
    {
        if(response.Status == 0)
        {
            for(int i = 0; i < response.Result.Length; i++)
            {
                m_ASDict.Add(new Vector2Int(response.Result[i].X, response.Result[i].Y), response.Result[i]);
            }

            Debug.Log("Finished AS info : "+m_ASDict.Count);
        }
        else
        {
            Debug.LogError("RequestASMapInfo get error : " + response.Status);
        }
    }

    // TEST --------
    public ASInfo GetASByPosition(int x, int y, float height)
    {
        Vector2 v = new Vector2(x,y);
        if(m_ASDict.ContainsKey(v))
        {
            //Debug.Log("Found AS : ");
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
        return m_ASDict.ContainsKey(new Vector2(x,y));
    }
}
