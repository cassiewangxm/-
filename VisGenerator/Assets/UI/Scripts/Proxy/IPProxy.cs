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

    public IpDetail GetIpDetail(string ip)
    {
        if (fadeIpDetailDic.ContainsKey(ip))
            return fadeIpDetailDic[ip];
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
                Debug.LogErrorFormat("ip {0} has existed!!", curDetail.IP);
                continue;
            }
            fadeIpDetailDic.Add(curDetail.IP, curDetail);
        }
        
    }
}
