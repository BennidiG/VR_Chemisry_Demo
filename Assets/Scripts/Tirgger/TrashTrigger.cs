using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashTrigger : MonoBehaviour
{
    CreateMatch IF_CreateMatch = new CreateMatch();
    Score Match_ReFire_Score = new Score();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Match")
        {
            if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Match_ReFire_Step4())
            {
                Match_ReFire_Score.Yes_Match_Destory();
            }
            Destroy(collider.gameObject);
            IF_CreateMatch.Clear_Old_Match();
        }
        else
            Destroy(collider.gameObject);
    }

    public void OnTriggerStay(Collider collider)
    {

    }

    public void OnTriggerExit(Collider collider)
    {

    }
}
