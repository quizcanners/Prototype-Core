using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    [ExecuteAlways]
    public class TriggerValuesService : IsItGameServiceBase, IPEGI
    {
        [SerializeField] private List<TriggerGroupsMeta> _triggerGroupsMeta = new List<TriggerGroupsMeta>();
        public TriggerValues Values = new TriggerValues();

        internal TriggerGroupsMeta.TriggerMeta this[IIntTriggerIndex index]
        {
            get
            {
                if (index.IsValid() == false)
                    return null;

                foreach ( var t in _triggerGroupsMeta) 
                {
                    var trM = t[index];

                    if (trM != null)
                        return trM;
                }
                return null;
            }
        }

        internal TriggerGroupsMeta.TriggerMeta this[IBoolTriggerIndex index]
        {
            get
            {
                if (index.IsValid() == false)
                    return null;

                foreach (var t in _triggerGroupsMeta)
                {
                    var trM = t[index];

                    if (trM != null)
                        return trM;
                }
                return null;
            }
        }


        #region Inspector

        private int _inspectedStuff = -1;
        private int _inspectedGroup = -1;
        public override void Inspect()
        {
            pegi.nl();
            "Values".enter_Inspect(Values, ref _inspectedStuff, 0).nl();
            "Triggers".enter_List_UObj(_triggerGroupsMeta,  ref _inspectedGroup,  ref _inspectedStuff, 1).nl();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(TriggerValuesService))] internal class TriggerValuesServiceDrawer : PEGI_Inspector_Override { }
}
