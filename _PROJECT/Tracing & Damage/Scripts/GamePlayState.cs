using QuizCanners.IsItGame.StateMachine;

namespace QuizCanners.IsItGame.Develop
{
    public class GamePlayState : BaseState, IState<IigEnum_Music>, IState<IigEnum_Scene>, IState<IigEnum_UiView>
    {
        public IigEnum_Music Get() => IigEnum_Music.Combat;
        IigEnum_Scene IState<IigEnum_Scene>.Get() => IigEnum_Scene.GameScene;
        IigEnum_UiView IState<IigEnum_UiView>.Get() => IigEnum_UiView.BackToMainMenuButton;
    }
}
