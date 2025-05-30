using MarTools;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
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
        [SerializeField] AudioMixerGroup group;
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
            source.volume = volume;

            source.outputAudioMixerGroup = group;
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

    [Name("Juice/Flash enemies")]
    [System.Serializable]
    public class TweenFlashParameters : IMarKitAction
    {
        public string flashParameter = "_OverlayProgress";

        private Renderer[] renderers = null;
        private MaterialPropertyBlock[] propertyBlocks = null;
        private Coroutine cor = null;

        public void Invoke(IMarkitEventCaller behavior, MarKitEvent ev)
        {
            if (renderers == null)
            {
                renderers = behavior.gameObject.GetComponentsInChildren<Renderer>();
                propertyBlocks = new MaterialPropertyBlock[renderers.Length];
            }

            if (cor != null)
            {
                behavior.behavior.StopCoroutine(cor);
                cor = null;
            }

            cor = behavior.behavior.DelayedAction(0.25f, null, t =>
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.SetFloat(flashParameter, 1-t);
                }
            });
        }
    }
}
