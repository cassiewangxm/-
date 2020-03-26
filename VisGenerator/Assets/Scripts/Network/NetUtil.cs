﻿//#define NET_DEBUG

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

                GameObject parent = GameObject.Find("Singleton");
                if (parent != null)
                    mounter.transform.parent = parent.transform;

				instance = mounter.AddComponent<NetUtil>();
			}
			return instance;
		}
	}

    private string m_baseAdressIP = "http://166.111.9.83:28280/";
    private string m_baseAdressAS = "http://166.111.9.83:3000/";
    private string m_baseAdressASQuery = "http://166.111.9.83:3001/";
    
    private Dictionary<NetMessageType, string> m_meessageKeywords = new Dictionary<NetMessageType, string> {
        { NetMessageType.IpMapLoad, "IPMAP_Loading"},
        { NetMessageType.IpMapFilter, "IPMAP_Query"},
        { NetMessageType.ASMapLoad, "ASMAP_Loading"},
        { NetMessageType.ASMapQuery, "ASMAP_Query"},
        { NetMessageType.AttackLoad, "Attack_Loading"}
    };

    // void FakeResponse(string data, Action<IpMapResponse, Action> action, Action action2)
    // {
    //     string path = Application.dataPath + data;//Path.Combine(Application.dataPath, data);
    //     Debug.Log(Application.dataPath + " ?? " + path);
    //     StreamReader sr = new StreamReader(path);
    //     string rdata = sr.ReadToEnd();

    //     IpMapResponse response = JsonUtility.FromJson<IpMapResponse>(rdata);//(uwr.downloadHandler.text);

    //     sr.Close();
    //     sr.Dispose();

    //     action(response, action2);
    // }

    /// <summary>
    /// 请求IP地图信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestIpMapInfo(MessageRequestIpMap msg, IPLayerInfo info, Action<IpInfoType1[], IPLayerInfo, Action<IpDetail[],IPLayerInfo>> action, Action<IpDetail[],IPLayerInfo> action2)
    {
        StartCoroutine(_RequestIpMapInfo(msg, info, action, action2));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestIpMapInfo(MessageRequestIpMap msg, IPLayerInfo info, Action<IpInfoType1[],IPLayerInfo, Action<IpDetail[],IPLayerInfo>> action, Action<IpDetail[],IPLayerInfo> action2)
    {
    #if USE_NET
        StringBuilder sb = new StringBuilder(m_baseAdressIP);
        sb.Append(m_meessageKeywords[NetMessageType.IpMapLoad]);
        sb.Append(msg.GetParamString());
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        Debug.Log("Request : " + uwr.url);
        yield return uwr.SendWebRequest();
        
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            /////TEST 把返回的IP数据写入文件
            string path = Application.dataPath + string.Format("/../IP_{0}_{1}.txt", msg.IPx, msg.IPy) ;
            WriteToFile(uwr.downloadHandler.data, path);
            ////////////

            Debug.Log("Response : "+uwr.url);
            IpInfoType1[] array = JsonConvert.DeserializeObject<IpInfoType1[]>(uwr.downloadHandler.text);

            if(action != null)
                action(array, info,  action2);
        }

        uwr.Dispose();
    #else
        FakeIPMapResponse("/IP_0_0.txt", info, action, action2);
        yield return new WaitForEndOfFrame();
    #endif
    }

    void FakeIPMapResponse(string filepath, IPLayerInfo info, Action<IpInfoType1[],IPLayerInfo, Action<IpDetail[],IPLayerInfo>> action, Action<IpDetail[],IPLayerInfo> action2)
    {
        string path = Application.dataPath + filepath;
        Debug.Log(path);

        StreamReader sr = new StreamReader(path);
        string rdata = sr.ReadToEnd();
        IpInfoType1[] array = JsonConvert.DeserializeObject<IpInfoType1[]>(rdata);
        sr.Close();
        sr.Dispose();

        if(action != null)
            action(array, info,  action2);
    }

    /// <summary>
    /// 请求IP地图Filter信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    //public void RequestIpMapFilterInfo(MessageRequestIpMapFilter msg, Action<IpInfoType1[], Action<IpDetail[]>> action, Action<IpDetail[]> action2)
    public void RequestIpMapFilterInfo(MessageRequestIpMapFilter msg, Action<IpInfoType1[]> action)
    {
        StartCoroutine(_RequestIpMapFilterInfo(msg.GetParamString(), action));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestIpMapFilterInfo(string urlParam,Action<IpInfoType1[]> action)
    {
        StringBuilder sb = new StringBuilder(m_baseAdressIP);
        sb.Append(m_meessageKeywords[NetMessageType.IpMapFilter]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        Debug.Log("Request : "+uwr.url);
        yield return uwr.SendWebRequest();
        
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  

            /////TEST 把返回的IP数据写入文件
            string path = Application.dataPath + "/../Filter.txt";
            WriteToFile(uwr.downloadHandler.data, path);
            ////////////

            Debug.Log("Response : "+uwr.url);
            IpInfoType1[] array = JsonConvert.DeserializeObject<IpInfoType1[]>(uwr.downloadHandler.text);

            if(action != null)
                action(array);
        }
        uwr.Dispose();
    }

    /// <summary>
    /// 请求AS地图信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestASMapInfo(MessageRequestASMap msg,Action<ASInfo[],Action> action, Action action2)
    {
        StartCoroutine(_RequestASMapInfo(msg.GetParamString(), action, action2));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestASMapInfo(string urlParam, Action<ASInfo[],Action> action, Action action2)
    {
#if USE_NET
        
        StringBuilder sb = new StringBuilder(m_baseAdressAS);
        sb.Append(m_meessageKeywords[NetMessageType.ASMapLoad]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        Debug.Log("Request : "+uwr.url);
        yield return uwr.SendWebRequest();
        
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            /////TEST 把返回的IP数据写入文件
            string path = Application.dataPath + "/../AS.txt";
            WriteToFile(uwr.downloadHandler.data, path);
            ////////////

            Debug.Log("Response : "+uwr.url);
            ASInfo[] asinfo = JsonConvert.DeserializeObject<ASInfo[]>(uwr.downloadHandler.text);
            
            if(action != null)
                action(asinfo, action2);
        }
        uwr.Dispose();
#else
        FakeASResponse("/AS.json", action, action2);
        yield return new WaitForEndOfFrame();
#endif
    }

    void FakeASResponse(string filepath, Action<ASInfo[],Action> action, Action action2)
    {
        string path = Application.dataPath + filepath;//Path.Combine(Application.dataPath, data);
        Debug.Log(path);

        List<ASInfo> list = new List<ASInfo>();
        StreamReader sr = new StreamReader(path);
        string rdata = sr.ReadLine();
        while(!string.IsNullOrEmpty(rdata))
        {
            ASInfo asinfo = JsonConvert.DeserializeObject<ASInfo>(rdata);
            asinfo.Transfer();
            list.Add(asinfo);
            
            rdata = sr.ReadLine();
        }
        sr.Close();
        sr.Dispose();

        if(action != null)
            action(list.ToArray(), action2);
    }

    /// <summary>
    /// 请求AS具体层信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestASSegmentsInfo(MessageRequestASSegments msg, Action<IpInfoType1[],string, Action<IpDetail>> action, string key, Action<IpDetail> action2)
    {
        StartCoroutine(_RequestASSegmentsInfo(msg.GetParamString(), action, key, action2));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestASSegmentsInfo(string urlParam, Action<IpInfoType1[],string, Action<IpDetail>> action, string key, Action<IpDetail> action2)
    {
        StringBuilder sb = new StringBuilder(m_baseAdressASQuery);
        sb.Append(m_meessageKeywords[NetMessageType.ASMapQuery]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        Debug.Log("Request : " + uwr.url);
        yield return uwr.SendWebRequest();
        
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            string path = Application.dataPath + "/../Segment.txt";
            WriteToFile(uwr.downloadHandler.data, path);

            Debug.Log("Response : "+uwr.url);
            IpInfoType1[] array = JsonConvert.DeserializeObject<IpInfoType1[]>(uwr.downloadHandler.text);
            if(action != null)
                action(array, key, action2);
        }
        uwr.Dispose();
    }

    /// <summary>
    /// 请求AS具体层信息
    /// </summary>
    /// <param name="msg">msg中的变量不用全赋值，未赋值变量会自动使用默认值</param>
    /// <param name="action"></param>
    public void RequestAttackInfo(MessageRequestAttackInfo msg, Action<AttackData[]> action)
    {
        StartCoroutine(_RequestAttackInfo(msg.GetParamString(), action));
    }

    /// <summary>
    /// 发送网络请求
    /// </summary>
    /// <param name="urlParam"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator _RequestAttackInfo(string urlParam, Action<AttackData[]> action)
    {
    #if USE_NET
        StringBuilder sb = new StringBuilder(m_baseAdressAS);
        sb.Append(m_meessageKeywords[NetMessageType.AttackLoad]);
        sb.Append(urlParam);
        UnityWebRequest uwr = UnityWebRequest.Get(sb.ToString());
        Debug.Log("Request : " + uwr.url);
        yield return uwr.SendWebRequest();
        
        if(!uwr.isNetworkError && !uwr.isHttpError && action != null)
        {  
            string path = Application.dataPath + "/../Attack.txt";
            WriteToFile(uwr.downloadHandler.data, path);

            Debug.Log("Response : "+uwr.url);
            AttackData[] array = JsonConvert.DeserializeObject<AttackData[]>(uwr.downloadHandler.text);
            if(action != null)
                action(array);
        }
        uwr.Dispose();
    #else
        FakeAttackResponse("/Attack.txt", action);
        yield return new WaitForEndOfFrame();
    #endif
    }

    void FakeAttackResponse(string filepath, Action<AttackData[]> action)
    {
        string path = Application.dataPath + filepath;//Path.Combine(Application.dataPath, data);
        Debug.Log(path);

        StreamReader sr = new StreamReader(path);
        string rdata = sr.ReadToEnd();
        AttackData[] array = JsonConvert.DeserializeObject<AttackData[]>(rdata);
        sr.Close();
        sr.Dispose();

        if(action != null)
            action(array);
            
        Debug.Log("Attack Data Loading Finish ~ ~ ~ ~ ~ ~ ~ ");
    }

    // TEST 测试专用
    void WriteToFile(byte[] data, string path)
    {
        if(System.IO.File.Exists(path))
            System.IO.File.Delete(path);
            
            /////TEST 把返回的IP数据写入文件

            FileStream fileStream = System.IO.File.Create(path);
            //FileStream fileStream = File.OpenWrite(path);
            fileStream.Write(data , 0 , data.Length);
            fileStream.Close();
            fileStream.Dispose();
            /////
    }
}
