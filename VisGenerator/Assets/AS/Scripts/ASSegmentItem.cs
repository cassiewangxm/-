using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ASSegmentItem : MonoBehaviour
{
    public TextMeshPro TMPtext;

    public ASSegment segemntData;

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }
    public void SetSegment(ASSegment data)
    {
        segemntData = data;

        SetText();

        gameObject.SetActive(true);
    }
    private void SetText()
    {
        TMPtext.text = "---" + segemntData.BornDate + " , " + segemntData.IPCount;
    }
}
