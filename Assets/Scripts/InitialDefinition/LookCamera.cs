using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    private Transform target;//ÉãÏñ»ú¶ÔÏó
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Main Camera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tar = target.position;
        tar.x = transform.position.x + transform.position.x - target.position.x;
        tar.y = transform.position.y;
        tar.z = transform.position.z + transform.position.z - target.position.z;
        transform.LookAt(tar, Vector3.up);
    }
}
