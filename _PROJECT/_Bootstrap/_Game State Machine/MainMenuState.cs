namespace QuizCanners.IsItGame.StateMachine
{
    public class MainMenuState : BaseState, IState<IigEnum_Music>, IState<IigEnum_UiView>
    {
        public IigEnum_Music Get() => IigEnum_Music.MainMenu;
        IigEnum_UiView IState<IigEnum_UiView>.Get() => IigEnum_UiView.MainMenu;      
    }
}
