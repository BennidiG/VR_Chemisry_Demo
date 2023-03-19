using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustPosition : MonoBehaviour
{
    private float MoveSpeed = 0.5f;

    private GameObject Clip;
    private GameObject MetalFixed;

    private GameObject Up;
    private GameObject Down;
    // Start is called before the first frame update
    void Start()
    {
        Clip = GameObject.Find("Clip");
        MetalFixed = GameObject.Find("MetalFixed");

        Up = GameObject.Find("Forward");
        Down = GameObject.Find("Back");

        Up.gameObject.SetActive(false);
        Down.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show()
    {
        Up.gameObject.SetActive(true);
        Down.gameObject.SetActive(true);
    }

    public void TranslateUp()
    {
        Clip.transform.Translate(Vector3.up * MoveSpeed * Time.deltaTime);
        MetalFixed.transform.Translate(Vector3.up * MoveSpeed * Time.deltaTime);
    }

    public void TranslateDown()
    {
        Clip.transform.Translate(Vector3.down * MoveSpeed * Time.deltaTime);
        MetalFixed.transform.Translate(Vector3.down * MoveSpeed * Time.deltaTime);
    }
}
