using QuizCanners.Inspect;

namespace QuizCanners.IsItGame
{
    public class IsItGameClassBase 
    {
        private GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected GameEntities GameEntities => Mgmt.Entities;
        protected GameAssets GameAssets => Mgmt.Assets;
        protected StateMachine.Manager GameState => Mgmt.StateMachine;
    }
}
