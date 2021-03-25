using System;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    /*
    public abstract class LogicMGMT : MonoBehaviour, IPEGI   {

        public static LogicMGMT instLogicMgmt;

        private bool _waiting;
        private float _timeToWait = -1;
        private static int _currentLogicVersion;

        public static int CurrentLogicVersion => _currentLogicVersion;
        
        public static void AddLogicVersion() {
            _currentLogicVersion++;
            if (instLogicMgmt)
                instLogicMgmt.OnLogicVersionChange();
        }

        private static int _realTimeOnStartUp;

        public static int RealTimeNow()
        {
            if (_realTimeOnStartUp == 0)
                _realTimeOnStartUp = (int)((DateTime.Now.Ticks - 733000 * TimeSpan.TicksPerDay) / TimeSpan.TicksPerSecond);

            return _realTimeOnStartUp + (int)QcUnity.TimeSinceStartup();
        }

        public virtual void OnEnable()  =>  instLogicMgmt = this;
        
        public void AddTimeListener(float seconds) {
            seconds += 0.5f;
            _timeToWait = !_waiting ? seconds : Mathf.Min(_timeToWait, seconds);
            _waiting = true;
        }

        protected virtual void DerivedUpdate() { }

        public abstract void OnLogicVersionChange();

        public void Update()
        {
            if (_waiting)
            {
                _timeToWait -= Time.deltaTime;
                if (_timeToWait < 0)
                {
                    _waiting = false;
                    AddLogicVersion();
                }
            }

            DerivedUpdate();
        }

        public void Awake() => RealTimeNow();

        #region Inspector

        protected virtual void ResetInspector() {
            inspectedTriggerGroup = -1;
        }

        [SerializeField] protected int inspectedTriggerGroup = -1;
        [SerializeField] protected int tmpIndex = -1;
       // [NonSerialized] private TriggerGroup _replaceReceived;
        [NonSerialized] private bool _inspectReplacementOption;
        public virtual void Inspect()
        {
            pegi.nl();
            
            if (inspectedTriggerGroup == -1) {

                #region Paste Options

                if ("Paste Options".isFoldout().nl())
                {

                    if (_replaceReceived != null)
                    {

                        var current = TriggerGroup.all.GetIfExists(_replaceReceived.IndexForInspector);
                        var hint = (current != null)
                            ? "{0} [ Old: {1} => New: {2} triggers ] ".F(_replaceReceived.NameForInspector, current.Count,
                                _replaceReceived.Count)
                            : _replaceReceived.NameForInspector;

                        if (hint.isEntered(ref _inspectReplacementOption))
                            _replaceReceived.Nested_Inspect();
                        else
                        {
                            if (icon.Done.ClickUnFocus())
                            {
                                TriggerGroup.all[_replaceReceived.IndexForInspector] = _replaceReceived;
                                _replaceReceived = null;
                            }

                            if (icon.Close.ClickUnFocus())
                                _replaceReceived = null;
                        }
                    }
                    else
                    {

                        var tmp = "";
                        if ("Paste Messaged STD data".edit(140, ref tmp) || ICfgExtensions.DropStringObject(out tmp))
                        {

                            var group = new TriggerGroup();
                            group.DecodeFromExternal(tmp);

                            var current = TriggerGroup.all.GetIfExists(group.IndexForInspector);

                            if (current == null)
                                TriggerGroup.all[group.IndexForInspector] = group;
                            else
                            {
                                _replaceReceived = group;
                                if (!_replaceReceived.NameForInspector.SameAs(current.NameForInspector))
                                    _replaceReceived.NameForInspector += " replaces {0}".F(current.NameForInspector);
                            }
                        }



                    }

                    pegi.nl();
                }

                #endregion

                "Trigger Groups".nl(PEGI_Styles.ListLabel);
            }

            ExtensionsForGenericCountless.Inspect<UnNullableCfg<TriggerGroup>, TriggerGroup>(TriggerGroup.all, ref inspectedTriggerGroup);

            if (inspectedTriggerGroup == -1)
            {
                "At Index: ".edit(60, ref tmpIndex);
                if (tmpIndex >= 0 && TriggerGroup.all.TryGet(tmpIndex) == null && icon.Add.ClickUnFocus("Create New Group"))
                {
                    TriggerGroup.all[tmpIndex].NameForInspector = "Group " + tmpIndex;
                    tmpIndex++;
                }
                pegi.nl();

                "Adding a group will also try to load it".writeHint();
                pegi.nl();
            }
            
            pegi.nl();

        }

        #endregion
    }
    */
}