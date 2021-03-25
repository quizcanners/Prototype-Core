namespace QuizCanners.IsItGame.StateMachine
{
    public class SpaceEffectState : BaseState, IState<IigEnum_Scene>, IState<IigEnum_UiView>
    {
        public IigEnum_Scene Get() => IigEnum_Scene.SpaceEffect;
        IigEnum_UiView IState<IigEnum_UiView>.Get() => IigEnum_UiView.SpaceEffect;

    }
}