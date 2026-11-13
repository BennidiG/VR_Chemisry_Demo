using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ReG : MonoBehaviour
{

    public void Click()
    {
        ExperimentRunner.GetOrCreate().ResetCurrentExperiment();
    }
}