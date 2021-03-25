using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME +"/Managers/"+ FILE_NAME)]
    public class SimpleOfflineGeneratorTest : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Offline Generator Test";

        [SerializeField] private SerializableDateTime _lastSyncDateTime = new SerializableDateTime();
        [SerializeField] private SerializableTimeSpan _unprocessedOfflineTime = new SerializableTimeSpan();
        [SerializeField] private SerializableTimeSpan _processedTicksWithoutSync = new SerializableTimeSpan();
        [NonSerialized] private TimeSpan _ticksToProcess;

        [NonSerialized] private SerializableTimer _testTimer = new SerializableTimer();
        [NonSerialized] private readonly Gate.Frame _frameGate = new Gate.Frame();
      
        private NetworkTimeService NetworkTime => Service.Get<NetworkTimeService>();

        public void FeedSynchronizationTime(DateTime syncDateTime)
        {
            if (!_lastSyncDateTime.IsSet)
            {
                _lastSyncDateTime = syncDateTime;
                _processedTicksWithoutSync = TimeSpan.Zero;
                return;
            }

            TimeSpan unsynchronizedTimeSpan = syncDateTime - _lastSyncDateTime;
            _lastSyncDateTime = syncDateTime;

            if (unsynchronizedTimeSpan.TotalSeconds < 0) 
            {
                Debug.LogError("Offline Time passed < 0");
                return;
            }

            TimeSpan processedSynchronizedTime = (unsynchronizedTimeSpan < _processedTicksWithoutSync.Value) ? unsynchronizedTimeSpan : _processedTicksWithoutSync.Value;

            // Turn Unvarified time to varified time
            _processedTicksWithoutSync -= processedSynchronizedTime; // Should result in zero
            _unprocessedOfflineTime += (unsynchronizedTimeSpan - processedSynchronizedTime);

            _ticksToProcess = TimeSpan.Zero; // Unprocessed ticks are now accounted by sync

            if (_unprocessedOfflineTime < TimeSpan.Zero) 
            {
                Debug.LogError("Time Span was below zero upon sync");
                _unprocessedOfflineTime = TimeSpan.Zero;
            }

            if (_processedTicksWithoutSync.Value.TotalSeconds > 60) 
            {
                Debug.LogError("Leftover Offline Generation: {0}".F(_processedTicksWithoutSync.Value.ToShortDisplayString()));
                _processedTicksWithoutSync.Value = TimeSpan.Zero;
            }
        }

        public void OnDeltaTimeProcessed() 
        {
            _processedTicksWithoutSync += _ticksToProcess;
            _ticksToProcess = TimeSpan.Zero;
            _unprocessedOfflineTime.Value = TimeSpan.Zero;
        }

        public double UnProcessedTimeDelta
        { 
            get
            {
                if (_frameGate.TryEnter())
                    _ticksToProcess += TimeSpan.FromSeconds(Time.unscaledDeltaTime);
                
                return _ticksToProcess.TotalSeconds + _unprocessedOfflineTime.Value.TotalSeconds;
            }
        }

        private void ResetAll()
        {
            _lastSyncDateTime = new SerializableDateTime();
            _processedTicksWithoutSync = TimeSpan.Zero;
        }

        #region Inspector
        public void Inspect()
        {
            if ("Reset All".ClickConfirm(confirmationTag: "Res All", toolTip: "Will loose all generated resource").nl())
                ResetAll();

            if (NetworkTime != null)
                NetworkTime.Nested_Inspect();
            else
                "{0} service is missing".F(nameof(NetworkTimeService)).writeWarning();

            pegi.nl();

            if (!_lastSyncDateTime.IsSet)
                "NOT SYNCHRONIZED".nl();

            if (NetworkTime != null)
            {
                if (!NetworkTime.IsTimeValid)
                    "Network Time Currently Unknown".write();
                else
                {
                    "(Network_Time - SyncTime): {0} ".F((NetworkTime.Time - _lastSyncDateTime).ToShortDisplayString()).write();
                    if ("Feed".Click())
                        FeedSynchronizationTime(NetworkTime.Time);
                }
            }
            pegi.nl();

            "UnProcessed Time: {0}  = Ticks: {1} + Offline: {2}".F(
                QcSharp.SecondsToReadableString(UnProcessedTimeDelta),
                _ticksToProcess.ToShortDisplayString(),
                _unprocessedOfflineTime.Value.ToShortDisplayString()).writeBig();

            pegi.nl();

            if ("Process".Click().nl())
                OnDeltaTimeProcessed();

            if (icon.Clear.Click())
                _processedTicksWithoutSync.Value = TimeSpan.Zero;

            "UnSynchronized Ticks: {0}".F(_processedTicksWithoutSync.Value.ToShortDisplayString()).nl();

            _testTimer.Nested_Inspect();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(SimpleOfflineGeneratorTest))] internal class SimpleGeneratorTestDrawer : PEGI_Inspector_Override { }

}