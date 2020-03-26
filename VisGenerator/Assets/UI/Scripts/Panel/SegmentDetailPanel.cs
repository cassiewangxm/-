using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SegmentDetailPanel : UIBasePanel, IPointerClickHandler, IDragHandler
{
    [SerializeField]
    private Text m_TimeText;
    [SerializeField]
    private RawImage m_IPMap;
    [SerializeField]
    private RectTransform m_validClickRect;
    [SerializeField]
    private IPDetailPanel m_IpDetailPanel;
    [SerializeField]
    private RawImage m_focusMark;

    private Texture2D m_texture;
    private ASSegmentInfo m_segmentData;
    private int m_originalWidth;
    private int m_maxWidth;
    private int m_curLevel;
    private int m_maxLevel;
    private Vector2Int m_pixelInfo = Vector2Int.one; // x = pixel size， y = line count
    void Awake()
    {
        m_originalWidth = (int)m_IPMap.rectTransform.sizeDelta.x;
        m_texture = new Texture2D((int)m_originalWidth, (int)m_originalWidth);
        m_IPMap.texture = m_texture;
    }

    void OnDisable()
    {
        if(RTFocusCamera.Get != null)
        {
            RTFocusCamera.Get.LookAroundSettings.IsLookAroundEnabled = true;
            RTFocusCamera.Get.ZoomSettings.IsZoomEnabled = true;
            RTFocusCamera.Get.PanSettings.IsPanningEnabled = true;
        }
    }
    public void SetUIData(ASSegmentInfo info)
    {
        m_segmentData = info;
        m_curLevel = 1;
        int maxw = Mathf.CeilToInt( Mathf.Sqrt(info.IPCount) ) * 16;
        m_maxLevel = Mathf.CeilToInt((float)maxw / m_originalWidth);
        m_maxWidth = m_maxLevel * m_originalWidth;

        Debug.LogFormat("IPcount : {0} ; MaxWidth : {1}; MaxLevel : {2}", info.IPCount, m_maxWidth, m_maxLevel);

        Clean();
        StopAllCoroutines();
        StartCoroutine(GenerateTextureByLevel(m_curLevel));
        UpdateUI();

        RTFocusCamera.Get.LookAroundSettings.IsLookAroundEnabled = false;
        RTFocusCamera.Get.ZoomSettings.IsZoomEnabled = false;
        RTFocusCamera.Get.PanSettings.IsPanningEnabled = false;
    }

    IEnumerator GenerateTextureByLevel(int level)
    {
        int width = level * m_originalWidth;
        width = Mathf.Min(m_maxWidth, width);
        if(m_texture.width != width)
        {
            m_texture.Resize(width, width);
        }

        int pixelsize = (int)Mathf.Sqrt( (width * width) / m_segmentData.IPCount);
        pixelsize = pixelsize > 0 ? pixelsize : 1;
        int lineCount = width/pixelsize + 1;
        m_pixelInfo.x = pixelsize;
        m_pixelInfo.y = lineCount;
        Debug.LogFormat("IPMap Resize (width , pixel) = ({0},{1}) ", width, pixelsize);

        int curCount = 0;
        for(int i = 0; i < width; i++)
        {  
            for(int j = 0; j < width; j++)
            {
                if(j/pixelsize >= lineCount)
                {
                    m_texture.SetPixel(i, j, Color.black);
                }
                else
                {
                    curCount = i/pixelsize * lineCount + j/pixelsize;
                    m_texture.SetPixel(i, j, m_segmentData.GetIPColor(curCount));
                    //Debug.LogFormat("({0},{1}) : {2}", i, j, m_segmentData.GetIPColor(curCount));
                }
            }

            if(curCount % 512 == 0)
            {
                m_texture.Apply();
                yield return null;
            }
        }
        m_texture.Apply();
    }

    public void UpdatePos(Vector2 screenPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 offset = screenPos - new Vector2(Screen.width/2, Screen.height/2);

        if(offset.x > 0)
            offset.x -= rect.sizeDelta.x/2;
        else
            offset.x += rect.sizeDelta.x/2;
        
        //越界检测
        if(offset.x - rect.sizeDelta.x/2 < - Screen.width/2)
            offset.x = rect.sizeDelta.x/2 - Screen.width/2;
        if(offset.x + rect.sizeDelta.x/2 > Screen.width/2)
            offset.x = Screen.width/2 - rect.sizeDelta.x/2;
        if(offset.y - rect.sizeDelta.y/2 < - Screen.height/2)
            offset.y = rect.sizeDelta.y/2 - Screen.height/2;
        if(offset.y + rect.sizeDelta.y/2 > Screen.height/2)
            offset.y = Screen.height/2 - rect.sizeDelta.y/2;
        
        rect.anchoredPosition = offset;
    }

    private void UpdateUI()
    {
        if (m_segmentData == null) 
            return;
            
        m_focusMark.gameObject.SetActive(false);
        m_TimeText.text = m_segmentData.Time;
    }

    private void Clean()
    {
        m_TimeText.text = string.Empty;
    }

    void Update()
    {
        if(Input.mouseScrollDelta != Vector2.zero)
        {
            HideFocusMark();

            //Debug.Log(Input.mouseScrollDelta); 
            Vector2 targetSize = m_IPMap.rectTransform.sizeDelta * (1.0f + Input.mouseScrollDelta.y / 100);
            float scale = targetSize.x / m_originalWidth;
            //Debug.Log(scale+" , "+m_curLevel);
            if(scale >= m_curLevel + 1 && m_curLevel < m_maxLevel)
            {
                m_curLevel ++;
                StopAllCoroutines();
                StartCoroutine(GenerateTextureByLevel(m_curLevel));
            }
            else if(scale <= m_curLevel - 1 && m_curLevel > 1)
            {
                m_curLevel --;
                StopAllCoroutines();
                StartCoroutine(GenerateTextureByLevel(m_curLevel));
            }

            if(targetSize.x <= m_maxWidth * 2 && targetSize.x >= m_originalWidth)
                m_IPMap.rectTransform.sizeDelta = targetSize;
        }
    }
    public void OnPointerClick(PointerEventData data)
    {
        if(CheckClickPos(data.position, m_validClickRect) && CheckClickPos(data.position, m_IPMap.rectTransform))
        {
            UpdateMarkPos(data.position);

            Vector2 offset = GetOffset(data.position, m_IPMap.rectTransform);

            int y = (int)(offset.y/m_IPMap.rectTransform.rect.height * m_pixelInfo.y);
            int x = (int)(offset.x/m_IPMap.rectTransform.rect.width * m_pixelInfo.y);

            int index = y * m_pixelInfo.y + x;

            ASProxy.instance.GetSegmentIPInfo(m_segmentData, index, ShowIPDetail);
        }
        
    }
    public void OnDrag(PointerEventData eventData)
    {
        HideFocusMark();
    }   

    void UpdateMarkPos(Vector2 pos)
    {
        float x = pos.x - m_validClickRect.position.x;
        float y = pos.y - m_validClickRect.position.y;
        m_focusMark.gameObject.SetActive(true);
        m_focusMark.rectTransform.localPosition = new Vector3(x, y, 0);
    }

    void HideFocusMark()
    {
        if(m_focusMark.gameObject.activeSelf)
            m_focusMark.gameObject.SetActive(false);
    }

    void ShowIPDetail(IpDetail ipDetail)
    {
        if(m_IpDetailPanel != null && ipDetail != null)
        {
            m_IpDetailPanel.SetUIData(ipDetail);
            m_IpDetailPanel.gameObject.SetActive(true);
        }
    }

    bool CheckClickPos(Vector2 pos, RectTransform rect)
    {
        Vector2 offset = GetOffset(pos, rect);
        
        if(offset.x >= 0 && offset.y >= 0 && offset.x <= rect.rect.width && offset.y <= rect.rect.height)
        {
            return true;
        }
        return false;
    }

    Vector2 GetOffset(Vector2 pos, RectTransform rect)
    {
        float x = pos.x - (rect.position.x - rect.pivot.x * rect.rect.width);
        float y = pos.y - (rect.position.y - rect.pivot.y * rect.rect.height);

        return new Vector2(x,y);
    }
    
}
