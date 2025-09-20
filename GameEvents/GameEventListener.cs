using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace Proselyte.Sigils
{
    public class GameEventListener : MonoBehaviour
    {
        [Serializable]
        public class EventResponse
        {
            public GameEvent gameEvent;
            public UnityEvent response;
        }
        public List<EventResponse> eventResponses;
        private Dictionary<GameEvent, UnityAction> registeredActions = new();

        private void OnEnable()
        {
            foreach(var pair in eventResponses)
            {
                UnityAction action = () => pair.response.Invoke();
                registeredActions[pair.gameEvent] = action;
                pair.gameEvent.RegisterListener(action);
            }
        }

        private void OnDisable()
        {
            foreach(var pair in eventResponses)
            {
                if(registeredActions.TryGetValue(pair.gameEvent, out var action))
                {
                    pair.gameEvent.UnregisterListener(action);
                }
            }
            registeredActions.Clear();
        }
    }
}
