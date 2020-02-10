using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IpTopologyBox : MonoBehaviour
{
    public Transform NextPoint;
    public Transform PrePoint;
    [SerializeField]
    private TMP_Text m_IpText;

    public void SetData(string ip)
    {
        m_IpText.text = ip;
    }
}
