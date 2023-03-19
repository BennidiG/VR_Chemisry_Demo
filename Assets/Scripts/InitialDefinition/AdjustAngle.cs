using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustAngle : MonoBehaviour
{
    private float RotateSpeed = 10f;

    private GameObject Clip;

    private GameObject Forward;
    private GameObject Back;

    // Start is called before the first frame update
    void Start()
    {
        Clip = GameObject.Find("Clip");

        Forward = GameObject.Find("Up");
        Back = GameObject.Find("Down");

        Forward.gameObject.SetActive(false);
        Back.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show()
    {
        Forward.gameObject.SetActive(true);
        Back.gameObject.SetActive(true);
    }

    public void RotateForward()
    {
        Clip.transform.Rotate(Vector3.forward * RotateSpeed * Time.deltaTime);
    }

    public void RotateBack()
    {
        Clip.transform.Rotate(Vector3.back * RotateSpeed * Time.deltaTime);
    }
}
