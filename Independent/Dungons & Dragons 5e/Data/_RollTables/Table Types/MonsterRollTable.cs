using System;
using System.Collections;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;


namespace Dungeons_and_Dragons
{
    [CreateAssetMenu(fileName = FILE_NAME,  menuName = TABLE_CREATE_NEW_PATH + FILE_NAME)]
    public class MonsterRollTable : RollTableGeneric<MonsterRollTable.Element> {

        public const string FILE_NAME = "Random Monster";
        
        public List<Element> elements;

        public Element this[RolledTable.Result roll] => Get(elements, roll.Roll);

        protected override List<Element> List { get => elements; set => elements = value; }

        protected override bool EditList() => "{0} {1}".F(_dicesToRoll.ToDescription(), name).edit_List(elements).nl();

        public override void Inspect()
        {
            base.Inspect();

            if ("Get Random".Click())
                pegi.GameView.ShowNotification(GetRandom().Name);
        }

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (icon.Enter.Click())
                edited = index;
        }

        internal override void SelectInternal(RolledTable.Result result)
        {
            Element el = this[result];
            if ("Enemy".select(70, ref el, elements) && el != null)
                result.Roll = GetTargetRoll(elements, el);

            this.ClickHighlight();
        }

        protected override void InspectInternal(RolledTable.Result result)
        {
            if (icon.Dice.Click())
                Roll(result);

            SelectInternal(result);

            pegi.nl();
        }

        protected override void RollInternal(RolledTable.Result result)
        {
            result.Roll = RollDices();
        }

        public override string GetRolledElementName(RolledTable.Result result)
        {
            if (!result.IsRolled)
                return "Not Rolled";

            var el = Get(elements, result.Roll);
            if (el != null)
                return el.Name;

            return "No Element for {0}".F(result.Roll);
        }

        [Serializable]
        public class Element : RollTableElementBase, IGotName
        {
            public string Name;

            public string NameForInspector { get => Name; set => Name = value; }

            public override void InspectInList(ref int edited, int ind)
            {
                base.InspectInList(ref edited, ind);
                
                if (!Data)
                    "No Manager".writeWarning();
                else if (!Data)
                    "No Prototypes in Manager".writeWarning();
                else
                    pegi.select(ref Name, Data.Monsters);
            }
        }
    }
    
    [PEGI_Inspector_Override(typeof(MonsterRollTable))] internal class MonsterRollTableDrawer : PEGI_Inspector_Override { }
}