using UnityEngine;

public class Wobble : MonoBehaviour
{
    private Renderer rend;
    private MaterialPropertyBlock wobbleProperties;
    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 lastRot;
    private Vector3 angularVelocity;

    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float pulse;
    private float time = 0.5f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        wobbleProperties = new MaterialPropertyBlock();
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0F)
            return;

        time += deltaTime;
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, deltaTime * Recovery);

        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        rend.GetPropertyBlock(wobbleProperties);
        wobbleProperties.SetFloat("_WobbleX", wobbleAmountX);
        wobbleProperties.SetFloat("_WobbleZ", wobbleAmountZ);
        rend.SetPropertyBlock(wobbleProperties);

        velocity = (lastPos - transform.position) / deltaTime;
        Vector3 currentRot = transform.rotation.eulerAngles;
        angularVelocity = new Vector3(
            Mathf.DeltaAngle(lastRot.x, currentRot.x),
            Mathf.DeltaAngle(lastRot.y, currentRot.y),
            Mathf.DeltaAngle(lastRot.z, currentRot.z));

        wobbleAmountToAddX += Mathf.Clamp((velocity.x + angularVelocity.z * 0.2f) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + angularVelocity.x * 0.2f) * MaxWobble, -MaxWobble, MaxWobble);

        lastPos = transform.position;
        lastRot = currentRot;
    }
}
