// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace AxonGenesis
{
    /// <summary>
    /// An event is triggered at a specific time in the Timeflow playback. Override this class to create
    /// custom event behaviors. Events can broadcast messages or send Unity events, which can happen once
    /// or repeatedly as desired.
    /// </summary>
    [ExecuteInEditMode]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Timeflow Event")]
    public partial class TimeflowEvent : TimeflowBehavior, ITimeflowBehaviorMenu, IBehaviorPresets
    {
        #region PUBLIC

        public GameObject Obj;
        public string Function;
        public string Parameter;

        public int TriggerLimit;
        public bool WasTriggered;

        public bool LogEnabled = false;

        public UnityEvent OnTrigger;

        #endregion

        #region PRIVATE

        [SerializeField]
        private float _TriggerTime;

        [SerializeField]
        private bool _LockTime;

        [SerializeField]
        private string _Name = "Event Name";

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private int triggerCount;

        #endregion

        #region ACCESSORS

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public float TriggerTime {
            get {
                return _TriggerTime;
            }
            set {
                if (!LockTime && _TriggerTime != value) {
                    _TriggerTime = value;
                    if (Timeflow != null && _TriggerTime < Timeflow.StartTime) _TriggerTime = Timeflow.StartTime;
                }
            }
        }

        public float TriggerTimeWorld {
            get {
                return _TriggerTime + ParentObject.TimeOffsetWorld;
            }
            set {
                if (!LockTime) {
                    _TriggerTime = value - ParentObject.TimeOffsetWorld;
                }
            }
        }

        public bool LockTime {
            get {
                return _LockTime;
            }
            set {
                if (_LockTime != value) {
                    _LockTime = value;
                }
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log(name + ".TimeflowEvent.OnAwake:" + Name);
            base.OnAwake();
            Reset();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ".TimeflowEvent.OnDestruct");
            if (ParentObject != null) ParentObject.RemoveEvent(this);
            base.OnDestruct();
        }

        public override void SetupChannels(bool forceSetup)
        {
            // Inentionally override to clear base behavior
        }

        public virtual void Copy(TimeflowEvent evt)
        {
            Name = evt.Name;
            Enabled = evt.Enabled;
            Obj = evt.Obj;
            Function = evt.Function;
            Parameter = evt.Parameter;

            TriggerTime = evt.TriggerTime;
            TriggerLimit = evt.TriggerLimit;
            WasTriggered = evt.WasTriggered;

            if (evt.OnTrigger != null) {
                OnTrigger = evt.OnTrigger.Clone();
                //ObjectUtil.CopyUnityEvents(evt, "OnTrigger", this);
            }

#if UNITY_EDITOR
            GUIRect = evt.GUIRect;
#endif
        }

        #endregion

        #region UPDATE

        public virtual void Reset()
        {
            WasTriggered = false;
            triggerCount = 0;
            lastTime = 0;
        }

        public override void UpdateTime()
        {
            if (!CanUpdate || Timeflow == null) return;
            //if (DebugEnabled) Debug.Log(name + ".TimeflowEvent.UpdateTime:" + CurrentTime + " last:" + lastTime);
            if (Timeflow.IsPlaying && ((lastTime > 0 && CurrentTime > lastTime) || TriggerTime <= 0)) {
                CheckTrigger();
            }
            lastTime = CurrentTime;
            base.UpdateTime();
        }

        public override void OnRewind()
        {
            base.OnRewind();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                // only reset the trigger count when rewinding in the editor
                Reset();
            }
#endif
        }

        #endregion

        #region TRIGGER

        public void CheckTrigger()
        {
            if (!CanUpdate) return;
            //if (DebugEnabled) Debug.Log(name + ".TimeflowEvent.CheckTrigger:" + CurrentTime + " at:" + TriggerTime + " wasT:" + WasTriggered);
            if (Enabled && TriggerTime >= lastTime && TriggerTime <= CurrentTime) {
                //if (DebugEnabled) Debug.Log(name + ".TimeflowEvent EventTrigger:" + Function + ":" + CurrentTime + " last:" + lastTime + " tr:" + TriggerTime);
                Trigger();
            }
        }

        public virtual void Trigger(bool force = false)
        {
            if (!CanUpdate) return;
            if ((force || TriggerLimit == 0 || triggerCount < TriggerLimit)) {
                if(LogEnabled) Debug.Log("TimeflowEvent[" + name + "].Triggered Count:" + triggerCount + " Limit:" + TriggerLimit + " Force:" + force);
                triggerCount++;
                WasTriggered = true;

                if (!string.IsNullOrEmpty(Function)) {
                    if (Obj == null) Obj = gameObject;
                    //if (DebugEnabled) Debug.Log("TimeflowEvent[" + name + "]." + Function + "(" + Parameter + ")");
                    Obj.SendMessage(Function, Parameter, SendMessageOptions.DontRequireReceiver);
                }
                OnTriggered();
                if (OnTrigger != null) {
                    //if (DebugEnabled) Debug.Log("TimeflowEvent[" + name + "].OnTrigger.Invoke");
                    OnTrigger.Invoke();
                }
            }
        }

        public virtual void OnTriggered()
        {
            /// Override in derrived classes to implement custom behavior
        }

        #endregion

#if UNITY_EDITOR

        [NonSerialized]
        private bool _IsSelected;

        public override bool IsSelected {
            get {
                if (Timeflow.Active != null) {
                    _IsSelected = Timeflow.Active.View.SelectedEvents != null && Timeflow.Active.View.SelectedEvents.Contains(this);
                }
                return _IsSelected;
            }
            set => _IsSelected = value;
        }
#endif
    }

}//AxonGenesis
