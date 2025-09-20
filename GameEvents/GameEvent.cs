using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace Proselyte.Sigils
{
    [CreateAssetMenu]
    public class GameEvent : ScriptableObject
    {
        private readonly List<UnityAction> listeners = new();

        public void Raise()
        {
            Debug.Log("Raising Event: " + name);
            foreach(var listener in listeners)
            {
                listener.Invoke();
            }
        }

        public void RegisterListener(UnityAction listener)
        {
            if(!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(UnityAction listener)
        {
            if(listeners.Contains(listener))
                listeners.Remove(listener);
        }
    }
}
