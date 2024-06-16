using System;
using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;

namespace AnythingWorld.Utilities
{
    /// <summary>
    /// Anchor for coroutines in runtime (not in editor)
    /// </summary>
    public static class CoroutineExtension
    {

        private static CoroutineAnchor _anchor;
        /// <summary>
        /// Anchor for coroutines created in runtime
        /// </summary>
        private static CoroutineAnchor Anchor
        {
            get
            {
                if (_anchor)
                {
                    return _anchor;
                }

                _anchor = new GameObject("CoroutineAnchor").AddComponent<CoroutineAnchor>();
                _anchor.hideFlags = HideFlags.HideInHierarchy;
                return _anchor;
            }
        }

        public static void StopCoroutines()
        {
        }


        /// <summary>
        /// Starts coroutine depending on context of engine (editor or runtime)
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="owner">Owner MonoBehaviour script.</param>
        public static void StartCoroutine(IEnumerator enumerator, MonoBehaviour owner)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutine(enumerator, owner);
#else
            owner.StartCoroutine(enumerator);
#endif
        }
        /// <summary>
        /// Starts coroutine depending on context of engine (editor or runtime)
        /// </summary>
        /// <param name="enumerator"></param>
        public static void StartCoroutine(IEnumerator enumerator)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);
#else
            Anchor.StartCoroutine(enumerator);
#endif
        }
        /// <summary>
        /// Waits for seconds depending on context of engine (editor or runtime)
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator WaitForSeconds(float seconds)
        {
#if UNITY_EDITOR
            yield return new EditorWaitForSeconds(seconds);
#else
        yield return new WaitForSeconds(seconds);
#endif

        }
#if UNITY_EDITOR
        public static void StartEditorCoroutine(IEnumerator enumerator, object owner)
        {
            EditorCoroutineUtility.StartCoroutine(enumerator, owner);

        }
        public static void StartEditorCoroutine(IEnumerator enumerator)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

        }
#endif
        /// <summary>
        /// waits for seconds and then invokes action
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="firstDo"></param>
        /// <param name="whenDone"></param>
        /// <returns></returns>
        public static IEnumerator WaitThen(float seconds, Action firstDo, Action whenDone)
        {
            firstDo.Invoke();
#if UNITY_EDITOR
            yield return new EditorWaitForSeconds(seconds);
#else
        yield return new WaitForSeconds(seconds);
#endif
            whenDone?.Invoke();
        }
    }
}

