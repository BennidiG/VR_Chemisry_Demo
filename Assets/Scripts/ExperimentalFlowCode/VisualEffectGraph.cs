using System;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class VisualEffectGraph : MonoBehaviour
{
    private const float MaxOxygen = 10000F;
    private const float OxygenPerSecond = 60F;
    private const float NormalBurnRate = 0.006F;
    private const float ReFireBurnRate = 0.012F;

    private VisualEffect Fire;
    private VisualEffect ReFire;
    private VisualEffect Smoke;
    private GameObject MatchHead;
    private GameObject Mars;
    private GameObject MHead;
    private GameObject StickTirgger;
    private GameObject DisslpateBody;
    private BoxCollider Stick_ContainerCollider;
    private Rigidbody matchBody;
    private Renderer marsRenderer;
    private Renderer dissolveBodyRenderer;
    private MeshRenderer My_RDisslpate;

    private TMP_Text responseText;
    private TMP_Text promptText;
    private TMP_Text oxygenText;
    private TMP_Text Match_ReFire_Step2;
    private TMP_Text Match_ReFire_Step3;
    private Score Match_ReFire_Score;
    private OxygenSupply oxygenSupply;
    private ExperimentRunner runner;

    private int Can_refire;
    private int Is_fire;
    private int Is_refire;
    private int Flag;
    private bool DissAdd;
    private bool DestroyDiss;

    private float aliveParticleCount;
    private float O2ParticleCount;
    private float Off;
    private float Dissposition;
    private float nextOxygenTextRefresh;
    private bool oxygenTextDirty;
    private readonly System.Random random = new System.Random();

    void Start()
    {
        Match_ReFire_Score = FindObjectOfType<Score>();
        oxygenSupply = FindObjectOfType<OxygenSupply>();
        if (oxygenSupply != null)
        {
            O2ParticleCount = oxygenSupply.Amount;
            oxygenSupply.AmountChanged += OnOxygenAmountChanged;
        }
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;
        BindMatchObjects();
        BindUserInterface();

        Can_refire = 0;
        Is_refire = 0;
        Is_fire = 0;
        Flag = 0;
        DissAdd = false;
        DestroyDiss = false;
        aliveParticleCount = 0;
        O2ParticleCount = oxygenSupply != null ? oxygenSupply.Amount : 0F;
        Off = 0.500001F;
        Dissposition = 0.600001F;
        oxygenTextDirty = true;
        nextOxygenTextRefresh = 0F;

        Fire.Stop();
        ReFire.Stop();
        Smoke.Stop();
        marsRenderer.enabled = false;
        dissolveBodyRenderer.enabled = false;
        My_RDisslpate.enabled = false;
    }

    private void BindMatchObjects()
    {
        MatchHead = transform.Find("MatchHead").gameObject;
        Mars = transform.Find("Mars").gameObject;
        MHead = transform.Find("MHead").gameObject;
        StickTirgger = transform.Find("StickTirgger").gameObject;
        DisslpateBody = transform.Find("DisslpateBody").gameObject;

        Transform stick = transform.Find("stick");
        Stick_ContainerCollider = stick.GetComponent<BoxCollider>();
        matchBody = GetComponent<Rigidbody>();
        Fire = transform.Find("Fire").GetComponent<VisualEffect>();
        ReFire = transform.Find("ReFire").GetComponent<VisualEffect>();
        Smoke = transform.Find("Smoke").GetComponent<VisualEffect>();
        My_RDisslpate = transform.Find("Disslpate").GetComponent<MeshRenderer>();
        marsRenderer = Mars.GetComponent<Renderer>();
        dissolveBodyRenderer = DisslpateBody.GetComponent<Renderer>();
    }

    private void BindUserInterface()
    {
        responseText = FindText("ResponseRatio");
        promptText = FindText("Prompt");
        oxygenText = FindText("ParticleCount");
        Match_ReFire_Step2 = FindText("Match_ReFire_Step2");
        Match_ReFire_Step3 = FindText("Match_ReFire_Step3");
    }

    private static TMP_Text FindText(string objectName)
    {
        GameObject textObject = SceneObjectFinder.Find(objectName);
        return textObject != null ? textObject.GetComponent<TMP_Text>() : null;
    }

    void Update()
    {
        if (oxygenTextDirty && Time.unscaledTime >= nextOxygenTextRefresh)
        {
            nextOxygenTextRefresh = Time.unscaledTime + 0.1F;
            oxygenTextDirty = false;
            UpdateOxygenText("O2 Particle Count :");
        }
        if (Flag == 1 || aliveParticleCount > 0)
            Particle_Number();
        if (Is_fire == 1 || Is_refire == 1)
            Short_Stick();
        if (DissAdd || Is_fire == 1 || Is_refire == 1)
            DisslpateCount();
        if (DestroyDiss)
            DestroyDisslpate();
        OutSpeed();
    }

    void Particle_Number()
    {
        int targetMin = Application.platform == RuntimePlatform.Android ? 150 : 450;
        int targetMax = Application.platform == RuntimePlatform.Android ? 390 : 790;
        int upperLimit = Application.platform == RuntimePlatform.Android ? 400 : 820;
        if (Flag == 1)
        {
            if (aliveParticleCount < targetMin)
                aliveParticleCount += random.Next(-1, 4);
            else if (aliveParticleCount > upperLimit)
                aliveParticleCount -= random.Next(-1, 3);
            else if (aliveParticleCount < targetMax)
                aliveParticleCount += random.Next(-1, 3);
            else
                aliveParticleCount += random.Next(-2, 2);
        }
        else if (aliveParticleCount > 5)
        {
            aliveParticleCount -= random.Next(-1, 3);
        }
        else
        {
            aliveParticleCount = 0;
        }
        aliveParticleCount = Mathf.Max(0, aliveParticleCount);

        if (responseText != null)
        {
            float divisor = Is_refire == 0 ? 40F : 10F;
            responseText.text = "Response Ratio :" + (aliveParticleCount / divisor).ToString("F1").PadRight(13) + "\t%";
        }

        if (Is_refire == 1)
        {
            if (oxygenSupply != null)
            {
                oxygenSupply.ConsumeOxygen(OxygenPerSecond * Time.deltaTime);
                O2ParticleCount = oxygenSupply.Amount;
            }
            else
            {
                O2ParticleCount = Mathf.Max(0, O2ParticleCount - OxygenPerSecond * Time.deltaTime);
            }

            if (O2ParticleCount <= 0)
            {
                O2_Empty();
                Can_refire = 0;
                SetPrompt("Oxygen depleted. Use an external flame to ignite again.");
            }
        }

        UpdateOxygenText("O2 Particle Count :");
    }

    void Short_Stick()
    {
        if (aliveParticleCount > 600 && Off > 0.2F)
        {
            float burnRate = Is_refire == 0 ? NormalBurnRate : ReFireBurnRate;
            Off = Mathf.Max(0.2F, Off - burnRate * Time.deltaTime);

            Stick_ContainerCollider.transform.localScale = new Vector3(1, 2 * Off, 1);
            Mars.transform.localPosition = new Vector3(0, Off, 0);
            MHead.transform.localPosition = new Vector3(0, Off, 0);
            Fire.transform.localPosition = new Vector3(0, Off, 0);
            ReFire.transform.localPosition = new Vector3(0, Off, 0);
            Smoke.transform.localPosition = new Vector3(0, Off, 0);
            StickTirgger.transform.localPosition = new Vector3(0, Off + 0.02F, 0);
            DisslpateBody.transform.localPosition = new Vector3(0, Off, 0);
            My_RDisslpate.transform.localPosition = new Vector3(0, Off, 0);
        }

        if (Off <= 0.2F && Flag == 1)
        {
            stopfire();
            Can_refire = 0;
            SetPrompt("Match is burned out and cannot be reused");
        }
    }

    void DisslpateCount()
    {
        if (Is_fire != 1)
            return;

        Dissposition += (DissAdd ? 0.003F : -0.003F) * Time.deltaTime;
        if (Dissposition > 0.6F)
        {
            Dissposition = 0.6F;
            DissAdd = false;
        }
        else if (Dissposition < 0.5F)
        {
            Dissposition = 0.5F;
            DissAdd = true;
        }

        LabMaterialState.SetDissolve(My_RDisslpate, Dissposition - 0.2F, Dissposition);
    }

    public void startfire()
    {
        if (Off <= 0.2F)
        {
            Can_refire = 0;
            SetPrompt("Match is burned out and cannot be ignited again");
            return;
        }

        // Repeated trigger stays must not restart VFX or reset the burn effect.
        if (Is_fire == 1 && Is_refire == 0)
            return;

        CancelPendingExtinction();
        if (MatchHead != null)
            Destroy(MatchHead);

        Fire.Play();
        ReFire.Stop();
        Smoke.Play();
        marsRenderer.enabled = true;
        dissolveBodyRenderer.enabled = true;
        My_RDisslpate.enabled = true;

        SetPrompt("Start Fire : O2 + C = CO2 ");
        Can_refire = 1;
        Is_fire = 1;
        Is_refire = 0;
        Flag = 1;
    }

    public void stopfire()
    {
        if (Flag == 0)
            return;

        Fire.Stop();
        ReFire.Stop();
        Is_refire = 0;
        Is_fire = 0;
        Flag = 0;
        Can_refire = 1;
        dissolveBodyRenderer.enabled = false;

        CancelInvoke("SetDestroyDisslpate");
        CancelInvoke("DelayStopFire");
        Invoke("SetDestroyDisslpate", 2F);
        Invoke("DelayStopFire", 5F);
        SetPrompt("Stop Fire, please put match in O2 bottle in 5s ");
    }

    public void refire()
    {
        if (Off <= 0.2F)
        {
            Can_refire = 0;
            SetPrompt("Match is burned out and cannot restart fire");
            return;
        }

        ReactionResult refireResult = ChemistryLegacyAdapter.EvaluateMatchRefire(Can_refire == 1, oxygenSupply != null ? oxygenSupply.Amount : O2ParticleCount);
        if (!refireResult.Succeeded)
        {
            SetPrompt(refireResult.FailureReason);
            return;
        }

        // Entering the same oxygen volume again must not replay the effect.
        if (Is_fire == 1 && Is_refire == 1)
            return;

        if (Is_fire == 0 && Match_ReFire_Score != null && Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step2())
        {
            Match_ReFire_Score.Yes_Match_ReFire_Step3();
            if (Match_ReFire_Step3 != null)
                Match_ReFire_Step3.color = Color.green;
        }

        CancelPendingExtinction();
        Fire.Stop();
        ReFire.Play();
        ReFire.playRate = 2;
        Smoke.Play();
        marsRenderer.enabled = true;
        dissolveBodyRenderer.enabled = true;
        My_RDisslpate.enabled = true;

        SetPrompt("Restart Fire : O2 + C = CO2 ");
        Can_refire = 1;
        Is_fire = 1;
        Is_refire = 1;
        Flag = 1;
    }

    public void exit_O2()
    {
        if (Is_fire == 0 || Is_refire == 0)
            return;

        Fire.Play();
        ReFire.Stop();
        Smoke.Play();
        Can_refire = 1;
        Is_fire = 1;
        Is_refire = 0;
        Flag = 1;
        marsRenderer.enabled = true;
        dissolveBodyRenderer.enabled = true;
        My_RDisslpate.enabled = true;
        SetPrompt("Exit O2, fire become smaller");
    }

    private void CancelPendingExtinction()
    {
        CancelInvoke("SetDestroyDisslpate");
        CancelInvoke("DelayStopFire");
        DestroyDiss = false;
        DissAdd = false;
        Dissposition = 0.600001F;
    }

    public void DelayStopFire()
    {
        if (Flag != 0)
            return;

        Smoke.Stop();
        marsRenderer.enabled = false;
        SetPrompt("Too late, the fire cannot restart ");
        Can_refire = 0;
        Is_fire = 0;
        Is_refire = 0;
    }

    public void SetDestroyDisslpate()
    {
        if (Flag == 0)
            DestroyDiss = true;
    }

    public void DestroyDisslpate()
    {
        if (Dissposition > 0.1F)
        {
            Dissposition = Mathf.Max(0.1F, Dissposition - 0.06F * Time.deltaTime);
            LabMaterialState.SetDissolve(My_RDisslpate, Dissposition - 0.2F, Dissposition);
        }
        else
        {
            My_RDisslpate.enabled = false;
            Dissposition = 0.600001F;
            DestroyDiss = false;
            DissAdd = false;
        }
    }

    public void OutSpeed()
    {
        if (Is_fire != 1 || matchBody.velocity.magnitude <= 3.5F)
            return;

        stopfire();
        if (Match_ReFire_Score != null && Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step1())
        {
            Match_ReFire_Score.Yes_Match_ReFire_Step2();
            if (Match_ReFire_Step2 != null)
                Match_ReFire_Step2.color = Color.green;
        }
    }

    public void O2_Empty()
    {
        if (Flag == 1)
            stopfire();
        Can_refire = 0;
    }

    public bool C_refire()
    {
        return Can_refire == 1;
    }

    public void AddO2Count()
    {
        if (oxygenSupply != null)
        {
            oxygenSupply.AddOxygen(OxygenPerSecond * Time.fixedDeltaTime);
            O2ParticleCount = oxygenSupply.Amount;
        }
        else
        {
            O2ParticleCount = Mathf.Min(MaxOxygen, O2ParticleCount + OxygenPerSecond * Time.fixedDeltaTime);
        }
        oxygenTextDirty = true;
    }

    public void Start_ReFireStick()
    {
        if (oxygenSupply != null)
        {
            oxygenSupply.InitializeForExperiment();
            O2ParticleCount = oxygenSupply.Amount;
        }
        else
        {
            O2ParticleCount = MaxOxygen;
        }
        UpdateOxygenText("Making O2: Particle Count :");
        oxygenTextDirty = false;
    }

    public bool IfFire()
    {
        return Is_fire == 1;
    }

    private void OnOxygenAmountChanged(float amount)
    {
        O2ParticleCount = amount;
        oxygenTextDirty = true;
    }

    private void OnDestroy()
    {
        if (oxygenSupply != null)
            oxygenSupply.AmountChanged -= OnOxygenAmountChanged;
        LabMaterialState.ClearRenderer(My_RDisslpate);
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    private void OnExperimentReset()
    {
        ResetForExperiment();
    }

    public void ResetForExperiment()
    {
        CancelInvoke();
        Can_refire = 0;
        Is_fire = 0;
        Is_refire = 0;
        Flag = 0;
        DissAdd = false;
        DestroyDiss = false;
        aliveParticleCount = 0;
        O2ParticleCount = oxygenSupply != null ? oxygenSupply.Amount : 0F;
        Off = 0.500001F;
        Dissposition = 0.600001F;
        oxygenTextDirty = true;
        if (matchBody != null)
        {
            matchBody.velocity = Vector3.zero;
            matchBody.angularVelocity = Vector3.zero;
        }
        if (Fire != null)
            Fire.Stop();
        if (ReFire != null)
            ReFire.Stop();
        if (Smoke != null)
            Smoke.Stop();
        if (marsRenderer != null)
            marsRenderer.enabled = false;
        if (dissolveBodyRenderer != null)
            dissolveBodyRenderer.enabled = false;
        if (My_RDisslpate != null)
            My_RDisslpate.enabled = false;
    }

    private void SetPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }

    private void UpdateOxygenText(string prefix)
    {
        if (oxygenText != null)
            oxygenText.text = prefix + O2ParticleCount.ToString("F0");
    }
}
