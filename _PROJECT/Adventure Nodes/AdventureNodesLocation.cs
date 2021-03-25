using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class AdventureNodesLocation : ScriptableObject, IPEGI, IGotName
    {
        public const string FILE_NAME = "Adventure Location";

        public List<TableRollResult> RandomRollResults = new List<TableRollResult>();
        public List<AdventureNodeSkillCheck> SkillChecks = new List<AdventureNodeSkillCheck>();
        [SerializeField] private string _key;

        #region Inspector
        private int _inspectedStuff = -1;
        private int _inspectedTableRollResult = -1;
        private int _inspectedSkill = -1;

        public string NameForInspector { get => _key; set => _key = value; }

        public void Inspect()
        {
            pegi.nl();

            int groupIndex = -1;

            "Random Roll Results".enter_List(RandomRollResults, ref _inspectedTableRollResult, ref _inspectedStuff, ++groupIndex).nl();
            "Skill Checks".enter_List(SkillChecks, ref _inspectedSkill, ref _inspectedStuff, ++groupIndex).nl();

        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(AdventureNodesLocation))] internal class AdventureNodesLocationDrawer : PEGI_Inspector_Override { }
}