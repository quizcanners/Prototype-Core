using System.Collections.Generic;
using System.Linq;
using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.NodeNotes
{

    // Trigger usage is only used for PEGI. Logic engine will not need this to process triggers

//#pragma warning disable IDE0019 // Use pattern matching

    /*
    public abstract class TriggerUsage : IGotReadOnlyName  {
        
        private static readonly List<string> Names = new List<string>();

        private static readonly List<TriggerUsage> Usages = new List<TriggerUsage>();

        public static TriggerUsage Get(int ind) => Usages[ind];
        
        #region Inspector
        public static bool SelectUsage(ref int ind) => pegi.select_Index(ref ind, Usages, 45);

        public virtual void Inspect(ConditionLogic c) { }

        public abstract bool Inspect(Result r);

        public virtual string GetResultName(Result r) => "???";

        public virtual string GetConditionValueName(ConditionLogic c) => "???";

        protected virtual bool Select(ref ResultType r, Dictionary<int, string> resultUsages) {
            var changed = pegi.ChangeTrackStart();
            var t = (int)r;

            if (!resultUsages.ContainsKey(t)) {
                if (icon.Warning.Click("Is " + r + ". Click to FIX "))
                    r = (ResultType)(resultUsages.First().Key);
            }
            else
            {
                if (pegi.select(ref t, resultUsages, 40)) 
                    r = (ResultType)t;
            }
            return changed;
        }

        protected virtual bool Select(ref ConditionType c, Dictionary<int, string> conditionUsages)
        {
            var changed = pegi.ChangeTrackStart();
            var t = (int)c;

            if (!conditionUsages.ContainsKey(t))
            {
                if (icon.Warning.Click("Is {0}. FIC".F(c)))
                    c = (ConditionType)(conditionUsages.First().Key);
 
            }
            else if (pegi.select(ref t, conditionUsages, 40))
                    c = (ConditionType)t;
            
            return changed;
        }
        
        public virtual bool Inspect(Trigger t) {
            var changed = pegi.ChangeTrackStart();
            var before = t.name;
            if (pegi.editDelayed(ref before, 150 - (HasMoreTriggerOptions ? 30 : 0))) {
                Trigger.searchField = before;
                t.name = before;
                pegi.UnFocus();
            }
            return changed;
        }
        
        #endregion

        public virtual bool HasMoreTriggerOptions => false;
        
        public virtual bool IsBoolean => false;

        public abstract string GetNameForInspector(); 

        public virtual bool UsingEnum() => false;
        
        public static readonly UsageBoolean Boolean = new UsageBoolean(0);
        public static readonly UsageNumber Number = new UsageNumber(1);
        public static readonly UsageStringEnum Enumeration = new UsageStringEnum(2);
       // public static readonly UsageGameTimeStamp Timestamp = new UsageGameTimeStamp(3);
     //   public static readonly UsageRealTimeStamp RealTime = new UsageRealTimeStamp(4);
        //  public static Usage_BoolTag boolTag = new Usage_BoolTag(5);
        //  public static Usage_IntTag intTag = new Usage_IntTag(6);
     //   public static Usage_Pointer pointer = new Usage_Pointer(7);

        public readonly int index;

        protected TriggerUsage(int ind) {
            index = ind;

            while(Names.Count <= ind) Names.Add("");
            while(Usages.Count <= ind) Usages.Add(null);

            Names[ind] = ind.ToString();
            Usages[ind]= this;
        }
    }

    public class UsageBoolean : TriggerUsage {


        #region Inspector
        
        public override string GetNameForInspector() => "YesNo";

        public override string GetConditionValueName(ConditionLogic c)
        {
            if (!c.IsBoolean)
                return base.GetConditionValueName(c);
            else
            {
                return ((ConditionLogicBool)c).compareValue.ToString();
            }
        }


        public override void Inspect(ConditionLogic c) {
            if (!c.IsBoolean) {
                icon.Warning.draw("Wrong Type: " + c.IsBoolean);
            }
            else
                pegi.toggleIcon(ref ((ConditionLogicBool)c).compareValue, "Condition Value");
        }

        public override string GetResultName(Result r)
        {
            if (!r.IsBoolean)
                return base.GetResultName(r);
            else
            {
                return r.updateValue == 0 ? "False" : "True";
            }
        }

        public override bool Inspect(Result r) {
            if (r.IsBoolean) return pegi.toggleIcon(ref r.updateValue);
            
            if (icon.Warning.Click("Wrong Type:" + r.type + ". Change To Bool"))
            {
                r.type = ResultType.SetBool;
                return true;
            }
            return false;

        }

        public override bool Inspect(Trigger t)
        {
            var vals = TriggerValues.Global;

            var changed = pegi.ChangeTrackStart();
            base.Inspect(t);
            vals.booleans.Toggle(t);
            return changed;
        }

        #endregion

        public override bool IsBoolean => true;

        public UsageBoolean(int index) : base(index) { }
    }

    public class UsageNumber : TriggerUsage {

        public override string GetNameForInspector()=> "Number";

        public static readonly Dictionary<int,string> ConditionUsages = new Dictionary<int, string> { 
            { ((int)ConditionType.Equals), "==" },
            { ((int)ConditionType.Above), ">" },
            { ((int)ConditionType.Below), "<" },
            { ((int)ConditionType.NotEquals), "!=" }
        };

        public static readonly Dictionary<int, string> ResultUsages = new Dictionary<int, string> {
            {(int)ResultType.Set, ResultType.Set.GetText()},
            {(int)ResultType.Add, ResultType.Add.GetText()},
            {(int)ResultType.Subtract, ResultType.Subtract.GetText()}
        };

        #region Inspector

        public override void Inspect(ConditionLogic c) {

            var num = c as ConditionLogicInt;

            if (num == null)
                icon.Warning.draw("Condition is not a number");
            else
            {
                Select(ref num.type, ConditionUsages);

                pegi.edit(ref num.compareValue, 40);
            }
        }

        public override string GetConditionValueName(ConditionLogic c)
        {
            if (c.IsBoolean)
                return base.GetConditionValueName(c);
            else
            {
                var num = c as ConditionLogicInt;
                return num.compareValue.ToString();
            }
        }

        public override string GetResultName(Result r)
        {
            if (r.IsBoolean)
                return base.GetResultName(r);
            else
            {
                return r.updateValue.ToString();
            }
        }

        public override bool Inspect(Result r) => 
            Select(ref r.type, ResultUsages) ||
            pegi.edit(ref r.updateValue, 40);
        
        public override bool Inspect(Trigger t) {
            var changed = pegi.ChangeTrackStart();
                
            base.Inspect(t);
            TriggerValues.Global.ints.Edit(t);

            return changed;
        }

        #endregion

        public UsageNumber(int index) : base(index) { }
    }

    public class UsageStringEnum : TriggerUsage
    {

        public override string GetNameForInspector()=> "Enums";

        #region Inspector

        public override void Inspect(ConditionLogic c) {


            var num = c as ConditionLogicInt;

            if (num != null)
            {
                Select(ref num.type, UsageNumber.ConditionUsages);

                pegi.select(ref num.compareValue, num.Trigger.enm);
            }
            else
                icon.Warning.draw("Incorrect type");
        }

        public override string GetConditionValueName(ConditionLogic c)
        {
            if (c.IsBoolean)
                return base.GetConditionValueName(c);
            var num = c as ConditionLogicInt;

            c.Trigger.enm.TryGetValue(num.compareValue, out string value);

            if (value.IsNullOrEmpty())
                value = num.compareValue.ToString();

            return value;
        }

        public override bool Inspect(Result r) {
            bool changed = false;

            changed |= Select(ref r.type, UsageNumber.ResultUsages);
            
            pegi.select(ref r.updateValue, r.Trigger.enm);
            return changed;
        }
        
        public override string GetResultName(Result r)
        {
            if (r.IsBoolean)
                return base.GetResultName(r);
          
            string value = null;

            switch (r.type)
            {
                case ResultType.Set: r.Trigger.enm.TryGetValue(r.updateValue, out value); break;
            }

            if (value.IsNullOrEmpty())
                value = r.updateValue.ToString();

            return value;
        }

        public override bool Inspect(Trigger t) {

            bool changed = pegi.ChangeTrackStart();
                
           base.Inspect(t);
            
           TriggerValues.Global.ints.Select(t).nl(); 

            if (Trigger.inspected != t) return changed;

            "__ Enums__".edit_Dictionary(t.enm);

            return changed;
        }

        #endregion

        public override bool HasMoreTriggerOptions => true;
        
        public UsageStringEnum(int index) : base(index) { }
    }
    */
    /*
    public class UsageGameTimeStamp : TriggerUsage {

        public override string GetNameForInspector()=> "Game Time";

        private static readonly Dictionary<int, string> ConditionUsages = new Dictionary<int, string> {
            { ((int)ConditionType.VirtualTimePassedAbove), "Game_Time passed > " },
            { ((int)ConditionType.VirtualTimePassedBelow), "Game_Time passed < " }
        };

        #region Inspector
     
        public override void Inspect(ConditionLogic c) {

            var num = c as ConditionLogicInt;

            if (num == null)
                icon.Warning.draw("Condition is not a number", 90); //.write();
            else
            {
                Select(ref num.type, ConditionUsages);

                pegi.edit(ref num.compareValue, 40);
            }
        }

        public override bool Inspect(Result r) {
            bool changed = false;
            
            changed |= Select(ref r.type , ResultUsages);

            if (r.type!= ResultType.SetTimeGame)
                changed |= pegi.edit(ref r.updateValue);

            return changed;
        }
       
        #endregion

        private static readonly Dictionary<int, string> ResultUsages = new Dictionary<int, string> {
            {(int)ResultType.SetTimeGame, ResultType.SetTimeGame.GetText()},
            {(int)ResultType.Add, ResultType.Add.GetText()},
            {(int)ResultType.Subtract, ResultType.Subtract.GetText()},
            {(int)ResultType.Set, ResultType.Set.GetText()}
        };

        public UsageGameTimeStamp(int index) : base(index) { }
    }

    public class UsageRealTimeStamp : TriggerUsage {

        public override string GetNameForInspector()=> "Real Time";

        private static readonly Dictionary<int, string> ConditionUsages = new Dictionary<int, string> {
            { ((int)ConditionType.RealTimePassedAbove), "Real_Time passed > " },
            { ((int)ConditionType.RealTimePassedBelow), "Real_Time passed < " }
        };

        #region Inspector

        public override void Inspect(ConditionLogic c) {

            var num = c as ConditionLogicInt;

            if (num == null)
                icon.Warning.draw("Condition is not a number", 90);
            else
            {
                Select(ref num.type, ConditionUsages);

                pegi.edit(ref num.compareValue, 40);
            }
        }

        public override bool Inspect(Result r) {
            var changed = pegi.ChangeTrackStart();

            Select(ref r.type, ResultUsages);

            if (r.type != ResultType.SetTimeReal)
                pegi.edit(ref r.updateValue);

            return changed;
        }

        #endregion

        private static readonly Dictionary<int, string> ResultUsages = new Dictionary<int, string> {
            {(int)ResultType.SetTimeReal, ResultType.SetTimeReal.GetText()},
            {(int)ResultType.Add, ResultType.Add.GetText()},
            {(int)ResultType.Subtract, ResultType.Subtract.GetText()}
        };

      

        public UsageRealTimeStamp(int index) : base(index) { }
    }
    */
    /*
    public class Usage_IntTag : TriggerUsage {

        public override string NameForDisplayPEGI()=> "TagGroup";

        #region Inspector
     
        public override bool Inspect(Trigger t) {
            var changed = base.Inspect(t);

            int value = Values.global.GetTagEnum(t);
            if (pegi.select(ref value, t.enm).nl()) 
                Values.global.SetTagEnum(t, value);
             
            if (Trigger.inspected != t) return changed;

           // "__ Tags __".nl();

            const string NoZerosForTrigs = "No04t";
            "Can't use 0 as tag index. ".writeOneTimeHint(NoZerosForTrigs);

            if ("___Tags___".edit_Dictionary(ref t.enm).changes(ref changed)) {
                string dummy;
                if (t.enm.TryGetValue(0, out dummy)) {
                    t.enm.Remove(0);
                    pegi.resetOneTimeHint(NoZerosForTrigs);
                }
            }

            return changed;
        }

        public override void Inspect(ConditionLogic c) {

            var num = c as ConditionLogicInt;

            if (num == null)
                icon.Warning.write("Condition is not a number", 60); 
            else {
                Select(ref num.type, Usage_Number.conditionUsages);
                pegi.select(ref num.compareValue, num.Trigger.enm);
            }
        }

        public override bool Inspect(Result r) {
            bool changed = false;

            changed |= Select(ref r.type, Usage_Number.resultUsages);
            
            pegi.select(ref r.updateValue, r.Trigger.enm);
            return changed;
        }
        #endregion

        public override bool HasMoreTriggerOptions => true;
        
        public Usage_IntTag(int index) : base(index) { }
    }

    public class Usage_BoolTag : TriggerUsage {

        public override string NameForDisplayPEGI()=> "Tag";

        #region Inspector
      
        public override void Inspect(ConditionLogic c) {

            var num = c as ConditionLogicBool;

            if (num == null)
                icon.Warning.write("Condition is not a bool",60);
            else
                pegi.toggleIcon(ref num.compareValue);
        }

        public override bool Inspect(Result r) => pegi.toggleIcon(ref r.updateValue);
        
        public override bool Inspect(Trigger t) {
            bool changed = base.Inspect(t);
            
                bool val = Values.global.GetTagBool(t);
                if (pegi.toggleIcon(ref val).changes(ref changed)) 
                    Values.global.SetTagBool(t, val);

            return changed;
        }
        #endregion
        
        public override bool IsBoolean => true;

        public Usage_BoolTag(int index) : base(index) { }
    }

    */


}