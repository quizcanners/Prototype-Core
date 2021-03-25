using QuizCanners.IsItGame.SpecialEffects;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class GyroscopeTestView : MonoBehaviour
    {
        [SerializeField] private RectTransform _gyroOffset;

        private void Update()
        {
            Service.Try<SpecialEffectShadersService>(serv => _gyroOffset.anchoredPosition = serv.GyroscopeParallax.AccumulatedOffset * 512);
        }
    }
}
