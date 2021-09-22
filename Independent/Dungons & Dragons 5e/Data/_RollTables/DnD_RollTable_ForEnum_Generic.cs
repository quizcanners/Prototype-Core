using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public abstract class RollTable_ForEnum_Generic<T> : RollTableGeneric<RollTable_ForEnum_Generic<T>.Element>
    {

        [SerializeField] protected List<Element> elements = new List<Element>();

        public Element this[RolledTable.Result roll] => Get(elements, roll.Roll);

        protected override List<Element> List { get => elements; set => elements = value; }


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
            if (pegi.select(ref el, elements, stripSlashes: true) && el != null)
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }

            this.ClickHighlight().nl();

            if (el != null)
                el.Inspect(result.SubResultsList);
        }

        public override void Inspect()
        {
            if (elements.Count == 0 && "Auto-Fill".Click().nl())
            {
                var enumValues = Enum.GetValues(typeof(T));

                foreach (var eVal in enumValues)
                    elements.Add(new Element() { Value = (T)eVal });
            }

            base.Inspect();
        }

        private int _editedElement = -1;
        protected override bool EditList()
        {
            return "{0} {1}".F(_dicesToRoll.ToDescription(), QcSharp.KeyToReadablaString(name.SimplifyTypeName())).edit_List(elements, ref _editedElement).nl();
        }

        internal override void SelectInternal(RolledTable.Result result)
        {
            Element el = this[result];
            if (pegi.select(ref el, elements, stripSlashes: true) && el != null)
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }

            if (el != null)
                el.TrySelectInspect(result.SubResultsList);
        }

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() || "{0} | {1}".F(index, typeof(T).ToPegiStringType()).ClickLabel(width: 120))
                edited = index;
        }

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
                if (typeof(T).Equals(typeof(CT)))
                {
                    value = (CT)(object)el.Value;
                    return true;
                }

                if (el.TryGetConcept(out value, result))
                    return true;
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
        public class Element : RollTableElementWithSubTablesBase, IGotReadOnlyName
        {
            public T Value;
    
            public override string GetRolledElementName(List<RolledTable.Result> subTableResults) => "{0} {1}".F(pegi.GetNameForInspector(Value), base.GetRolledElementName(subTableResults)); 
            
            public override void Inspect(List<RolledTable.Result> subTablesRolls)
            {
                if (inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);

                base.Inspect();
            }

            public override void InspectInList(ref int edited, int ind)
            {
                pegi.editEnum(ref Value);

                base.InspectInList(ref edited, ind);

                if (icon.Enter.Click())
                    edited = ind;
            }

            public string GetReadOnlyName() => pegi.GetNameForInspector(Value);


            public override void Inspect()
            {
                if (inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);

                base.Inspect();

            }
        }
    }
}