using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBasePanel : MonoBehaviour
{
    public static readonly float UIPadding = 5f;

    public virtual void OnClose()
    {
        this.gameObject.SetActive(false);
    }
}
