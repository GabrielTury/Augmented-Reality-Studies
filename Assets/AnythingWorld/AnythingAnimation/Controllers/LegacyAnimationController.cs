using AnythingWorld.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AnythingWorld.Animation
{




    using Animation = UnityEngine.Animation;
    public class LegacyAnimationController : MonoBehaviour
    {
        // Start is called before the first frame update
        public Animation animationPlayer;
        public List<string> loadedAnimations = new List<string>();
        public float crossfadeTime = 0.01f;
        public List<AWAnimationState> animationStates = new List<AWAnimationState>();
        public List<AnimationState> anims = new List<AnimationState>();
        public float animationScale = 1;

        public void CrossFadeAnimation(string animationName)
        {
            if (this == null) return;
            //Debug.Log($"Contains animation: {animationName}");
            if (loadedAnimations.Contains(animationName))
            {
                //Debug.Log("true");
                if(TryGetComponent<Animation>(out var animation))
                {
                    animation.CrossFade(animationName, crossfadeTime);
                }
            }

        }
        public void PlayAnimation(string animationName)
        {
            if (this == null) return;
            if (loadedAnimations.Contains(animationName))
            {
                if (TryGetComponent<Animation>(out var animation))
                {
                    animation.Play(animationName);
                    
                }
            }
        }

        public IEnumerator Wait(float seconds, Action callback)
        {
            yield return CoroutineExtension.WaitForSeconds(seconds);
            callback?.Invoke();
        }
        public void StopAnimations()
        {
            if (TryGetComponent<Animation>(out var animation))
            {
                animation.Stop();
            }
        }
    }

    public struct AWAnimationState
    {
        public Animation animation;
        public bool animationActive;
        public float animationBlendThreshold;
    }
}
