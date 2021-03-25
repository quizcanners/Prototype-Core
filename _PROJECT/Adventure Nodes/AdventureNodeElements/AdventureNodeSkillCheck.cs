using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.NodeNotes;
using System;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class AdventureNodeSkillCheck : IPEGI, IGotName
    {
        public string Name;
        public SkillCheck Check;
        public ConditionBranch Condition = new ConditionBranch();

        public string NameForInspector 
        { 
            get => Name; 
            set => Name = value; 
        }

        public void Inspect()
        {
            "Name".edit(60, ref Name).nl();
            Check.Nested_Inspect().nl();
            pegi.line();
            Condition.Nested_Inspect().nl();
        }
    }
}
