using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public abstract class RollTableElementWithSubTablesBase : RollTableElementBase, IPEGI
    {
        [SerializeField] protected List<RandomElementsRollTables> subTables = new List<RandomElementsRollTables>();

        public BigTextEditable Description;

        public bool TryGetConcept<CT>(out CT value, RolledTable.Result result) 
            where CT : IComparable => result.TryGetConcept(out value, subTables);

        internal void TrySelectInspect(List<RolledTable.Result> results)
        {
            if (subTables.Count < 3)
            {
                for (int i = 0; i < subTables.Count; i++)
                {
                    var t = subTables[i];
                    if (t)
                        t.SelectInternal(results.GetOrCreate(i));
                }
            }
        }

        public virtual string GetRolledElementName(List<RolledTable.Result> subTableResults) => Description.GetRolledElementName(subTableResults, subTables);

        public void Roll(List<RolledTable.Result> results)
        {
            results.Clear();
            foreach (var sub in subTables)
            {
                var newRes = new RolledTable.Result();
                results.Add(newRes);

                if (sub)
                {
                    sub.Roll(newRes);
                }
            }
        }

        public override void Inspect(List<RolledTable.Result> subTablesRolls)
        {
            if (inspectedSubTable >= subTables.Count)
                inspectedSubTable = -1;

            if (subTables.Count == 1)
                inspectedSubTable = 0;

            if (inspectedSubTable == -1)
            {
                for (int i = 0; i < subTables.Count; i++)
                {
                    var t = subTables[i];
                    var r = subTablesRolls.GetOrCreate(i);
                    if (icon.Enter.Click() || t.GetRolledElementName(r).ClickLabel().nl())
                        inspectedSubTable = i;
                }
            }
            else
            {
                if (subTables.Count > 1 && icon.Exit.Click())
                    inspectedSubTable = -1;
                else
                {
                    subTables[inspectedSubTable].Inspect(subTablesRolls.GetOrCreate(inspectedSubTable));
                }
            }
        }

        public override void InspectInList(ref int edited, int ind)
        {
            base.InspectInList(ref edited, ind);

            if (subTables.Count < 2)
            {
                RandomElementsRollTables tmp = subTables.TryGet(0);
                if (pegi.edit(ref tmp, 90))
                    subTables.ForceSet(0, tmp);
            }
            else
                "X {0}".F(subTables.Count).write(40);
        }


        protected int inspectedSubTable = -1;

        public virtual void Inspect()
        {
            "Sub Table".edit_List_UObj(subTables, ref inspectedSubTable);
        }
    }


    [Serializable]
    public abstract class RollTableElementBase : IPEGI_ListInspect, ICfgDecode
    {
        protected DnDPrototypesScriptableObject Data => Service.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);

        public int Chances = 1;
        [NonSerialized] private RollResult _rangeStart;

        private static char[] SPLITBY = new char[] { '-', '–' };

        public virtual void Decode(string key, CfgData data)
        {
            if (key == "Roll" || (key.Length > 0 && key[0] == 'd'))
            {
                var val = data.ToString();

                bool isRange = false;
                foreach (var x in SPLITBY)
                {
                    if (val.Contains(x.ToString()))
                    {
                        isRange = true;
                        break;
                    }
                }

                if (isRange)
                {
                    var parts = val.Split(SPLITBY, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        var from = new CfgData(parts[0]).ToInt();
                        var to = new CfgData(parts[1]).ToInt();

                        if (to == 0)
                            to = 100;


                        Chances = Math.Max(1, to - from + 1);
                    }
                }
                else
                    Chances = 1;

                return;
            }

            switch (key)
            {
                case "Chances": Chances = data.ToInt(); break;
            }
        }

        #region Inspector

        public virtual void Inspect(List<RolledTable.Result> results) { }

        public void SetRangeStart(ref RollResult rangeStart)
        {
            _rangeStart = rangeStart;
            rangeStart += Chances;
        }

        public virtual void InspectInList(ref int edited, int ind)
        {
            string rangeString;

            if (Chances > 1)
                rangeString = "{0}-{1}".F(_rangeStart, _rangeStart + Chances - 1);
            else
                rangeString = (_rangeStart).ToString();

            if (rangeString.edit(45, ref Chances, valueWidth: 35))
                Chances = Mathf.Max(1, Chances);
        }

        #endregion

      /*  [Serializable]
        public class BigTextEditable : IPEGI
        {
            public string Text;
            [SerializeField] private bool _editing;

            public void Inspect()
            {
                pegi.nl();
                if (_editing)
                {
                    pegi.editBig(ref Text);
                    if (icon.Done.Click())
                        _editing = false;
                }
                else
                {
                    if (Text.IsNullOrEmpty())
                    {
                        if ("Add Description".Click())
                            _editing = true;
                    }
                    else
                    {
                        Text.write(PEGI_Styles.OverflowText);

                        if (icon.Edit.Click().nl())
                            _editing = true;
                    }
                }
                pegi.nl();
            }
        }*/
    }
}