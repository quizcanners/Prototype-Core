using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public abstract class RollTableGeneric<T> : RandomElementsRollTables, IPEGI where T : RollTableElementBase, new()
    {
        [SerializeField] private QcGoogleSheetToCfg _sheetParcer;

        public List<Dice> _dicesToRoll;

        protected abstract List<T> List { get; set; }
        protected RollResult RollDices() => _dicesToRoll.Roll();
        protected T GetRandom() => Get(List, RollDices());
        protected RollResult GetTargetRoll(List<T> fromTable, T el)
        {
            var nextElementFirstIndex = _dicesToRoll.MinRoll();

            foreach (var item in fromTable)
            {
                if (item == el)
                    return nextElementFirstIndex;

                nextElementFirstIndex += item.Chances;
            }

            return new RollResult();
        }

        protected T Get(List<T> fromTable, RolledTable.Result result) => Get(fromTable, result.Roll);

        protected T Get(List<T> fromTable, RollResult rollResult)
        {
            var nextElementFirstIndex = _dicesToRoll.MinRoll();

            foreach (var item in fromTable)
            {
                if ((rollResult >= nextElementFirstIndex) && (rollResult < (nextElementFirstIndex + item.Chances)))
                    return item;

                nextElementFirstIndex += item.Chances;
            }

            return null;//fromTable.TryGet(0);
        }

        #region Inspector
        protected int _inspectedStuff = -1;
        protected abstract bool EditList();
        public virtual void Inspect()
        {
            this.ClickHighlight();

            pegi.nl();

            var rangeStart = _dicesToRoll.MinRoll();

            var lst = List;

            if (lst != null)
                for (int i = 0; i < lst.Count; i++)// var el in lst)
                {
                    var el = lst[i];

                    if (el != null)
                        el.SetRangeStart(ref rangeStart);
                }

            var endOfRange = rangeStart - 1;

            int groupIndex = -1;

            if ("Dices: {0}".F(_dicesToRoll.ToDescription(showPossibiliesNumber: true)).isEntered(ref _inspectedStuff, ++groupIndex).nl())
            {
                "Table: {0} - elements, {1} - total Value".F(lst.Count, rangeStart - _dicesToRoll.MinRoll()).writeHint();
                pegi.nl();

                if (_dicesToRoll == null)
                {
                    if ("Add Dice".Click())
                        _dicesToRoll = new List<Dice>() { Dice.D20 };
                }
                else
                {
                    _dicesToRoll.Inspect();
                }
            }

            if ("Table ({0} elements)".F(List.Count).isEntered(ref _inspectedStuff, ++groupIndex).nl())
            {
                EditList();

                if (endOfRange != _dicesToRoll.MaxRoll())
                    "{0}/{1}".F(endOfRange, _dicesToRoll.MaxRoll()).nl();
            }

            _sheetParcer.enter_Inspect_AsList(ref _inspectedStuff, ++groupIndex).nl_ifEntered();

            if (_inspectedStuff == groupIndex)  //"Download from Google Sheet".isEntered(ref _inspectedStuff, ++groupIndex).nl_ifEntered())
            {

                if (_sheetParcer.IsDownloading())
                    "Downloading...".nl();
                else if (_sheetParcer.IsDownloaded && "Update Table".Click())
                {
                    var slt = List;
                    _sheetParcer.ToListOverride(ref slt);
                }
            }

            if (_inspectedStuff == -1) 
            {
                if (_sheetParcer.IsDownloading())
                    icon.Wait.draw();
                else if ( _sheetParcer.NeedAttention().IsNullOrEmpty() && icon.Download.Click())
                    Service.Try<DnD_Service>(s => s.StartCoroutine(_sheetParcer.DownloadingCoro(
                        onFinished: () => 
                        {
                            var slt = List;
                            _sheetParcer.ToListOverride(ref slt);

                            var cols = _sheetParcer.columns;

                            var zeroCol = cols[0];

                            if (zeroCol.IsNullOrEmpty() == false && zeroCol.Contains("d"))
                            {
                                var diceValue = zeroCol.Substring(1);

                                if (int.TryParse(diceValue, out int dice))
                                {
                                    _dicesToRoll.Clear();
                                    _dicesToRoll.Add((Dice)dice);
                                }
                            }

                            
                            foreach (var col in cols)
                            {
                                if (col.Equals("Name") || col.Equals("name"))
                                    return;
                            }

                            Debug.LogError("Name column wasn't found in {0}. Replace {1} with Name".F(name, cols[cols.Count-1]));

                        } )));
            }



        }
        #endregion
    }

    [Serializable]
    public class RollTableElementBase : IPEGI_ListInspect, ICfgDecode
    {
        protected DnDPrototypesScriptableObject Data => Service.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);

        public int Chances = 1;
        [NonSerialized] private RollResult _rangeStart;

        private static char[] SPLITBY = new char[] { '-', '–' };

        public virtual void Decode(string key, CfgData data)
        {
            if (key == "Roll" || (key.Length>0 && key[0] == 'd'))
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

        [Serializable]
        public class BigTextEditable : IPEGI
        {
            public string Description;
            [SerializeField] private bool _editing;

            public void Inspect()
            {
                pegi.nl();
                if (_editing)
                {
                    pegi.editBig(ref Description);
                    if (icon.Done.Click())
                        _editing = false;
                }
                else
                {


                    if (Description.IsNullOrEmpty())
                    {
                        if ("Add Description".Click())
                            _editing = true;
                    }
                    else
                    {
                        Description.write(PEGI_Styles.OverflowText);

                        if (icon.Edit.Click().nl())
                            _editing = true;
                    }
                }
                pegi.nl();
            }
        }
    }

}
