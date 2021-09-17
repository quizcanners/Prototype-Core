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

        [SerializeField] private BigTextEditable _shortSentence = new BigTextEditable();

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

        public override void UpdatePrototypes()
        {
            base.UpdatePrototypes();

            foreach (var el in Tables)
                if (el)
                    el.UpdatePrototypes();
        }

        #region Inspector
        private int _inspectedTable = -1;

        public override string GetRolledElementName(RolledTable.Result result) => Sentence.GetRolledElementName(result.SubResultsList, Tables);
   
        public void Inspect()
        {
            if (_inspectedTable == -1)
            {
                if (pegi.Click(UpdatePrototypes).nl())
                    Debug.Log("Update Prototypes Started");

                "Short Text".write(90);
                _shortSentence.Nested_Inspect(fromNewLine: false);
                if (!_shortSentence.Value.IsNullOrEmpty() && !_shortSentence.Editing)
                    _shortSentence.Value.write(PEGI_Styles.OverflowText);
                pegi.nl();

                "Long Text".write(90);
                Sentence.Nested_Inspect(fromNewLine: false);
                if (!Sentence.Value.IsNullOrEmpty() && !Sentence.Editing)
                    Sentence.Value.write(PEGI_Styles.OverflowText);
                pegi.nl();
            }

            pegi.nl();
            "Tables to execute".edit_List_UObj(Tables, ref _inspectedTable).nl();
        }

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            string shortName = _shortSentence.Value.IsNullOrEmpty() ? "" : _shortSentence.GetRolledElementName(result.SubResultsList, Tables);

            if (icon.Enter.Click() || "{0} | {1} {2}".F(index, this.GetNameForInspector().Replace("Random ", ""), shortName).ClickLabel())
                edited = index;

        }

        protected override void InspectInternal(RolledTable.Result result)
        {
            if (_inspectedTable > result.SubResultsList.Count)
                _inspectedTable = -1;

            if (_inspectedTable == -1)
            {
                pegi.line();

                "Short Text".write(90);
                _shortSentence.Nested_Inspect(fromNewLine: false);
                if (!_shortSentence.Value.IsNullOrEmpty())
                    _shortSentence.GetRolledElementName(result.SubResultsList, Tables).write(PEGI_Styles.OverflowText);
                pegi.nl();

                "Long Text".write(90);
                Sentence.Nested_Inspect(fromNewLine: false);
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

    }

    [PEGI_Inspector_Override(typeof(GroupOfRollTables))] internal class GroupOfRollTablesDrawer : PEGI_Inspector_Override { }
}
