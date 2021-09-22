using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class TableRollResult : IPEGI, IGotReadOnlyName, IGotName, IPEGI_ListInspect, ISerializationCallbackReceiver, ICfg
    {
        [SerializeField] private string _name;
        [SerializeField] private string tableKey;

        public RolledTable.Result result = new RolledTable.Result();

        private RandomElementsRollTablesDictionary Tables => Service.TryGetValue<DnD_Service, RandomElementsRollTablesDictionary>(s => s.DnDPrototypes.RollTables);

        #region Encode & Decode

        public CfgEncoder Encode() => new CfgEncoder()
            .Add_String("n", _name)
            .Add_String("t", tableKey)
            .Add("r", result);

        public void Decode(string key, CfgData data)
        {
            switch (key) 
            {
                case "n": _name = data.ToString(); break;
                case "t": tableKey = data.ToString(); break;
                case "r": result.DecodeFull(data); break;
            }
        }

        [SerializeField] private CfgData _data;
        public void OnBeforeSerialize() =>  _data = result.Encode().CfgData;
        public void OnAfterDeserialize()
        {
            result = new RolledTable.Result();
            _data.DecodeOverride(ref result);
            
        }

        #endregion

        #region Inspector
        public string NameForInspector { get => _name; set => _name = value; }
        public string GetReadOnlyName() => tableKey;

        public void Inspect()
        {
            if (Tables == null) 
            {
                "No Tables found".writeWarning();
                return;
            }

            if (Tables.TryGetValue(tableKey, out RandomElementsRollTables table) && icon.Dice.Click())
                    table.Roll(result);
            
            "Table".select(70, ref tableKey, Tables);

            if (icon.Save.Click()) OnBeforeSerialize();
            if (icon.Load.Click()) OnAfterDeserialize();

            if (table!= null)
            {
                table.ClickHighlight().nl();
                table.Inspect(result);
            }
        }
        public void InspectInList(ref int edited, int index)
        {
            pegi.edit(ref _name);

            pegi.select(ref tableKey, Tables);

            if (icon.Enter.Click())
                edited = index;

            Tables.TryGet(tableKey).ClickHighlight();

            if (tableKey.IsNullOrEmpty() == false && Tables.TryGetValue(tableKey, out RandomElementsRollTables table))
            {
                if (icon.Dice.Click())
                    table.Roll(result);

                using (pegi.Indent())
                {
                    pegi.nl();
                    table.GetRolledElementName(result).write(PEGI_Styles.HintText);
                }
            }
        }

        #endregion

    }
}