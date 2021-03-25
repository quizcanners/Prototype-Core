using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Dungeons_and_Dragons
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = TABLE_CREATE_NEW_PATH  + FILE_NAME)]
    public class GroupOfRollTables : RandomElementsRollTables, IPEGI
    {
        public const string FILE_NAME = "Group of Roll Tables";

        [SerializeField] private BigTextEditable Sentence = new BigTextEditable();
        public List<RandomElementsRollTables> Tables = new List<RandomElementsRollTables>();


        public override bool TryGetConcept<CT>(out CT value, RolledTable.Result result) 
        {
            for (int i = 0; i < Tables.Count; i++)
            {
                var t = Tables[i];
                var r = result.SubResultsList.TryGet(i);

                if (r!= null) 
                {
                    if (t.TryGetConcept(out value, r))
                        return true;
                }
            }

            return base.TryGetConcept(out value, result);
        }


        #region Inspector
        private int _inspectedTable = -1;

        public override string GetRolledElementName(RolledTable.Result result)
        {
        
            var arguments = new List<string>();

                
            for (int i = 0; i < Tables.Count; i++)
            {
                var t = Tables[i];
                var r = result.SubResultsList.TryGet(i);
       
                if (r == null) 
                {
                    arguments.Add(" Not rolled");
                    break;
                }
                arguments.Add(t.GetRolledElementName(r));
            }

            if (!Sentence.Value.IsNullOrEmpty())
            {
                try 
                {
                    var res = string.Format(Sentence.Value, arguments.ToArray());
                    return res;

                } catch {}
            }
            
            var sb = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                var t = arguments[i];
                sb.Append(t);
                sb.Append(' ');
            }

            return sb.ToString();
          
        }

        public void Inspect()
        {

            Sentence.Nested_Inspect();

            if (!Sentence.Value.IsNullOrEmpty() && !Sentence.Editing)
                Sentence.Value.write(PEGI_Styles.OverflowText);

            pegi.nl();
            "Tables to execute".edit_List_UObj(Tables, ref _inspectedTable).nl();
        }

        protected override void InspectInternal(RolledTable.Result result)
        {
            if (_inspectedTable > result.SubResultsList.Count)
                _inspectedTable = -1;

            if (_inspectedTable == -1)
            {

                pegi.line();

                Sentence.Nested_Inspect();

                if (!Sentence.Value.IsNullOrEmpty()) 
                    GetRolledElementName(result).write(PEGI_Styles.OverflowText);

                pegi.nl();

                pegi.line();

                for (int i = 0; i < Tables.Count; i++)
                {
                    RandomElementsRollTables t = Tables[i];

                    t.InspectInList(ref _inspectedTable, i, result.SubResultsList.GetOrCreate(i));

                    pegi.nl();
                }
            } 
            else 
            {
                pegi.nl();
                if (icon.Back.Click())
                    _inspectedTable = -1;
                else 
                    Tables[_inspectedTable].Inspect(result.SubResultsList.GetOrCreate(_inspectedTable));
            }

        }

        protected override void RollInternal(RolledTable.Result result)
        {
            result.Roll = RollResult.From(0);

            result.SubResultsList.Clear();
            for (int i=0; i<Tables.Count; i++) 
            {
                var res = new RolledTable.Result();
                result.SubResultsList.Add(res);

                Tables[i].Roll(res);
            }
        }
        #endregion


        [Serializable]
        public class BigTextEditable : IPEGI
        {
            public string Value;
            [SerializeField] public bool Editing;

            public void Inspect()
            {
                pegi.nl();
                if (Editing)
                {
                    pegi.editBig(ref Value);
                    if (icon.Done.Click())
                        Editing = false;

                    "<- Click when you are done".writeHint();
                    pegi.nl();
                }
                else
                {
                    if (Value.IsNullOrEmpty())
                    {
                        if ("Add Description".Click().nl())
                            Editing = true;
                    }
                    else
                    {
                        if (icon.Edit.Click())
                            Editing = true;
                    }
                }
            }
        }

    }

    [PEGI_Inspector_Override(typeof(GroupOfRollTables))] internal class GroupOfRollTablesDrawer : PEGI_Inspector_Override { }
}
