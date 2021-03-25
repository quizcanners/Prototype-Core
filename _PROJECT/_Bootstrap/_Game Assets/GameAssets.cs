using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class GameAssets : IsItGameScriptableObjectBase, IPEGI
    {
        public const string FILE_NAME = "Game Assets";

        #region Inspector
        public void Inspect()
        {

        }
        #endregion
    }
}