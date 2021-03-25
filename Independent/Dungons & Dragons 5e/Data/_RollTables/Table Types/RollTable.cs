using System;
using System.Collections.Generic;
using System.Text;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;


namespace Dungeons_and_Dragons
{
    
    [CreateAssetMenu(fileName = FILE_NAME,  menuName = TABLE_CREATE_NEW_PATH + FILE_NAME)]
    public class RollTable : RollTableGeneric<RollTable.Element>
    {
        public const string FILE_NAME = "Generic";

        public List<Element> elements = new List<Element>();

        private Element this[RolledTable.Result value] => Get(elements, value);

        protected override void RollInternal(RolledTable.Result result) 
        {
            result.Roll = RollDices();
            var el = Get(elements, result.Roll);
            if (el != null)
                el.Roll(result.SubResultsList);
        }


        private bool _inspectingRollResult;
        protected override void InspectInternal(RolledTable.Result result)
        {
            if (icon.Dice.Click())
                Roll(result);

            Element el = Get(elements, result.Roll);
            if (pegi.select(ref el, elements, stripSlashes: true) && el!= null) 
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }
            
            this.ClickHighlight();

            if (el != null && icon.Enter.isEntered(ref _inspectingRollResult).nl())
                el.Inspect(result.SubResultsList);
            
        }

        private int _editedElement = -1;

        protected override List<Element> List { get => elements; set => elements = value; }

        protected override bool EditList() =>
            "{0} {1}".F(_dicesToRoll.ToDescription(), QcSharp.KeyToReadablaString(name.SimplifyTypeName())).edit_List(elements, ref _editedElement).nl();

        public override string GetRolledElementName(RolledTable.Result result)
        {
            if (!result.IsRolled)
                return "Not Rolled";

            var el = Get(elements, result.Roll);
            if (el == null)
                return "NULL for {0}".F(result.Roll);

            return el.GetRolledElementName(result.SubResultsList);
        }

        public override bool TryGetConcept<CT>(out CT value, RolledTable.Result result)
        {
            var el = this[result];

            if (el != null)
            {
                return el.TryGetConcept(out value, result.SubResult);
            }

            return base.TryGetConcept(out value, result);
        }

        [Serializable]
        public class Element : RollTableElementBase, IPEGI, IGotName
        {
            [SerializeField] List<RandomElementsRollTables> subTables = new List<RandomElementsRollTables>();
            public string Name;

            public BigTextEditable Description;

            public string NameForInspector { get => Name; set => Name = value; }

            public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT : IComparable => result.TryGetConcept(out value, subTables);


            public string GetRolledElementName(List<RolledTable.Result> subTableResults)
            {
               StringBuilder sb = new StringBuilder(Name);

               for( int i =0; i<subTables.Count; i++) 
               {
                    var subRoll = subTableResults.TryGet(i);

                    if (subRoll!= null && subRoll.IsRolled) 
                    {
                        sb.Append("| ");
                        sb.Append(subTables[i].GetRolledElementName(subRoll));
                    }
               }

               return sb.ToString();
            }

            public void Roll(List<RolledTable.Result> results) 
            {
                results.Clear();
                foreach(var sub in subTables) 
                {
                    var newRes = new RolledTable.Result();
                    results.Add(newRes);
                    sub.Roll(newRes);
                }
            }

            public override void Decode(string key, CfgData data)
            {
                switch (key) 
                {
                    case "Name":
                    case "name":
                        Name = data.ToString();
                        break;
                    case "Description": Description.Description = data.ToString(); break;
                    default:   base.Decode(key, data); break;
                }
            }

            public override void Inspect(List<RolledTable.Result> subTablesRolls) 
            {
                if (_inspectedSubTable >= subTables.Count)
                    _inspectedSubTable = -1;

                if (subTables.Count == 1)
                    _inspectedSubTable = 0;

                if (_inspectedSubTable == -1)
                {
                    Description.Nested_Inspect(fromNewLine: false);

                    for (int i = 0; i < subTables.Count; i++)
                    {
                        var t = subTables[i];
                        var r = subTablesRolls.GetOrCreate(i);
                        if (icon.Enter.Click() || t.GetRolledElementName(r).ClickLabel().nl())
                            _inspectedSubTable = i;
                    }
                } else 
                {
                    if (subTables.Count>1 && icon.Exit.Click())
                        _inspectedSubTable = -1;
                    else
                    {
                        subTables[_inspectedSubTable].Inspect(subTablesRolls.GetOrCreate(_inspectedSubTable));
                    }
                }
            }

            public override void InspectInList(ref int edited, int ind)
            {
                base.InspectInList(ref edited, ind);
                pegi.edit(ref Name);
                if (subTables.Count < 2)
                {
                    RandomElementsRollTables tmp = subTables.TryGet(0);
                    if (pegi.edit(ref tmp, 90))
                        subTables.ForceSet(0, tmp);
                }
                else
                    "X {0}".F(subTables.Count).write(40);

                if (icon.Enter.Click())
                    edited = ind;
            }

            private int _inspectedSubTable = -1;

            public void Inspect()
            {
                if (_inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);
                

                "Sub Table".edit_List_UObj(subTables, ref _inspectedSubTable);
            }
        }
    }

    [PEGI_Inspector_Override(typeof(RollTable))] internal class RollTableDrawer : PEGI_Inspector_Override { }

}