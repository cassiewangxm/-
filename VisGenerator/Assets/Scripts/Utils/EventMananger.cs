using System;
using UnityEngine;
using System.Collections.Generic;

public static class EventManager
{
    //public delegate void Callback(params object[] values);
    private static Dictionary<EventDefine, Delegate> eventMap = new Dictionary<EventDefine, Delegate>();

    public static void RegistEvent(EventDefine eventType, Delegate handler)
    {
        if (!eventMap.ContainsKey(eventType))
        {
            eventMap.Add(eventType, handler);
            return;
        }

        Delegate curHandler = eventMap[eventType];
        //check the handler added or not
        Delegate[] delegateList = curHandler.GetInvocationList();
        for (int i = 0; i < delegateList.Length; i++)
        {
            Delegate item = delegateList[i];
            if (item.Equals(handler))
                return;
        }

        curHandler = Delegate.Combine(curHandler, handler);
        eventMap[eventType] = curHandler;
    }

    public static void UnregistEvent(EventDefine eventType, Delegate handler)
    {
        if (eventMap.ContainsKey(eventType))
        {
            Delegate orgHandler = (Delegate) eventMap[eventType];
            orgHandler = Delegate.Remove(orgHandler, handler);
            if (orgHandler == null)
            {
                eventMap.Remove(eventType);
                return;
            }

            eventMap[eventType] = orgHandler;
        }
    }

    public static void SendEvent(EventDefine eventType, params object[] values)
    {
        try
        {
            Delegate callBack;
            if (eventMap.TryGetValue(eventType, out callBack))
            {
                if (callBack == null)
                    return;
                callBack.DynamicInvoke(values);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public static void Cleanup()
    {
        eventMap.Clear();
    }

    internal static void RegistEvent(EventDefine onRecieveSearchResult, Action showFilterResult)
    {
        throw new NotImplementedException();
    }
}