using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.NodeNotes
{

    public class LogicBranch<T> : IGotName , IPEGI, IAmConditional, ICanBeDefaultCfg, ISearchable  where T: class, ICfg, new() {

        public string name = "no name";

        public List<LogicBranch<T>> subBranches = new List<LogicBranch<T>>();

        public ConditionBranch conditions = new ConditionBranch();

        public List<T> elements = new List<T>();

        public virtual bool IsDefault => subBranches.Count ==0 && conditions.IsDefault && elements.Count == 0;

        public List<T> CollectAll(ref List<T> lst) {

            lst.AddRange(elements);

            foreach (var b in subBranches)
                b.CollectAll(ref lst);

            return lst;
        }

        public bool CheckConditions() => conditions.CheckConditions();

        #region Encode & Decode



        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("name", name)
            .Add("cond", conditions)
            .Add_IfNotEmpty("sub", subBranches)
            .Add_IfNotEmpty("el", elements)
            .Add_IfNotNegative("ie", _inspectedElement)
            .Add_IfNotNegative("is", _inspectedItems)
            .Add_IfNotNegative("br", _inspectedBranch);
        
        public virtual void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "name": name = data.ToString(); break;
                case "cond": conditions.DecodeFull(data); break;
                case "sub": data.ToList(out subBranches); break;
                case "el": data.ToList(out elements); break;
                case "ie": _inspectedElement = data.ToInt(); break;
                case "is": _inspectedItems = data.ToInt(); break;
                case "br": _inspectedBranch = data.ToInt(); break;
            }
        }
        #endregion

        #region Inspector

        public virtual string NameForElements => typeof(T).ToPegiStringType();

        public string NameForInspector
        {
            get { return name; }
            set { name = value; }
        }

        public void ResetInspector() {
            _inspectedElement = -1;
            _inspectedBranch = -1;
           //base.ResetInspector();
        }

        private int _inspectedElement = -1;
        private int _inspectedBranch = -1;
        private int _inspectedItems = -1;

        private readonly LoopLock searchLoopLock = new LoopLock();

        public bool IsContainsSearchWord(string searchString)
        {
            if (searchLoopLock.Unlocked)
                using(searchLoopLock.Lock()){

                    if (pegi.Try_SearchMatch_Obj(conditions, searchString))
                        return true;

                    foreach (var e in elements)
                        if (pegi.Try_SearchMatch_Obj(e, searchString))
                            return true;

                    foreach (var sb in subBranches)
                        if (pegi.Try_SearchMatch_Obj(sb, searchString))
                            return true;
                }

            return false;

        }


        private static LogicBranch<T> parent;

        public virtual void Inspect() {
   
            pegi.nl();

            if (parent != null || conditions.GetCount()>0)
                conditions.enter_Inspect_AsList(ref _inspectedItems, 1).nl();
            
            parent = this;

            NameForElements.enter_List(elements, ref _inspectedElement, ref _inspectedItems, 2).nl();

            (NameForInspector +"=>Branches").enter_List(subBranches, ref _inspectedBranch, ref _inspectedItems, 3).nl();

            parent = null;
        }
        
        #endregion

    }
}