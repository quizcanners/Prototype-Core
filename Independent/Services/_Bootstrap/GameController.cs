using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class GameController : MonoBehaviour, IPEGI
    {
        public const string PROJECT_NAME = "Is It A Game";

        public static GameController instance;

        public StateMachine.Manager StateMachine = new StateMachine.Manager();
        public Services.ServiceBootsrap Services = new Services.ServiceBootsrap();

        [Header("Scriptable Objects")]
        public EntityPrototypes EntityPrototypes;
        public GameEntities Entities;
        public GameAssets Assets;

        #region Inspector
        private int _inspectedStuff = -1;
        public void Inspect()
        {

            if (_inspectedStuff == -1)
                "GAME CONTROLLER".write(PEGI_Styles.ListLabel);

            pegi.nl();

            int sectionIndex = -1;

            "Services".enter_Inspect(Services, ref _inspectedStuff, ++sectionIndex).nl();  // Game Flow & Data independent logic
            "State Machine".enter_Inspect(StateMachine, ref _inspectedStuff, ++sectionIndex).nl();  // Game Flow logic.
            "Entities".edit_enter_Inspect(ref Entities, ref _inspectedStuff, ++sectionIndex).nl();  // Game Data that changes from run to run
            "Prototypes".edit_enter_Inspect(ref EntityPrototypes, ref _inspectedStuff, ++sectionIndex).nl();  // Game Data that stays persistent
            "Assets".edit_enter_Inspect(ref Assets, ref _inspectedStuff, ++sectionIndex).nl();

            if ("Utils".isEntered(ref _inspectedStuff, ++sectionIndex).nl())
                QcUtils.InspectAllUtils();

            if (_inspectedStuff == -1 && Application.isPlaying) 
                Service.Try<UiViewService>(s => s.InspectCurrentView());
        }
        #endregion

        void Update()
        {
            StateMachine.ManagedUpdate();

            if (Input.GetKey(KeyCode.Escape))
                Application.Quit();
        }

        void LateUpdate() => StateMachine.ManagedLateUpdate();
        void OnEnable()
        {
            instance = this;
            StateMachine.ManagedOnEnable();

            if (Entities)
                Entities.Load();
        }
        void OnDisable()
        {
            StateMachine.ManagedOnDisable();
            if (Entities)
                Entities.Save();
        }

        void Awake() 
        {
            QcDebug.ForceDebugOption(); // To have inspector without building IsDebug
            QcLog.LogHandler.SavingLogs = true;
        }
    }

    [PEGI_Inspector_Override(typeof(GameController))] internal class GameManagerDrawer : PEGI_Inspector_Override { }
}