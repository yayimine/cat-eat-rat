using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "ScreenShake/New Profile")]
public class ScreenShakeProfile : ScriptableObject
{
    [Header("Impulse Source Settings")]
    public float impactTime = 0.2f;
    public float impactForce = 1.0f;
    public Vector3 defaultVelocity = new Vector3(0.0f,-1.0f,0.0f);
    public AnimationCurve impulseCureve;

    [Header("Impulse Listener Settings")]
    public float listenerAmplitude = 1.0f;
    public float listenerFrequency = 1.0f;
    public float listenerDurantion = 1.0f;
    public NoiseSettings secondaryNoise;
}
