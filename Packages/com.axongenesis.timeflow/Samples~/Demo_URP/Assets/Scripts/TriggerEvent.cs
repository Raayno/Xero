// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Events;

namespace AxonGenesis
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class TriggerEvent : AxonGenesisBehavior
    {
        public UnityEvent OnTrigger;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if(DebugEnabled) Debug.Log($"{name}.OnTriggerEnter:{other.name}");
            if (OnTrigger != null) OnTrigger.Invoke();
        }
    }
}
