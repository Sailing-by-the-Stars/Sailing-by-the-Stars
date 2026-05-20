using System;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Lost_Souls
{
    /// <summary>
    /// Helper class to forward trigger events from child colliders to a parent or manager script.
    /// </summary>
    public class TriggerForwarder : MonoBehaviour
    {
        public Action<Collider> onTriggerEnterAction;
        public Action<Collider> onTriggerExitAction;

        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnterAction?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            onTriggerExitAction?.Invoke(other);
        }
    }
}

