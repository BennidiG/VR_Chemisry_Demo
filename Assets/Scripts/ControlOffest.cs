using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOffest : MonoBehaviour
{
    MeshRenderer My_R;
    public float ds;

    // Start is called before the first frame update
    void Start()
    {
        ds = 0.25F;
        //My_R = GetComponent<MeshRenderer>();
        My_R = GameObject.Find("Cubee").GetComponent<MeshRenderer>();
        //My_R = GameObject.Find()<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ds -= 0.001F;
        My_R.material.SetFloat("Disslpate", ds);
        if (ds <0)
            ds = 0.25F;
    }

    /*void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
    {
        Out = UV * Tiling + Offset;
    }*/
}
