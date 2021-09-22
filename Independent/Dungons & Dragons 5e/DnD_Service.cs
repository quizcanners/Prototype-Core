using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [ExecuteAlways]
    public class DnD_Service : Service.BehaniourBase, ITaggedCfg
    {
        public DnDPrototypesScriptableObject DnDPrototypes;

        #region Cfg

        public string TagForConfig => "DND";

        int test = -1;

        public void Decode(string key, CfgData data)
        {
            switch (key) 
            {
                case "t": test = data.ToInt(); break;
            }
        }

        public CfgEncoder Encode() => new CfgEncoder().Add("t", test);
        #endregion

        #region Inspector


        public override string NeedAttention()
        {
            if (!DnDPrototypes)
                return "No Prototypes";

            return base.NeedAttention();
        }

        public override string GetReadOnlyName() => "Dungeons & Dragons 5e";

        public override void Inspect()
        {
            base.Inspect();

            if (!DnDPrototypes)
                "Prototypes".edit(ref DnDPrototypes).nl();
            else
                DnDPrototypes.Nested_Inspect();
        }

        #endregion
    }
    

  [PEGI_Inspector_Override(typeof(DnD_Service))] internal class DnD_ManagerDrawer : PEGI_Inspector_Override { }

}