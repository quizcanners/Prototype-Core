using PlaytimePainter;
using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizCanners.IsItGame.SpecialEffects
{
    public class BlurTransitionSimple : MonoBehaviour, IPEGI
    {
        [SerializeField] private Image _blurImage;
        [SerializeField] private ScreenBlurController.ProcessCommand mode = ScreenBlurController.ProcessCommand.Blur;

        public void SetObscure(Action onDone) 
        {
            Service.Try<ScreenBlurController>(x => x.RequestUpdate(onFirstRendered: () =>
            {
                _blurImage.TrySetAlpha(1);
                onDone?.Invoke();
            }, afterScreenGrab: mode));           
        }

        public void Transition(Action onHidden)
        {
            Service.Try<ScreenBlurController>(x => x.RequestUpdate(onFirstRendered: () =>
            {
                _blurImage.TrySetAlpha(1);
                onHidden?.Invoke();
                enabled = true;
            }, afterScreenGrab: mode));
        }

        public void Reveal() => enabled = true;

        void Awake()
        {
            _blurImage.TrySetAlpha(0);
            enabled = false;
        }

        void Reset() 
        {
            if (!_blurImage)
                _blurImage = GetComponent<Image>();
        }

        void Update()
        {
            if (_blurImage.color.a > 0)
            {
                _blurImage.IsLerpingAlphaBySpeed(0, 0.7f);
            } else 
            {
                enabled = false;
            }
        }

        public void Inspect()
        {
            if ("Transition Test".Click())
                Transition(null);
        }
    }
    
    [PEGI_Inspector_Override(typeof(BlurTransitionSimple))]
    internal class BlurTransitionSimpleDrawer : PEGI_Inspector_Override { }
}
