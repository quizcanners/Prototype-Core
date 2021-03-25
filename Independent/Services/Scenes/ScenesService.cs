using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    [ExecuteAlways]
    public class ScenesService : IsItGameServiceBase, INeedAttention, Service.ILoadingProgressForInspector
    {
        [SerializeField] private SceneManagerScriptableObject _scenes;

        public void Update()
        {
            if (TryEnterIfStateChanged()) 
            {
                var type = IigEnum_Scene.None;
                GameStateMachine.TryChange(ref type);
                _scenes.LoadOnly(type);
            }
        }

        public bool IsLoading(ref string state, ref float progress01)
        {
            if (_scenes)
                return _scenes.IsLoading(ref state, ref progress01);

            return false;
        }

        #region Inspector
        private int _inspectedStuff = -1;

        public override void Inspect()
        {
            pegi.nl();
            "Scenes".edit_enter_Inspect(ref _scenes, ref _inspectedStuff, 0).nl();
        }

        public override string NeedAttention()
        {
            if (!_scenes)
                return "Scenes not assigned";

            var na = _scenes.NeedAttention();

            if (na.IsNullOrEmpty())
                return na;

            return base.NeedAttention();
            
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(ScenesService))] internal class ScenesServiceDrawer : PEGI_Inspector_Override { }
}