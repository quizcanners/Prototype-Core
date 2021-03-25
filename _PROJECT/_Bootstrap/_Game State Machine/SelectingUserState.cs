using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    public class SelectingUserState : BaseState, IState<IigEnum_UiView>
    {
        public IigEnum_UiView Get() => IigEnum_UiView.SelectUser;

        internal override void Update()
        {
            if (GameEntities.Player.PlayerName.IsNullOrEmpty() == false)
            {
                Exit();
            }
        }
    }
}
