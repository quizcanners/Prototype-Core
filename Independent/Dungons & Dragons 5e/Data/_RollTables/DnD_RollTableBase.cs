using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    internal interface IRollResult 
    {
        void Roll(RolledTable.Result result);
    }

  

    public abstract class RandomElementsRollTables : ScriptableObject, IGotName, IRollResult, IPEGI_ListInspect
    {
        protected const string TABLE_CREATE_NEW_PATH = "Quiz Canners/Dungeons & Dragons/Roll Table/";

        public virtual bool TryGetConcept<CT>(out CT value, RolledTable.Result result) where CT : IComparable
        {
            value = default(CT);
            return false;
        }

        public void Roll(RolledTable.Result result) 
        {
            using (result.AddAndUse(this)) 
            {
                RollInternal(result);
            }
        }

        protected abstract void RollInternal(RolledTable.Result result);

        #region Inspector

        public string NameForInspector 
        { 
            get => name;
            set => QcUnity.RenameAsset(this, value); 
        }

        protected abstract void InspectInternal(RolledTable.Result result);

        public void Inspect(RolledTable.Result result) 
        {
            using (result.AddAndUse(this))
            {
                InspectInternal(result);
            };
        }


        public abstract string GetRolledElementName(RolledTable.Result result);

        public void InspectInList(ref int edited, int index, RolledTable.Result result) 
        {
            using (result.AddAndUse(this))
            {
                InspectInList_Internal(ref edited, index, result);
            };
        }

        protected virtual void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click() || "{0} | {1} : {2}".F(index, this.GetNameForInspector().Replace("Random ", ""), GetRolledElementName(result)).ClickLabel())
                edited = index;

            if (icon.Dice.Click())
                Roll(result);

            this.ClickHighlight();
        }

        public void InspectInList(ref int edited, int index)
        {
            if (icon.Enter.Click() || "{0} | {1}".F(index, this.GetNameForInspector().Replace("Random ", "")).ClickLabel())
                edited = index;

            this.ClickHighlight();
        }

        #endregion
    }

    [Serializable]
    public class RandomElementsRollTablesDictionary : SerializableDictionary<string, RandomElementsRollTables> { }
}
