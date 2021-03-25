using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    [Serializable]
    public class TriggerValues : IPEGI, IGotCount
    {
        public int Version = -1;

        [SerializeField] private DictionaryOfTriggerValues groups = new DictionaryOfTriggerValues();
       
        internal int this[IIntTriggerIndex index] 
        {
            get => index.IsValid() ? (groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) ? vals[index] : 0) : 0;
            set 
            {
                var dic = groups.GetOrCreate(index.GetGroupId());
                if (dic[index] != value)
                {
                    dic[index] = value;
                    Version++;
                }
            }
        }

        internal bool this[IBoolTriggerIndex index]
        {
            get => index.IsValid() ? (groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) && vals[index]) : false;
            
            set
            {
                var dic = groups.GetOrCreate(index.GetGroupId());
                if ((dic[index]) != value)
                {
                    dic[index] = value;
                    Version++;
                }
            }
        }

        public void Clear()
        {
            groups.Clear();
        }

        #region Inspector

        public int GetCount() => groups.Count;// + enumTags.CountForInspector + boolTags.CountForInspector; 

        public virtual void Inspect() 
        {
            pegi.nl();
            groups.Nested_Inspect();
        }

        #endregion


        [Serializable]
        private class GroupOfTriggerValues : IPEGI, IGotCount
        {
            [SerializeField] private DictionaryOfTriggerValues _dictionary = new DictionaryOfTriggerValues();

            [Serializable] private class DictionaryOfTriggerValues : SerializableDictionary<string, Trigger> { }

            public int this[ITriggerIndex index]
            {
                get => _dictionary.TryGetValue(index.GetTriggerId(), out Trigger val) ? val.value : 0;
                set
                {
                    if (value == 0)
                        _dictionary.Remove(index.GetTriggerId());
                    else
                        _dictionary[index.GetTriggerId()] = new Trigger { value = value };
                }
            }

            public void Clear() => _dictionary.Clear();

            public int GetCount() => _dictionary.Count;

            public void Inspect()
            {
                _dictionary.Nested_Inspect();
            }


            [Serializable]
            private struct Trigger : IPEGI_ListInspect
            {
                [SerializeField] public int value;

                public void InspectInList(ref int edited, int index)
                {
                    pegi.edit(ref value);
                }
            }

        }

        [Serializable]
        private class GroupOfTriggers : IPEGI, IGotCount, IPEGI_ListInspect
        {
            [SerializeField] private GroupOfTriggerValues booleans = new GroupOfTriggerValues();
            [SerializeField] private GroupOfTriggerValues ints = new GroupOfTriggerValues();

            public int this[IIntTriggerIndex index]
            {
                get => ints[index];
                set => ints[index] = value;
            }

            public bool this[IBoolTriggerIndex index]
            {
                get => booleans[index] > 0;
                set => booleans[index] = value ? 1 : 0;
            }

            public void Clear()
            {
                booleans.Clear();
                ints.Clear();

            }

            #region Inspector
            public int GetCount() => booleans.GetCount() + ints.GetCount();

            private int _inspectedStuff = -1;
            public void Inspect()
            {
                "Booleans".enter_Inspect(booleans, ref _inspectedStuff, 0).nl();
                "Integers".enter_Inspect(ints, ref _inspectedStuff, 1).nl();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (icon.Clear.ClickConfirm(confirmationTag: "Erase " + index, "Erase all valus from the group?"))
                    Clear();

                if (icon.Enter.Click())
                    edited = index;
            }
            #endregion
        }


        [Serializable] 
        private class DictionaryOfTriggerValues : SerializableDictionary<string, GroupOfTriggers>
        {  
        }
    }
}