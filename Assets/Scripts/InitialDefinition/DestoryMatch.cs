using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryMatch : MonoBehaviour
{
    private GameObject Match;

    CreateMatch IF_CreateMatch = new CreateMatch();
    // Start is called before the first frame update
    void Start()
    {
        Match = GameObject.Find("Match");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Des()
    {
        Match = GameObject.Find("Match");
        Destroy(Match);
        IF_CreateMatch.Clear_Old_Match();
    }
}
