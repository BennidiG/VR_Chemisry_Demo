using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CreateMatch : MonoBehaviour
{

    public GameObject Match;
    private static bool IF_CreateM;
    private static bool Have_M;

    // Start is called before the first frame update
    void Start()
    {
        IF_CreateM = false;
        Have_M = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Creat_New_Match()
    {
        if (!Have_M)
        {
            GameObject Match_Copy = Instantiate(Match, new Vector3(2.77F, 1.024F, 5.87F), Quaternion.identity);    //Œª÷√
            Match_Copy.transform.localScale = new Vector3(0.005F, 0.15F, 0.005F);    //≥§∂»
            Match_Copy.transform.Rotate(new Vector3(90, 0, -90));
            Match_Copy.name = "Match";
            IF_CreateM = true;
            Have_M = true;
        }
    }

    public bool Have_Match()
    {
        if(Have_M)
            return true;
        return false;
    }

    public bool Have_Created_Match()
    {
        if (IF_CreateM)
            return true;
        return false;
    }

    public void Clear_Old_Match()
    {
        IF_CreateM = false;
        Have_M = false;
    }

    public void Have_New_Match()
    {
        IF_CreateM = false;
        Have_M = true;
    }
}