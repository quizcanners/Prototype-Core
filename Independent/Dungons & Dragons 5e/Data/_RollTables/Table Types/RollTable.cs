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

        protected override void InspectInternal(RolledTable.Result result)
        {
            if (icon.Dice.Click())
                Roll(result);

            Element el = Get(elements, result.Roll);

            SelectInternal(result);

            this.ClickHighlight().nl();

            if (el != null)// && icon.Enter.isEntered(ref _inspectingRollResult).nl())
                el.Inspect(result.SubResultsList);
            
        }

        internal override void SelectInternal(RolledTable.Result result)
        {
            Element el = Get(elements, result.Roll);
            if (pegi.select(ref el, elements, stripSlashes: true) && el != null)
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }

            if (el != null)
                el.TrySelectInspect(result.SubResultsList);
        }

        private int _editedElement = -1;

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() || "{0} | {1}".F(index, name.Replace("Random", "")).ClickLabel(width: 120))
                edited = index;
        }

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

        public override void UpdatePrototypes()
        {
            base.UpdatePrototypes();

            foreach (var el in elements)
                el.UpdatePrototypes();
        }

        [Serializable]
        public class Element : RollTableElementWithSubTablesBase, IGotName
        {
            public string Name;

            public string NameForInspector { get => Name; set => Name = value; }

            public override string GetRolledElementName(List<RolledTable.Result> subTableResults)
            {
                var sub = base.GetRolledElementName(subTableResults);
                if (sub.IsNullOrEmpty() == false)
                    return "{0} {1}".F(Name, sub);

               return Name;
            }

            public override void Decode(string key, CfgData data)
            {
                switch (key) 
                {
                    case "Name":
                    case "name":
                        Name = data.ToString();
                        break;
                    case "Description": Description.Value = data.ToString(); break;
                    default:   base.Decode(key, data); break;
                }
            }

            public override void Inspect(List<RolledTable.Result> subTablesRolls) 
            {
                if (inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);

                base.Inspect();
            }

            public override void InspectInList(ref int edited, int ind)
            {
                base.InspectInList(ref edited, ind);

                pegi.edit(ref Name);
              
                if (icon.Enter.Click())
                    edited = ind;
            }

            public override void Inspect()
            {
                if (inspectedSubTable == -1)
                {
                    Description.Nested_Inspect(fromNewLine: false);
                    Description.Value.write();

                    pegi.nl();
                }
                base.Inspect();
            }
        }
    }

    [PEGI_Inspector_Override(typeof(RollTable))] internal class RollTableDrawer : PEGI_Inspector_Override { }
}