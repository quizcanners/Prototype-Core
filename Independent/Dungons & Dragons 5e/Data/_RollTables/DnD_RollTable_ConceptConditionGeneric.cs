using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public class DnD_RollTable_ConceptConditionGeneric<T> : RandomElementsRollTables, IPEGI where T: IComparable
    {
        [SerializeField] private List<Element> _elements = new List<Element>();

        private int _inspectedElement = -1;

        private Element this[RolledTable.Result value]
        {
            get
            {
                foreach (var el in _elements)
                    if (el.Condition == value.Roll.Value)
                        return el;

                return null;
            }
        }

        public override string GetRolledElementName(RolledTable.Result result)
        {
            if (!result.IsRolled) 
            {
                return "Not Rolled";
            }

            var el = this[result];
            if (el == null)
                return "El {0} not found".F((T)(object)result.Roll.Value);

            return el.GetRolledElementName(result.SubResult);
        }

        public void Inspect()
        {
            "Elements".edit_List(_elements, ref _inspectedElement);
        }

        protected override void InspectInternal(RolledTable.Result result)
        {
            var el = this[result];

            if ("Sub Table".select(90, ref el, _elements).nl())
            {
                result.Roll = RollResult.From(el.Condition);
                el.Roll(result.SubResult);
            }

            if (el!= null) 
                el.Inspect(result.SubResult);
            
        }

        protected override void RollInternal(RolledTable.Result result)
        {
            if (result.TryGetConceptFromUpperChain<T>(out var ccpt)) 
            {
                var ind = (int)(object)ccpt;

                foreach (var el in _elements) 
                {
                    if (el.Condition == ind) 
                    {
                        result.Roll = RollResult.From(el.Condition);
                        el.Table.Roll(result.SubResult);
                        return;
                    }
                }
            }

            var tbl = _elements.GetRandom();

            if (tbl != null)
            {
                result.Roll = RollResult.From(tbl.Condition);
                tbl.Table.Roll(result.SubResult);
            }
        }

        public override bool TryGetConcept<CT>(out CT value, RolledTable.Result result)
        {
            var el = this[result];

            if (el!= null) 
            {
                return el.TryGetConcept(out value, result.SubResult);
            } 

            return base.TryGetConcept(out value, result);
        }


        [Serializable]
        public class Element : IPEGI, IPEGI_ListInspect, IGotReadOnlyName
        {
            public int Condition;
            public RandomElementsRollTables Table;

            public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT : IComparable 
            {
                if (Table)
                    return Table.TryGetConcept(out value, result);

                value = default(CT);
                return false;
            }


            public string GetNameForInspector() => ((T)(object)Condition).ToString();

            public void Roll(RolledTable.Result result)
            {
                if (Table)
                    Table.Roll(result);
            }

            public string GetRolledElementName(RolledTable.Result result) 
            {
                if (!Table) 
                {
                    return "No Table";
                }

                return "{0} ({1})".F(Table.GetRolledElementName(result), GetNameForInspector());
            }

            public void Inspect(RolledTable.Result result)
            {
                if (!Table)
                    pegi.edit(ref Table).nl();
                else
                    Table.Inspect(result);
            }

            public void InspectInList(ref int edited, int ind)
            {
                pegi.editEnum<T>(ref Condition);
                pegi.edit(ref Table);
                if (icon.Enter.Click())
                    edited = ind;
            }

            public void Inspect()
            {
                "Condition".editEnum<T>(ref Condition).nl();
                pegi.edit(ref Table).nl();
                
            }
        }
    }
}