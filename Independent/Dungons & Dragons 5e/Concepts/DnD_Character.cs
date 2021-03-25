using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Character : Creature
    {
        [SerializeField] public CharacterLevel Level = new CharacterLevel();
        [SerializeField] internal protected Class characterClass;
        [SerializeField] internal protected Race characterRace;
        [SerializeField] internal protected int Experience;
        [SerializeField] internal ArmorProficiencies ArmorProficiencies = new ArmorProficiencies();
        [SerializeField] internal WeaponProficiencies WeaponProficiencies = new WeaponProficiencies();
        [SerializeField] internal ToolProficiencies ToolProficiencies = new ToolProficiencies();
        [SerializeField] private ClassFeatureStates _featureStates = new ClassFeatureStates();
        [SerializeField] public Wallet Wallet = new Wallet();

        public bool TryGet<T>(out T value) where T: ClassFeature.Base, new()
        {
            value = null;

            if (!Class.GotFeature<T>())
                return false;

            value = _featureStates.GetOrCreate<T>();
            return true;
        }

        protected override int ProficiencyBonus => Level.ProficiencyBonus;

        public CharacterClass Class => characterClass.Get();

        public CharacterRace Race => characterRace.Get();

        public override CellDistance this[SpeedType type]=>
            type switch
            {
                SpeedType.Walking => Race.WalkingSpeed,
                SpeedType.Swimming => Race.WalkingSpeed.Half(),
                SpeedType.Climbing => this[SpeedType.Walking].Half(),
                _ => CellDistance.FromCells(0),
            };
        
        public override Size Size => Size.Medium;

        public override int ArmorClass => 10 + GetStatBonus(Stat.Dexterity);
        
        public override int RollMaxHp()
        {
            var hitDice = characterClass.GetHitDie();
            
            var mod = GetStatBonus(Stat.Constitution);
            
            int hp = (int)hitDice + mod;

            if (Level > 1)
            {
                int diff = Level - 1;
                hp += (hitDice.Roll(diceCount: diff) + mod * diff).Value;
            }
            
            return hp;
        }

        protected override Proficiency SavingThrowProficiency(Stat stat) => Race[stat].And(Class[stat]).And(base.SavingThrowProficiency(stat));

        protected override Proficiency GetProficiency(Skill skill)
        {
            Proficiency prof = Race[skill].And(base.GetProficiency(skill));

            if (TryGet<ClassFeature.Proficiencies>(out var feature)) 
                prof = prof.And(feature.Skills[skill]);
            
            return prof;
        }

        protected override int GetStatValueBonuses(Stat stat) => Race.StatIncrease(stat);

        public bool CanLevelUp() => Level<20 && (Experience >= Level.ExperienceForNextLevel);

        #region Inspector

        public static Character inspected;

        public override void Inspect()
        {
            base.Inspect();

            inspected = this;

            if (inspectedStuff == -1)
            {
                "Character Race".editEnum(ref characterRace).nl();

                "Character Class".editEnum(ref characterClass).nl();

                if ("Level".edit(40, ref Level.Value, minInclusiven: 1, maxInclusive: 20)) 
                    Experience = Mathf.Max(Experience, Level.GetExperienceForLevel());

                if (CanLevelUp() && icon.Up.Click())
                    Level.Up();

                pegi.nl();

                "Exp".edit(ref Experience).nl();
            }

            int sectionIndex = 10;

            "Features".enter_Inspect(_featureStates, ref inspectedStuff, ++sectionIndex).nl();

            "Wallet".enter_Inspect(Wallet, ref inspectedStuff, ++sectionIndex).nl();

            if ("Other Proficiencies".isEntered(ref inspectedStuff, ++sectionIndex).nl()) 
            {
                ArmorProficiencies.Nested_Inspect();
                WeaponProficiencies.Nested_Inspect();
                ToolProficiencies.Nested_Inspect();
            }
        }

        internal override int GetDefaultValue(Stat stat)
        {
            return 10; 
            // ToDo: Get By clas
        }

        #endregion
    }



    [Serializable]
    public class CharacterSmartId : DnD_SmartId<Character>
    {
        protected override SerializableDictionary<string, Character> GetEnities() => Data.Characters;
    }

    [Serializable]
    public class CharactersDictionary: SerializableDictionary<string, Character> { }
}
