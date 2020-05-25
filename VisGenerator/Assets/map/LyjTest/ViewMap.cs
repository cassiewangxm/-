using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewMap : MonoBehaviour
{
    public GameObject IconParent;
    public GameObject PointPrefab;
    public Camera Camera;
    public Vector2[] position;

    [SerializeField]
    private bool test;

    [SerializeField]
    private Vector2[] oldPosition;
    private Vector3 newPosition;

    private Vector3 LatLng2Pos(Vector2 Item)
    {
        //return new Vector3(Item.lng / 180.0f * MapWidth, -8.4f, Item.lat / 90.0f * MapHeight);
        Debug.Log(Item.x / 180.0f * 640.0f);
        Debug.Log(((Item.y + 90.0f) / 185.0f * 180.0f - 90.0f) / 90.0f * 240.0f);
        return new Vector3(Item.x / 180.0f * 640.0f, -7f, ((Item.y + 90.0f) / 185.0f * 180.0f - 90.0f) / 90.0f * 240.0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in IconParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Vector2 Pointposition in position)
        {
            float xx = Pointposition.x;
            float yy = Pointposition.y;
            //GameObject NewIcon = Instantiate(PointPrefab, new Vector3(Pointposition.x, -7f, Pointposition.y), Quaternion.identity);
            GameObject NewIcon = Instantiate(PointPrefab, LatLng2Pos(Pointposition), Quaternion.identity);
            NewIcon.transform.SetParent(IconParent.transform);
            NewIcon.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            NewIcon.transform.localScale = new Vector3(10f, 10f, 0f);
            NewIcon.layer = 12;
        }
        oldPosition = (Vector2[])position.Clone();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 newPosition = Camera.transform.position;
        

        //oldPosition = position;
        //test = position.Equals(oldPosition);
        test = position.Length==oldPosition.Length;
        if (!test)
        {
            
            //oldPosition = position;
            foreach (Transform child in IconParent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Vector2 Pointposition in position)
            {
                GameObject NewIcon = Instantiate(PointPrefab, LatLng2Pos(Pointposition), Quaternion.identity);
                NewIcon.transform.SetParent(IconParent.transform);
                NewIcon.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                NewIcon.transform.localScale = new Vector3(10f, 10f, 0f);
                NewIcon.layer = 12;
            }
            oldPosition = (Vector2[])position.Clone();
        }
    }
}
