using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;


namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Monster : Creature
    {
        public ChallangeRating CR;

        public CreatureType Type;

        [SerializeField] private Size _size = Size.Medium;
        [SerializeField] protected CreatureSpeed Speed = new CreatureSpeed();

        public int HitDices = 1;
        
        public override Size Size => _size;

        public override int ArmorClass => CR.ArmorClass();

        public override int RollMaxHp() => Math.Max(1, (Size.GetHitDice().Roll(diceCount: HitDices) + GetStatBonus(Stat.Constitution) * HitDices).Value);

        protected override int ProficiencyBonus => CR.ProficiencyBonus();

        public override CellDistance this[SpeedType type] => Speed.TryGetValue(type, out CellDistance dist) ? dist : CellDistance.FromCells(0);

        protected override Proficiency GetProficiency(Skill stat) => base.GetProficiency(stat);

        protected override Proficiency SavingThrowProficiency(Stat stat) => base.SavingThrowProficiency(stat);

        protected override int GetStatValueBonuses(Stat stat) => 0;

        public override void Inspect()
        {
            if (inspectedStuff == -1 && Stats.IsNullOrEmpty() == false)
            {
                "Size".editEnum(ref _size).nl();
                pegi.editEnum(ref Type);
                pegi.nl();

                pegi.editEnum(ref CR).nl();


                "Hit Dice".edit(70, ref HitDices);
                int avgRoll = _size.GetHitDice().AvargeRoll(HitDices);
                int bonus = GetStatBonus(Stat.Constitution) * HitDices;

                "Avarage Hp: {0}{1} = {2}   RND:[{3}]".F(avgRoll, bonus.ToSignedNumber(), avgRoll + bonus, RollMaxHp()).nl();
            }
            
            base.Inspect();

            "Speed".enter_Inspect(Speed, ref inspectedStuff, 10).nl();
        }

        internal override int GetDefaultValue(Stat stat) => 10;
    }


    [Serializable]
    public class MonsterSmartId : DnD_SmartId<Monster>
    {
        protected override SerializableDictionary<string, Monster> GetEnities() => Data.Monsters;
    }

    [Serializable]
    public class MonstersDictionary : SerializableDictionary<string, Monster> { }


    [Serializable]
    public class CreatureSpeed: SerializableDictionary_ForEnum<SpeedType, CellDistance> 
    {
        public override void Create(SpeedType key)
        {
            this[key] = CellDistance.FromCells(6);
        }

        public CreatureSpeed() 
        {
            Add(SpeedType.Walking,  CellDistance.FromCells(6));
        }
    }


 
}
