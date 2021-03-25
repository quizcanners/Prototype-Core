
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class IsItGameScriptableObjectBase : ScriptableObject
    {
        private GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected GameEntities GameStates => Mgmt.Entities;
        protected GameAssets GameAssets => Mgmt.Assets;
    }
}
