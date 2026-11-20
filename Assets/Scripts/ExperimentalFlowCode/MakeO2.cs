using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class MakeO2 : MonoBehaviour
{
    private const string StartHeatingMethod = "DelayStartAirBubbles";
    private const string StopHeatingMethod = "DelayStopAirBubbles";

    private TMP_Text prompt;
    private ParticleSystem airBubbles;
    private Renderer bubblesRenderer;
    private VisualEffect lampFire;

    private static bool Lamp_Is_Fire;
    private static bool Tube_On_Fire;
    private static bool Hot;

    private O2IntoBottle If_O2Into_Bottle;
    private CanSee_AirBubbles If_CanSee_AB;
    private GetPowder If_Get_Powder;
    private Score score;
    private ExperimentRunner runner;

    void Start()
    {
        Lamp_Is_Fire = false;
        Tube_On_Fire = false;
        Hot = false;

        airBubbles = GameObject.Find("Bubbles").GetComponent<ParticleSystem>();
        bubblesRenderer = airBubbles.GetComponent<Renderer>();
        prompt = GameObject.Find("Prompt").GetComponent<TMP_Text>();
        lampFire = GameObject.Find("LampFire").GetComponent<VisualEffect>();

        If_O2Into_Bottle = FindObjectOfType<O2IntoBottle>();
        If_CanSee_AB = FindObjectOfType<CanSee_AirBubbles>();
        If_Get_Powder = FindObjectOfType<GetPowder>();
        score = FindObjectOfType<Score>();
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;

        bubblesRenderer.enabled = false;
        airBubbles.Stop();
    }

    void Update()
    {
        See_AB();
        HotEnough();
    }

    public void LampIsFire()
    {
        Lamp_Is_Fire = true;
        TryCompleteHeatingStep();
    }

    public void LampIsNotFire()
    {
        Lamp_Is_Fire = false;
        if (lampFire != null)
            lampFire.Stop();
        if (Tube_On_Fire)
        {
            Tube_On_Fire = false;
            ScheduleStop();
        }
    }

    public bool IfLampIsFire()
    {
        return Lamp_Is_Fire;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "OnFire")
            BeginHeating();
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "OnFire")
            BeginHeating();
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "OnFire")
        {
            Tube_On_Fire = false;
            ScheduleStop();
        }
    }

    private void BeginHeating()
    {
        if (!Lamp_Is_Fire)
            return;

        bool wasOnFire = Tube_On_Fire;
        Tube_On_Fire = true;
        TryCompleteHeatingStep();
        if (!wasOnFire)
            SetPrompt("Tube Is On Fire");

        // Re-entering the flame during the cooling delay keeps the reaction alive.
        CancelInvoke(StopHeatingMethod);
        if (!Hot && !IsInvoking(StartHeatingMethod))
            Invoke(StartHeatingMethod, 5F);
    }

    private void ScheduleStop()
    {
        CancelInvoke(StartHeatingMethod);
        SetPrompt("Tube Is Not On Fire");
        if (!IsInvoking(StopHeatingMethod))
            Invoke(StopHeatingMethod, 5F);
    }

    public void DelayStartAirBubbles()
    {
        if (!Tube_On_Fire || !Lamp_Is_Fire)
            return;

        Hot = true;
        bool hasReagent = If_Get_Powder != null && If_Get_Powder.Can_Burn();
        ReactionResult result = ChemistryLegacyAdapter.EvaluateMakeOxygen(true, hasReagent, score != null && score.If_Make_O2_Step4());
        if (result.Succeeded)
        {
            airBubbles.Play();
            SetPrompt("Chemical reaction in progress");
        }
        else
        {
            airBubbles.Stop();
            SetPrompt(hasReagent ? result.FailureReason : "No reagent in the test tube");
        }
    }

    public void DelayStopAirBubbles()
    {
        if (Tube_On_Fire && Lamp_Is_Fire)
            return;

        airBubbles.Stop();
        Hot = false;
        SetPrompt("Chemical reaction not in progress");
    }

    public void HotEnough()
    {
        if (If_O2Into_Bottle == null)
            return;

        ReactionResult result = ChemistryLegacyAdapter.EvaluateMakeOxygen(Hot, If_Get_Powder != null && If_Get_Powder.Can_Burn(), score != null && score.If_Make_O2_Step4());
        if (result.Succeeded)
            If_O2Into_Bottle.MakeingO2True();
        else
        {
            If_O2Into_Bottle.MakeingO2False();
            if (airBubbles != null)
                airBubbles.Stop();
        }
    }

    public void See_AB()
    {
        if (bubblesRenderer != null && If_CanSee_AB != null)
            bubblesRenderer.enabled = If_CanSee_AB.Can_See_AB();
    }

    public void StartAirBubbles()
    {
        airBubbles.Play();
    }

    public void StopAirBubbles()
    {
        airBubbles.Stop();
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    private void OnExperimentReset()
    {
        ResetForExperiment();
    }

    public void TryCompleteHeatingStep()
    {
        if (score == null || score.Project_Num() != 1 || !score.If_Make_O2_Step5())
            return;
        if (!Lamp_Is_Fire || !Tube_On_Fire)
            return;

        score.Make_O2_Step5_To_Step6();
        GameObject stepText = SceneObjectFinder.Find("Make_O2_Step6");
        if (stepText != null)
            stepText.GetComponent<TMP_Text>().color = Color.green;
    }

    public void ResetForExperiment()
    {
        CancelInvoke();
        Lamp_Is_Fire = false;
        Tube_On_Fire = false;
        Hot = false;
        if (airBubbles != null)
            airBubbles.Stop();
        if (lampFire != null)
            lampFire.Stop();
        if (If_O2Into_Bottle != null)
            If_O2Into_Bottle.MakeingO2False();
    }

    private void SetPrompt(string message)
    {
        if (prompt != null)
            prompt.text = message;
    }
}
