using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class EntityPrototypes : IsItGameScriptableObjectBase, IPEGI
    {
        public const string FILE_NAME = "Entity Prototypes";

        public List<LevelPrototype> Levels = new List<LevelPrototype>();

        #region Inspector
        private readonly CollectionMetaData _levelsListMeta = new CollectionMetaData("Levels");

        private int _inspecteedStuff = -1;
        public void Inspect()
        {
            _levelsListMeta.enter_List(Levels, ref _inspecteedStuff, 0).nl();

        }
        #endregion
    }
}