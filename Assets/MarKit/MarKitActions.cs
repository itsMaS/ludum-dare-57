using MarTools;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace MarKit
{
    public interface IMarKitAction
    {
        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev);
    }

    [System.Serializable]
    public class MarkitDebugAction : IMarKitAction
    {
        public string message;
        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            Debug.Log($"{behavior.gameObject.name}: [{message}]");
        }
    }

    [System.Serializable]
    public class MarkitInvokeUnityEvent : IMarKitAction
    {
        public UnityEvent<IMarkitEventCaller> Event;
        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            Event.Invoke(behavior);
        }
    }

    public class MarkitPlaySound : IMarKitAction
    {
        [SerializeField] AudioClip clip;
        [SerializeField] float volume = 1;
        [SerializeField] float pitchDeviation = 0f;
        
        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            GameObject audioSource = new GameObject($"Audio: {clip.name}");
            var source = audioSource.AddComponent<AudioSource>();

            behavior.behavior.DelayedAction(clip.length, () => GameObject.Destroy(audioSource));

            source.pitch = 1 + Random.value.Remap01(-pitchDeviation, pitchDeviation);
            source.clip = clip;
            source.Play();
        }
    }

    public class SpawnGameObject : IMarKitAction
    {
        public GameObject obj;
        public float destroyAfter = -1;

        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            GameObject instance = GameObject.Instantiate(obj, behavior.transform.position, Quaternion.identity);
            if(destroyAfter > 0)
            {
                GameObject.Destroy(instance, destroyAfter);
            }
        }
    }

    public class ShakeScreen : IMarKitAction
    {
        public float amplitude = 1f;
        public float frequency = 1f;

        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            CinemachineImpulseSource impulseSource;
            if(!behavior.gameObject.TryGetComponent<CinemachineImpulseSource>(out impulseSource))
            {
                impulseSource = behavior.gameObject.AddComponent<CinemachineImpulseSource>();
            }

            impulseSource.ImpulseDefinition = new CinemachineImpulseDefinition
            {
                ImpulseChannel = 1,
                ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
                ImpulseDuration = 0.2f,
                ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
                DissipationRate = 0.25f,
                AmplitudeGain = amplitude,
                FrequencyGain = frequency,
                RepeatMode = CinemachineImpulseDefinition.RepeatModes.Stretch,
                Randomize = true,
                ImpactRadius = 100,
                DissipationDistance = 100,
                PropagationSpeed = 343,
            };

            impulseSource.GenerateImpulseWithForce(amplitude);
        }
    }
}
