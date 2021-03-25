
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{


    public abstract class IsItGameOnGuiBehaviourBase : IsItGameBehaviourBase, IPEGI
    {
        public abstract void Inspect();

        private static Gate.Frame _frameGate = new Gate.Frame();
        private pegi.GameView.Window _window = new pegi.GameView.Window(upscale: 2);

        private void OnGUI()
        {
           // if (_frameGate.TryEnter())
           // {
                Service.Try<InspectorOnGuiService>(s =>
                {
                    if (!s.DrawInspectorOnGui)
                    {
                        _window.Render(this);
                    }
                });
           // }
        }
    }

    public class IsItGameBehaviourBase : MonoBehaviour
    {
        protected GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected GameEntities GameEntities => Mgmt.Entities;
        protected GameAssets GameAssets => Mgmt.Assets;
        protected StateMachine.Manager GameStateMachine => Mgmt.StateMachine;
    }

    public class IsItGameServiceBase : Service.BehaniourBase, IGotVersion
    {
        protected GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected GameEntities GameEntities => Mgmt.Entities;
        protected GameAssets GameAssets => Mgmt.Assets;
        protected StateMachine.Manager GameStateMachine => Mgmt.StateMachine;


        public int Version { get; private set; }
        protected void SetDirty() => Version++;

        protected Gate.Integer _checkedStateVersion = new Gate.Integer();
        protected bool TryEnterIfStateChanged() => Application.isPlaying && _checkedStateVersion.TryChange(GameStateMachine.Version);

        public override void Inspect()
        {
            base.Inspect();

            pegi.nl();

            "Checked Version: {0}".F(_checkedStateVersion.CurrentValue).write();

            if (icon.Refresh.Click())
            {
                _checkedStateVersion.TryChange(-1);
            }

            pegi.nl();

        }

    }
}
