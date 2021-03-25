
namespace QuizCanners.IsItGame.StateMachine 
{
    public class RayTracingSceneState : BaseState, IState<IigEnum_UiView>, IState<IigEnum_Scene>
    {
        public IigEnum_UiView Get() => IigEnum_UiView.RayTracingView;
        IigEnum_Scene IState<IigEnum_Scene>.Get() => IigEnum_Scene.RayTracing;
    }
}
