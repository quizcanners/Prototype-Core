using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

    [CreateAssetMenu(fileName = FILE_NAME+ ".triggers", menuName = "Quiz Canners/Node Configs/" + FILE_NAME)]
    public class TriggerGroupsMeta : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Triggers Meta";

        [SerializeField] private DictionaryOfTriggerValues groups = new DictionaryOfTriggerValues();

        internal TriggerMeta this[IIntTriggerIndex index]
        {
            get => groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) ? vals[index] : null;
            set => groups.GetOrCreate(index.GetGroupId())[index] = value;
        }

        internal TriggerMeta this[IBoolTriggerIndex index]
        {
            get => groups.TryGetValue(index.GetGroupId(), out GroupOfTriggers vals) ? vals[index] : null;
            set => groups.GetOrCreate(index.GetGroupId())[index] = value;
        }

        public void Clear() => groups.Clear();
        

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

            [Serializable] private class DictionaryOfTriggerValues : SerializableDictionary<string, TriggerMeta> { }

            public TriggerMeta this[ITriggerIndex index]
            {
                get => _dictionary.TryGetValue(index.GetTriggerId(), out TriggerMeta val) ? val : default(TriggerMeta);
                set => _dictionary[index.GetTriggerId()] = value;
            }

            public void Clear() => _dictionary.Clear();

            public int GetCount() => _dictionary.Count;

            public void Inspect()
            {
                _dictionary.Nested_Inspect();
            }



        }


        [Serializable]
        public class TriggerMeta : IPEGI_ListInspect
        {

            public void InspectInList(ref int edited, int index)
            {

            }
        }

        [Serializable]
        private class GroupOfTriggers : IPEGI, IGotCount, IPEGI_ListInspect
        {
            [SerializeField] private GroupOfTriggerValues booleans = new GroupOfTriggerValues();
            [SerializeField] private GroupOfTriggerValues ints = new GroupOfTriggerValues();

            public TriggerMeta this[IIntTriggerIndex index]
            {
                get => ints[index];
                set => ints[index] = value;
            }

            public TriggerMeta this[IBoolTriggerIndex index]
            {
                get => booleans[index];
                set => booleans[index] = value;
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

    [PEGI_Inspector_Override(typeof(TriggerGroupsMeta))] internal class TriggerValuesKeyCollectionDrawer : PEGI_Inspector_Override { }
}