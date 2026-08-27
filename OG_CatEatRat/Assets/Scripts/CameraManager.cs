using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Coal;

    public float globalShakeForce = 1.0f; 
    public CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        if (Coal == null)
        {
            Coal = this;
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
    public void CameraShake(CinemachineImpulseSource impulseSource, float shakeForce)
    {
        impulseSource.GenerateImpulseWithForce(shakeForce);
    }

    public void ScreenShakeFromProfile(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        //appply setttings
        ScreenShakeSettings(profile, impulseSource);

        //scree
        impulseSource.GenerateImpulseWithForce(profile.impactForce);
    }

    private void ScreenShakeSettings(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.ImpulseDefinition;

        impulseDefinition.ImpulseDuration = profile.impactTime;
        impulseSource.DefaultVelocity = profile.defaultVelocity; 
        impulseDefinition.CustomImpulseShape = profile.impulseCureve;

        impulseListener.ReactionSettings.AmplitudeGain = profile.listenerAmplitude;
        impulseListener.ReactionSettings.FrequencyGain = profile.listenerFrequency;
        impulseListener.ReactionSettings.Duration = profile.listenerDurantion;
        impulseListener.ReactionSettings.m_SecondaryNoise = profile.secondaryNoise;
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
