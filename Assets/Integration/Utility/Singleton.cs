using System;
using UnityEngine;

namespace Universal
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T inst { get; private set; }
        /// <summary>
        /// False by default.
        /// </summary>
        internal virtual bool DoNotDestroyOnLoad => false;
        internal virtual void Awake()
        {
            if (inst != null)
            {
                Destroy(this);
                return;
            }

            inst = this as T;
            
            if(DoNotDestroyOnLoad)
                DontDestroyOnLoad(this);
        }
        internal virtual void OnDestroy()
        {
            if (inst == this) inst = null;
        }
    }
}