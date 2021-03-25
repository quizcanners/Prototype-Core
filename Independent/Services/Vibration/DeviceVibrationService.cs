using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class DeviceVibrationService : Service.ClassBase, IPEGI
    {
        private const string VIBRATION = "ENABLE_VIBRATION";
        private bool _vibrationEnabled;
        private bool _isInitialized;

        public bool VibrationEnabled
        {
            get
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    _vibrationEnabled = PlayerPrefs.GetInt(VIBRATION) != 0;
                    Vibration.Init();
                }

                return _vibrationEnabled;
            }
            set
            {
                _vibrationEnabled = value;
                PlayerPrefs.SetInt(VIBRATION, value ? 1 : 0);
            }
        }

        public void OnPopVibrate()
        {
            if (VibrationEnabled)
            {
                try
                {
                    Vibration.VibratePop();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void OnPeakVibrate()
        {
            if (VibrationEnabled)
            {
                try
                {
                    Vibration.VibratePeek();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void OnNopeVibrate()
        {
            if (VibrationEnabled)
            {
                try
                {
                    Vibration.VibrateNope();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

       public void Inspect()
        {

            if ("VIBRATE".toggleIcon(ref _vibrationEnabled).nl())
                VibrationEnabled = _vibrationEnabled;

            if (_vibrationEnabled)
            {
                if ("Peak".Click())
                {
                    OnPeakVibrate();
                }

                if ("Pop".Click())
                {
                    OnPopVibrate();
                }

                if ("Nope".Click())
                {
                    OnNopeVibrate();
                }
            }
        }
    }
}
