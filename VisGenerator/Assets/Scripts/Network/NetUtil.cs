#define NET_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using UnityEngine.Events;
using Newtonsoft.Json;

public class NetUtil : MonoBehaviour
{
    static NetUtil instance;
	public static NetUtil Instance {
		get {
			if (instance == null) {
				GameObject mounter = new GameObject("NetUtil");
				instance = mounter.AddComponent<NetUtil>();
			}
			return instance;
		}
	}

    private string m_baseAdress = "http://166.111.9.83:28280/";
    private Dictionary<NetMessageType, string> m_meessageKeywords = new Dictionary<NetMessageType, string> {
        { NetMessageType.IpMapLoad, "IPMAP_Loading"},
        { NetMessageType.IpMapFilter, "IPMAP_Query"},
        { NetMessageType.ASMapLoad, "ASMAP_Loading"},
        { NetMessageType.ASMapFilter, "ASMAP_Query"}
    };

    void FakeResponse(string data, Action<IpMapResponse, Action> action, Action action2)
    {
        string path = Application.dataPath + data;//Path.Combine(Application.dataPath, data);
        Debug.Log(Application.dataPath + " ?? " + path);
        StreamReader sr = new StreamReader(path);
        string rdata = sr.ReadToEnd();

        IpMapResponse response = JsonUtility.FromJson<IpMapResponse>(rdata);//(uwr.downloadHandler.text);

        sr.Close();
        sr.Dispose();

        action(response, action2);
    }

    /// <summary>
    /// 请求IP地图信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestIpMapInfo(MessageRequestIpMap msg, Action<string, Action<IpInfoType1[]>> action, Action<IpInfoType1[]> action2)
    {
        StartCoroutine(_RequestIpMapInfo(msg.GetParamString(), action, action2));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestIpMapInfo(string urlParam, Action<string, Action<IpInfoType1[]>> action, Action<IpInfoType1[]> action2)
    {
// #if NET_DEBUG
//         //FakeResponse("/Scripts/Network/AS_2.json", action, action2);
//         yield return new WaitForEndOfFrame();
// #else
        StringBuilder sb = new StringBuilder(m_baseAdress);
        sb.Append(m_meessageKeywords[NetMessageType.IpMapLoad]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        yield return uwr.SendWebRequest();
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            //IpMapResponse response = JsonUtility.FromJson<IpMapResponse>(uwr.downloadHandler.text);
            action(uwr.downloadHandler.text, action2);
        }

        uwr.Dispose();
//#endif
    }

    /// <summary>
    /// 请求IP地图Filter信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestIpMapFilterInfo(MessageRequestIpMapFilter msg, Action<string, Action<IpInfoType1[]>> action, Action<IpInfoType1[]> action2)
    {
        StartCoroutine(_RequestIpMapFilterInfo(msg.GetParamString(), action, action2));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestIpMapFilterInfo(string urlParam,Action<string, Action<IpInfoType1[]>> action, Action<IpInfoType1[]> action2)
    {
#if NET_DEBUG

        //FakeResponse("/Scripts/Network/AS_2.json", action);
        yield return new WaitForEndOfFrame();

#else
        
        StringBuilder sb = new StringBuilder(m_baseAdress);
        sb.Append(m_meessageKeywords[NetMessageType.IpMapFilter]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        yield return uwr.SendWebRequest();
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            action(uwr.downloadHandler.text, action2);
        }
        uwr.Dispose();
#endif
    }

    /// <summary>
    /// 请求AS地图信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestASMapInfo(MessageRequestASMap msg,Action<ASMapResponse> action)
    {
        StartCoroutine(_RequestASMapInfo(msg.GetParamString(), action));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestASMapInfo(string urlParam, Action<ASMapResponse> action)
    {
#if NET_DEBUG
        FakeASResponse("/Scripts/Network/AS.json", action);
        yield return new WaitForEndOfFrame();
#else
        
        StringBuilder sb = new StringBuilder(m_baseAdress);
        sb.Append(m_meessageKeywords[NetMessageType.ASMapLoad]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        yield return uwr.SendWebRequest();
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            ASMapResponse response = JsonUtility.FromJson<ASMapResponse>(uwr.downloadHandler.text);
            
            action(response);
        }

#endif
    }

    void FakeASResponse(string filepath, Action<ASMapResponse> action)
    {
        string path = Application.dataPath + filepath;//Path.Combine(Application.dataPath, data);
        Debug.Log(path);

        ASMapResponse response = new ASMapResponse();
        response.Status = 0;

        StreamReader sr = new StreamReader(path);

        int count = (int)Mathf.Pow(2, 16);
        int i = 0;
        List<ASInfo> list = new List<ASInfo>();
        string rdata = sr.ReadLine();
        while(!string.IsNullOrEmpty(rdata) && i < count)
        {
            ASInfo asinfo = JsonConvert.DeserializeObject<ASInfo>(rdata);
            asinfo.Transfer();
            list.Add(asinfo);
            
            rdata = sr.ReadLine();
            i++;
        }

        response.Result = list.ToArray();

        sr.Close();
        sr.Dispose();

        action(response);
    }

    /// <summary>
    /// 请求AS具体层信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestASSegmentsInfo(MessageRequestASSegments msg,Action<ASSegmentsResponse> action)
    {
        StartCoroutine(_RequestASSegmentsInfo(msg.GetParamString(), action));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestASSegmentsInfo(string urlParam, Action<ASSegmentsResponse> action)
    {
#if NET_DEBUG
        //FakeASResponse("/Scripts/Network/AS.json", action);
        yield return new WaitForEndOfFrame();
#else
        
        StringBuilder sb = new StringBuilder(m_baseAdress);
        sb.Append(m_meessageKeywords[NetMessageType.ASMapLoad]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        yield return uwr.SendWebRequest();
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            ASSegmentsResponse response = JsonUtility.FromJson<ASSegmentsResponse>(uwr.downloadHandler.text);
            
            action(response);
        }

#endif
    }
}
