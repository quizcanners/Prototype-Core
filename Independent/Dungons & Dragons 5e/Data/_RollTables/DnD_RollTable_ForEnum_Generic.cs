using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Text;
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

        private bool _inspectingRollResult;
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

            this.ClickHighlight();

            if (el != null && icon.Enter.isEntered(ref _inspectingRollResult).nl())
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


        private void InspectSelect(RolledTable.Result result)
        {
            Element el = this[result];
            if (pegi.select(ref el, elements, stripSlashes: true) && el != null)
            {
                result.Roll = GetTargetRoll(elements, el);
                el.Roll(result.SubResultsList);
            }
        }

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() || "{0} | {1}".F(index, nameof(T)).ClickLabel(width: 120))
                edited = index;

            InspectSelect(result);
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


        [Serializable]
        public class Element : RollTableElementBase, IPEGI, IGotReadOnlyName
        {
            public T Value;
            [SerializeField] List<RandomElementsRollTables> subTables = new List<RandomElementsRollTables>();
            
            public BigTextEditable Description;

            public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT: IComparable => result.TryGetConcept(out value, subTables);

            public string GetRolledElementName(List<RolledTable.Result> subTableResults)
            {
                StringBuilder sb = new StringBuilder(GetNameForInspector());

                for (int i = 0; i < subTables.Count; i++)
                {
                    var subRoll = subTableResults.TryGet(i);

                    if (subRoll != null && subRoll.IsRolled)
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
                foreach (var sub in subTables)
                {
                    var newRes = new RolledTable.Result();
                    results.Add(newRes);
                    sub.Roll(newRes);
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
                }
                else
                {
                    if (subTables.Count > 1 && icon.Exit.Click())
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
                pegi.editEnum(ref Value);
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

            public string GetNameForInspector() => pegi.GetNameForInspector(Value);

            private int _inspectedSubTable = -1;

            public void Inspect()
            {
                if (_inspectedSubTable == -1)
                    Description.Nested_Inspect(fromNewLine: false);


                "Sub Table".edit_List_UObj(subTables, ref _inspectedSubTable);
            }

        }
    }

  
}