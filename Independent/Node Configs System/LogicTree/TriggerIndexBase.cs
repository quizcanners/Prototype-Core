using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

    internal interface ITriggerIndex 
    {
        public string GetGroupId(); // { get; }
        public string GetTriggerId(); // { get; }
        public bool IsValid();
    }

    internal interface IIntTriggerIndex : ITriggerIndex
    {
        public int GetValue();
    }

    internal interface IBoolTriggerIndex : ITriggerIndex
    {
        public bool GetValue();
    }



    public abstract class TriggerIndexBase : ITriggerIndex, ICfg, IGotReadOnlyName {

        public string groupId;
        public string triggerId;
        public string GetGroupId() => groupId;
        public string GetTriggerId() => triggerId;
        public bool IsValid() => groupId.IsNullOrEmpty() == false && triggerId.IsNullOrEmpty() == false;

        public TriggerIndexBase TriggerIndexes 
        { 
            set 
            { 
                if (value != null) 
                {
                    groupId = value.groupId;
                    triggerId = value.triggerId;
                }
            }
        }
        
        #region Encode & Decode
        public abstract CfgEncoder Encode();
        public abstract void Decode(string tg, CfgData data);

        protected CfgEncoder EncodeIndex() => new CfgEncoder()
            .Add_String("gi", groupId)
            .Add_String("ti", triggerId);

        protected void DecodeIndex(string tag, CfgData data)
        {
            switch (tag)
            {
                case "gi": groupId = data.ToString(); break;
                case "ti": triggerId = data.ToString(); break;
            }
        }
        #endregion

        internal TriggerValues Values => Service.Get<TriggerValuesService>().Values;


        #region Inspector

        public static Trigger selectedTrig;
        public static TriggerIndexBase selected;

        public virtual void Inspect()
        {
            
        }

        public static string focusName;

        public static TriggerIndexBase EditedValueIndex;

        // public virtual bool PEGI_inList_Sub(int ind, ref int inspecte) => false;

        /*
        public virtual void InspectInList(ref int inspected, int ind)
        {
            if (this != EditedValueIndex) {
                PEGI_inList_Sub(ind, ref inspected);

                if (icon.Edit.ClickUnFocus())
                    EditedValueIndex = this;

                SearchAndAdd_Triggers_PEGI(ind);
            }
            else
            {
                if (icon.FoldedOut.Click())
                    EditedValueIndex = null;

                Trigger.inspected = Trigger;

                Trigger.Inspect_AsInList();

                if (Trigger.inspected != Trigger)
                    EditedValueIndex = null;
            }
        }*/

        /*
        public bool FocusedField_PEGI(int index, string prefix) {

            bool changed = false;

            focusName = "{0}{1}_{2}".F(prefix,index,groupId);

            pegi.NameNextForFocus(focusName);

            string tmpname = Trigger.name;

            if (Trigger.focusIndex == index)
                changed |= pegi.edit(ref Trigger.searchField);
            else
                changed |= pegi.edit(ref tmpname);

            return changed;
        }*/

        /*
        public bool SearchAndAdd_Triggers_PEGI(int index)
        {
            bool changed = pegi.ChangeTrackStart();

            Trigger t = Trigger;

            if (this == EditedValueIndex) 
            {
                t.Inspect();
            }

            if (pegi.FocusedName.Equals(focusName) && (this != EditedValueIndex))
            {
                selected = this;

                if (Trigger.focusIndex != index)
                {
                    Trigger.focusIndex = index;
                    Trigger.searchField = Trigger.name;
                }

             //   if (Search_Triggers_PEGI())
                   // Trigger.searchField = Trigger.name;

              //  selectedTrig = Trigger;

            }
            else
            {
                if (index == Trigger.focusIndex) Trigger.focusIndex = -2;
            }
        
       // if (this == selected)
               // TriggerGroup.AddTrigger_PEGI(this);

            return changed;
        }
        */
        /*
        public bool Search_Triggers_PEGI() {

            bool changed = pegi.ChangeTrackStart();

            Trigger current = Trigger;

            Trigger.searchMatchesFound = 0;

            if (KeyCode.Return.IsDown().nl())
                pegi.UnFocus();

            int searchMax = 20;

            pegi.GetNameForInspector(current).write();

            if (icon.Done.Click().nl())
                pegi.UnFocus();
            else foreach (var gb in TriggerGroup.all) {
                    var lst = gb.GetFilteredList(ref searchMax,
                        !SearchTriggerSameType || IsBoolean ,
                        !SearchTriggerSameType || !IsBoolean );
                    foreach (var t in lst)
                        if (t != current) {
                            Trigger.searchMatchesFound++;

                            if (icon.Done.ClickUnFocus(20)) 
                                Trigger = t;

                            pegi.GetNameForInspector(t).nl();
                        }

            }
            return changed;
        }
        */
        public virtual string GetReadOnlyName() => GetType().ToPegiStringType();



        #endregion
    }

    public static class ValueSettersExtensions
    {
        /*
        public static ValueIndex SetLastUsedTrigger(this ValueIndex index) {
            if (index != null)
                index.TriggerIndexes = TriggerGroup.TryGetLastUsedTrigger();
            return index;
        }

        public static bool Get(this UnNullableCfg<CountlessBool> uc, ValueIndex ind) => uc[ind.groupId][ind.triggerId];
        public static void Set(this UnNullableCfg<CountlessBool> uc, ValueIndex ind, bool value) => uc[ind.groupId][ind.triggerId] = value;

        public static int Get(this UnNullableCfg<CountlessInt> uc, ValueIndex ind) => uc[ind.groupId][ind.triggerId];
        public static void Set(this UnNullableCfg<CountlessInt> uc, ValueIndex ind, int value) => uc[ind.groupId][ind.triggerId] = value;

        
        public static bool Toggle(this UnNullableCfg<CountlessBool> uc, ValueIndex ind)
        {
            var tmp = uc.Get(ind);//[ind.groupIndex][ind.triggerIndex];
            if (pegi.toggleIcon(ref tmp))
            {
                uc.Set(ind, tmp);
                return true;
            }
            return false;
        }

        public static bool Edit(this UnNullableCfg<CountlessInt> uc, ValueIndex ind)
        {
            var tmp = uc.Get(ind);//[ind.groupIndex][ind.triggerIndex];
            if (pegi.edit(ref tmp))
            {
                uc.Set(ind, tmp);
                return true;
            }
            return false;
        }

        public static bool Select(this UnNullableCfg<CountlessInt> uc, Trigger t)
        {
            var tmp = uc.Get(t);
            if (pegi.select(ref tmp, t.enm))
            {
                uc.Set(t, tmp);
                return true;
            }
            return false;
        }
        */

    }

}