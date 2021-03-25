using QuizCanners.Utils;
using System.Collections.Generic;


namespace Dungeons_and_Dragons
{
    public static class CharacterClassesExtensions
    {
        private static readonly Dictionary<Class, CharacterClass> allClasses = new Dictionary<Class, CharacterClass>()
        {
            {Class.Barbarian, new Barbarian() },
             {Class.Bard , new Bard() },
             {Class.Cleric , new Cleric() },
             {Class.Druid , new Druid() },
             {Class.Fighter , new Fighter() },
             {Class.Monk, new Monk() },
             {Class.Paladin , new Paladin() },
             {Class.Ranger , new Ranger() },
             {Class.Rogue , new Rogue() },
             {Class.Sorcerer , new Sorcerer() },
             {Class.Warlock , new Warlock() },
             {Class.Wizard , new Wizard() },
        };

        public static CharacterClass Get(this Class cl) => allClasses[cl];


        public static Dice GetHitDie (this Class cl)
        {
            switch (cl) 
            {
                case Class.Barbarian: return Dice.D12;
                case Class.Bard: return Dice.D8;
                case Class.Cleric: return Dice.D8;
                case Class.Druid: return Dice.D8;
                case Class.Fighter: return Dice.D10;
                case Class.Monk: return Dice.D8;
                case Class.Paladin: return Dice.D10;
                case Class.Ranger: return Dice.D10;
                case Class.Rogue: return Dice.D8;
                case Class.Sorcerer: return Dice.D6;
                case Class.Warlock: return Dice.D8;
                case Class.Wizard: return Dice.D6;

                default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(cl, context: "GetHitDie")); return Dice.D8;
            }
        }


    }

    public enum Class
    {
        Barbarian = 0,
        Bard = 1,
        Cleric = 2,
        Druid = 3,
        Fighter = 4,
        Monk = 5,
        Paladin = 6,
        Ranger = 7,
        Rogue = 8,
        Sorcerer = 9,
        Warlock = 10,
        Wizard = 11,

    }

    public abstract class CharacterClass
    {

        public virtual Proficiency this[Stat stat] => Proficiency.None;

        public virtual Stat GetStatByPrioroty (int priorityZeroIsHighest) 
        {
            switch (priorityZeroIsHighest) 
            {
                case 0: return Stat.Dexterity;
                case 1: return Stat.Constitution;
                case 2: return Stat.Strength;
                case 3: return Stat.Intelligence;
                case 4: return Stat.Charisma;
                case 5: return Stat.Wisdom;
                default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(priorityZeroIsHighest, context: "GetStatByPrioroty")); return Stat.Constitution;
            }
        }

        public virtual bool GotFeature<T>() where T : ClassFeature.Base => false;
    }

    public class Barbarian : CharacterClass
    {
    }

   
    public class Bard : CharacterClass
    {
    }
    public class Cleric : CharacterClass
    {
    }
    public class Druid : CharacterClass
    {
    }
    public class Fighter : CharacterClass
    {
    }

    public class Monk : CharacterClass
    {
    }
    public class Paladin : CharacterClass
    {
    }
    public class Ranger : CharacterClass
    {
    }
    public class Rogue : CharacterClass
    {
    }
    public class Sorcerer : CharacterClass
    {
    }
    public class Warlock : CharacterClass
    {
    }

    public class Wizard : CharacterClass
    {
    }

}