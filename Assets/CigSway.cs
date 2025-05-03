using UnityEngine;

public class CigSway : MonoBehaviour
{
    [Header("Tuning (tiny numbers!)")]
    public float posScale = 0.002f;   // metres per degree of look velocity
    public float rotScale = 1.0f;    // degrees of roll per degree of look velocity
    public float damping = 6f;      // spring return speed
    public float maxPos = 0.04f;    // hard clamp in metres
    public float maxRoll = 5f;      // hard clamp in degrees

    Vector3 velocity;
    Vector3 targetLocalPos;
    Quaternion targetLocalRot;
    Quaternion prevCamRot;
    Transform camT;

    void Awake()
    {
        camT = transform.parent;          // MainCamera
        prevCamRot = camT.localRotation;
    }

    void LateUpdate()
    {
        /* 1 ? compute camera-space angular velocity (degrees/frame) */
        Quaternion delta = camT.localRotation * Quaternion.Inverse(prevCamRot);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        Vector3 angVel = axis * angle;          // signed degrees
        prevCamRot = camT.localRotation;

        /* 2 ? map that to a desired offset/roll (small numbers) */
        targetLocalPos = new Vector3(
             -angVel.y * posScale,              // yaw ? horizontal sway
             -angVel.x * posScale,              // pitch ? vertical sway
              0);

        targetLocalPos = Vector3.ClampMagnitude(targetLocalPos, maxPos);

        float roll = Mathf.Clamp(-angVel.y * rotScale, -maxRoll, maxRoll);
        targetLocalRot = Quaternion.Euler(0, 0, roll);

        /* 3 ? critically damped spring toward the target */
        transform.localPosition = SmoothDampSpring(
                transform.localPosition, targetLocalPos,
                ref velocity, damping);

        transform.localRotation = Quaternion.Slerp(
                transform.localRotation, targetLocalRot,
                Time.deltaTime * damping);
    }

    /* smallest stable critically-damped spring */
    static Vector3 SmoothDampSpring(
            Vector3 current, Vector3 target,
            ref Vector3 vel, float k)
    {
        Vector3 f = vel - (current - target) * (k * k);
        vel -= f * Time.deltaTime;
        return current + vel * Time.deltaTime;
    }
}
