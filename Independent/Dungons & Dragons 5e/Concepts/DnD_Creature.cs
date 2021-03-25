using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public abstract class Creature : IPEGI, IGotName, IPEGI_ListInspect
    {
        protected const int STATS_COUNT = 6;

        [SerializeField] public Allignment Allignment = new Allignment();
        [SerializeField] public StatSet Stats = new StatSet();
        [SerializeField] public List<Language> LanguagesKnown = new List<Language>() { Language.Common };
        [SerializeField] protected SkillSet skillSet = new SkillSet();
        [SerializeField] protected SavingThrowProficiencies savingThrowProficiencies = new SavingThrowProficiencies();
        [SerializeField] private string _name;
        [SerializeField] private SensesDictionary senses = new SensesDictionary();

        public abstract CellDistance this[SpeedType type] { get; }
        public bool this[Language lang] 
        {
            get => LanguagesKnown.Contains(lang);
            set 
            {
                var contains = LanguagesKnown.Contains(lang);
                if (value && !contains) 
                {
                    LanguagesKnown.Add(lang);
                } else if (!value && contains) 
                {
                    LanguagesKnown.Remove(lang);
                }    
            }
        }
        public abstract Size Size { get; }
        public CellDistance this [Sense sense]  => senses.TryGet(sense);
        protected int this[Proficiency prof]
        {
            get
            {
                switch (prof)
                {
                    case Proficiency.None: return 0;
                    case Proficiency.Normal: return ProficiencyBonus;
                    case Proficiency.Expertiese: return ProficiencyBonus * 2;
                    default:
                        Debug.LogError(QcLog.CaseNotImplemented(prof,  "Creature")); return 0;
                }
            }
        }
        protected abstract int ProficiencyBonus { get; }
        public abstract int ArmorClass { get; }
       
        #region Rolls
        public abstract int RollMaxHp();
        public virtual RollResult Roll(Skill skill, Influence influence = Influence.None) => Dice.D20.Roll(influence: influence) + this[skill];
        public virtual RollResult RollSavingThrow(Stat stat, Influence influence = Influence.None) => Dice.D20.Roll(influence: influence) + SavingThrowBonus(stat);
        #endregion

        public int CalculateDamage(int damage, DamageType damageType) 
        {
            var resistence = GetDamageResistance(damageType);

            switch (resistence) 
            {
                case DamageResistance.Immunity: return 0;
                case DamageResistance.None: return damage;
                case DamageResistance.Resistance: return damage / 2;
                case DamageResistance.Vulnerability: return damage * 2;
                default: QcLog.CaseNotImplemented(resistence, context: nameof(CalculateDamage)); return damage;
            }
        }

        public virtual DamageResistance GetDamageResistance(DamageType damageType) => DamageResistance.None;
        protected virtual int SavingThrowBonus(Stat stat) => GetStatBonus(stat) + this[SavingThrowProficiency(stat)];

        #region Stats
        protected int GetRolledStat(Stat stat)
        {
            if (Stats.TryGetValue(stat, out var st))
                return st.Value;
            else
                return GetDefaultValue(stat);
        }
        internal abstract int GetDefaultValue(Stat stat);
        internal int Get(Stat stat) => GetRolledStat(stat) + GetStatValueBonuses(stat);
        internal int GetStatBonus(Stat stat) => Mathf.FloorToInt((Get(stat) - 10) / 2);
        protected abstract int GetStatValueBonuses(Stat stat);
        #endregion

        protected virtual Proficiency SavingThrowProficiency(Stat stat) => savingThrowProficiencies[stat];
        protected virtual Proficiency GetProficiency(Skill stat) => skillSet[stat];
        protected virtual int this[Skill skill] => GetStatBonus(skill.GetStat()) + this[GetProficiency(skill)];
      
       
        #region Inspector

        internal static Creature inspectedCreature;

        protected int inspectedStuff = -1;
        public string NameForInspector { get => _name; set => _name = value; }

        public virtual void Inspect()
        {
            inspectedCreature = this;

            var tmp = this;
         
            if (inspectedStuff == -1) 
            {
                pegi.CopyPaste.InspectOptionsFor(ref tmp);

                "Name [Key]".edit(90, ref _name).nl();

                Allignment.Nested_Inspect();

                "AC:{0} | Prof: {1} |  Speed: {2}".F(ArmorClass, ProficiencyBonus, this[SpeedType.Walking]).nl();
            }

            if ("Stats".isEntered(ref inspectedStuff, 0).nl())
            {
                if ("Reroll".ClickConfirm(confirmationTag: "ReRollSt", toolTip: "This will rerolls all your stats.").nl())
                {
                    for (int i = 0; i < STATS_COUNT; i++)
                        Stats[(Stat)i] = new CoreStat(Dice.D6.Roll(4, Drop.Lowest));
                }

                Stats.Nested_Inspect().nl();
            }

            if ("Skills".isEntered(ref inspectedStuff, 1).nl())
            {
                var skills = (Skill[])Enum.GetValues(typeof(Skill));

                for (int i = 0; i < skills.Length; i++)
                {
                    Skill skill = skills[i];

                    skillSet.Inspect(skill);

                    "{0} {1} ({2})".F(this[skill].ToSignedNumber(), skill.ToString(), skill.GetStat().GetShortName()).write(110);

                    if (icon.Dice.Click())
                        pegi.GameView.ShowNotification (Roll(skill).ToString());

                    pegi.nl();
                }
            }

            if ("Saving Throws".isEntered(ref inspectedStuff, 2).nl()) 
            {
                for (int i=0; i< STATS_COUNT; i++) 
                {
                    var enm = ((Stat)i);

                    //var prof = SavingThrowProficiency(enm);

                    //prof.GetIcon().draw();

                    savingThrowProficiencies.Inspect(enm);

                    "{0} ({1})".F(enm.ToString(), SavingThrowBonus(enm).ToSignedNumber()).write();

                    if (icon.Dice.Click())
                        pegi.GameView.ShowNotification(RollSavingThrow(enm).ToString());

                    pegi.nl();
                }
            }

            if ("Senses".isEntered(ref inspectedStuff, 3).nl()) 
            {
                senses.Nested_Inspect();
            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            var name = NameForInspector;
            if (pegi.editDelayed(ref name))
                NameForInspector = name;

            if (NameForInspector.Length > 0 && icon.Clear.ClickConfirm(confirmationTag: "chTg" + NameForInspector, toolTip: "Name is used as a Key. Are you sure?"))
                NameForInspector = "";

            if (icon.Enter.Click())
                edited = ind;
        }

        #endregion

    }
}