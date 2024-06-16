using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Utilities
{
    public static class Destroy
    {

        public static void GameObject(GameObject model)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(model);
#else
            Object.Destroy(model);
#endif
        }
        public static void MonoBehaviour(MonoBehaviour script)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(script);
#else
            Object.Destroy(script);
#endif
        }
    }
}
