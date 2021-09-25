using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    [DisallowMultipleComponent]
    [ExecuteAlways]
    public partial class SpecialEffectShadersService : Utils.Service.BehaniourBase
    {
        [SerializeField] public EffectsRandomSessionSeedManager RandomSeed = new EffectsRandomSessionSeedManager();
        [SerializeField] public EffectsTimeManager EffectsTime = new EffectsTimeManager();
        [SerializeField] public GyroscopeParallaxManager GyroscopeParallax = new GyroscopeParallaxManager();
        [SerializeField] public EffectsMousePositionManager MousePosition = new EffectsMousePositionManager();
        [SerializeField] public NoiseTextureManager NoiseTexture = new NoiseTextureManager();

        #region Feeding Events

        public void OnViewChange() 
        {
            EffectsTime.OnViewChange();
        }

        protected void LateUpdate()
        {
            EffectsTime.ManagedLateUpdate();
            GyroscopeParallax.ManagedLateUpdate();
            MousePosition.ManagedLateUpdate();
        }

        private void OnApplicationPause(bool state)
        {
            EffectsTime.OnApplicationPauseManaged(state);
        }

        protected override void AfterEnable()
        {
            RandomSeed.ManagedOnEnable();
            GyroscopeParallax.ManagedOnEnable();
            MousePosition.ManagedOnEnable();
            NoiseTexture.ManagedOnEnable();

            if (!Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.update += LateUpdate;
                #endif
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.update -= LateUpdate;
                #endif
            }
        }


        #endregion

        #region Inspector

        [SerializeField] private int _inspectedManager = -1;

        public override void Inspect()
        {
            pegi.nl();
            int sectionIndex = -1;

            RandomSeed.enter_Inspect_AsList(ref _inspectedManager, ++ sectionIndex).nl();
            EffectsTime.enter_Inspect_AsList(ref _inspectedManager, ++ sectionIndex).nl();
            GyroscopeParallax.enter_Inspect_AsList(ref _inspectedManager, ++ sectionIndex).nl();
            MousePosition.enter_Inspect_AsList(ref _inspectedManager, ++sectionIndex).nl();
            NoiseTexture.enter_Inspect_AsList(ref _inspectedManager, ++sectionIndex).nl();
        }
        #endregion

    }

    [PEGI_Inspector_Override(typeof(SpecialEffectShadersService))] internal class SpecialEffectShadersServiceDrawer : PEGI_Inspector_Override { }
}