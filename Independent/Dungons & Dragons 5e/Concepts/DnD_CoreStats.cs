using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;

namespace Dungeons_and_Dragons
{

    [Serializable] public class StatSet : SerializableDictionary_ForEnum<Stat, CoreStat> 
    {
        protected override void InspectElementInList(Stat key, int index)
        {

            var value = this.TryGet(key);

            string name = "{0} ({1})".F(key.GetShortName(), Creature.inspectedCreature.GetStatBonus(key).ToSignedNumber());

            if (value == null)
            {
                int current = Creature.inspectedCreature.GetDefaultValue(key);
                if ("{0} (Default)".F(name).editDelayed(120, ref current) && current > 0)
                    Creature.inspectedCreature.Stats[key] = new CoreStat(current);
            }
            else
            {
                if (icon.Clear.Click())
                    Remove(key);
                else
                    name.edit(labelWidth: 110, ref value.Value, valueWidth: 35);
            }

            if (icon.Dice.Click())
                pegi.GameView.ShowNotification(Creature.inspectedCreature.RollSavingThrow(key, influence: Influence.None).ToString());

        }
    }

    public enum Stat
    {
        Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma
    }

    public static class DnDStatExtensions 
    {
        public static string GetShortName(this Stat stat) 
        {
            return stat switch
            {
                Stat.Strength => "Str",
                Stat.Dexterity => "Dex",
                Stat.Constitution => "Con",
                Stat.Intelligence => "Int",
                Stat.Wisdom => "Wis",
                Stat.Charisma => "Cha",
                _ => "Err",
            };
        }
    }

    [Serializable]
    public class CoreStat 
    {
        public int Value;


        public CoreStat() { }

        public CoreStat(RollResult value) { Value = value.Value; }

        public CoreStat(int value) { Value = value; }
    }

}