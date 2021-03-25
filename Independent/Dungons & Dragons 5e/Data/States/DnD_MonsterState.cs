using System;
using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class MonsterState : IPEGI, IPEGI_ListInspect, IGotReadOnlyName
    {
        protected static DnDPrototypesScriptableObject Data 
            => Service.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);

        public MonsterSmartId MonsterName = new MonsterSmartId();
        
        public int Hp = 1;
        public int MaxHp = 1;
        
        public void Inspect()
        {
            MonsterName.Nested_Inspect();
        }

        public void InspectInList(ref int edited, int ind)
        {
            MonsterName.enter_Inspect_AsList(ref edited, ind);

            if (icon.Enter.Click())
                edited = ind;
        }

        public string GetNameForInspector() => pegi.GetNameForInspector(MonsterName);
        
            
        
    }
}