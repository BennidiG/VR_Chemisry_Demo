using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryMatch : MonoBehaviour
{
    private GameObject Match;

    private CreateMatch IF_CreateMatch;
    private Score Match_ReFire_Score;
    // Start is called before the first frame update
    void Start()
    {
        IF_CreateMatch = FindObjectOfType<CreateMatch>();
        Match_ReFire_Score = FindObjectOfType<Score>();
        Match = GameObject.Find("Match");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Des()
    {
        Match = SceneObjectFinder.Find("Match");
        if (Match_ReFire_Score != null && ((Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step4()) || (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.IF_Make_O2_Step8())))
            Match_ReFire_Score.Yes_Match_Destory();
        if (Match != null)
            Destroy(Match);
        if (IF_CreateMatch != null)
            IF_CreateMatch.Clear_Old_Match();
    }
}
