using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UIEventType
{
    None,
    ShowIPMenu,
    ShowDetail,
    ShowTopology,
    ShowAttack,
    ShowBan
}

public class UIEventDispatcher
{
    public static Action<string, Vector2> showIPMenuPanel;
    public static Action hideIPMenuPanel;
    public static Action<IpDetail, Vector2> showIPDetail;
    public static Action<string> showIPTopology;
    public static Action<string> attackTarget;
    public static Action<string> banTarget;

    public static void OpenIpMenuPanel(string ip, Vector2 screenPos)
    {
        if (showIPMenuPanel != null)
            showIPMenuPanel(ip, screenPos);
    }

    public static void HideIpMenuPanel()
    {
        if (hideIPMenuPanel != null)
            hideIPMenuPanel();
    }

    public static void OpenIPDetailPanel(IpDetail info, Vector2 screenPos)
    {
        if (showIPDetail != null)
            showIPDetail(info, screenPos);
    }

    public static void OpenIPTopologyPanel(string ip)
    {
        if (showIPTopology != null)
            showIPTopology(ip);
    }
}
