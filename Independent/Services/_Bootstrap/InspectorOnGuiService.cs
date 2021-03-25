using QuizCanners.Inspect;
using QuizCanners.IsItGame.SpecialEffects;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class InspectorOnGuiService : IsItGameServiceBase
    {
        [SerializeField] private List<RectTransform> _InspectorUnderalay;
        public bool DrawInspectorOnGui;
        private readonly pegi.GameView.Window _window = new pegi.GameView.Window(upscale: 2.5f);

        public bool DrawInspector 
        {
            get => DrawInspectorOnGui;
            set 
            {
                DrawInspectorOnGui = value;
                _InspectorUnderalay.SetActive_List(DrawInspectorOnGui);
            }
        }

        public void Toggle()
        {
            if (!DrawInspector) 
            {
                Service.Try<ScreenBlurController>(
                    onFound: s => s.RequestUpdate(() => DrawInspector = true), 
                    onFailed: ()=> DrawInspector = true
                    );
            } else
            {
                DrawInspector = false;
            }
        }

        protected override void AfterEnable()
        {
            gameObject.SetActive(QcDebug.ShowDebugOptions);
            DrawInspector = false;
        }

        void OnGUI()
        {
            if (DrawInspectorOnGui)
            {
                _window.Render(Mgmt);

                switch (pegi.GameView.LatestEvent) 
                {
                    case pegi.LatestInteractionEvent.Click: IigEnum_SoundEffects.Click.Play(); break;
                    case pegi.LatestInteractionEvent.SliderScroll: IigEnum_SoundEffects.Scratch.Play(); break;
                    case pegi.LatestInteractionEvent.Enter: IigEnum_SoundEffects.Tab.Play(); break;
                    case pegi.LatestInteractionEvent.Exit: IigEnum_SoundEffects.MouseExit.Play(); break;
                }
            } else if (QcDebug.ShowDebugOptions) 
            {
                Service.Collector.Inspect_LoadingProgress();
            }
        }
    }
}
