using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons {

    public static class SkillsExtensions 
    {
        public static Stat GetStat (this Skill skill) 
        {
            switch (skill) 
            {
                case Skill.Acrobatics:        return Stat.Dexterity;
                case Skill.Animal_Handling:   return Stat.Wisdom;
                case Skill.Arcana:            return Stat.Intelligence;
                case Skill.Athletics:         return Stat.Strength;
                case Skill.Deception:         return Stat.Charisma;
                case Skill.History:           return Stat.Intelligence;
                case Skill.Insight:           return Stat.Wisdom;
                case Skill.Intimidation:      return Stat.Charisma;
                case Skill.Investigation:     return Stat.Intelligence;
                case Skill.Medicine:          return Stat.Wisdom;
                case Skill.Nature:            return Stat.Intelligence;
                case Skill.Perception:        return Stat.Wisdom;
                case Skill.Performance:       return Stat.Charisma;
                case Skill.Persuasion:        return Stat.Charisma;
                case Skill.Religion:          return Stat.Intelligence;
                case Skill.Sleight_of_Hand:   return Stat.Dexterity;
                case Skill.Stealth:           return Stat.Dexterity;
                case Skill.Survival:          return Stat.Wisdom;
                default: Debug.LogError("{0} skill not implemented".F(skill)); return Stat.Wisdom;
            }   
        }
    }



    public enum Skill 
    {
        Acrobatics = 1,
        Animal_Handling = 2,
        Arcana = 3,
        Athletics = 4,
        Deception = 5,
        History = 6,
        Insight = 7,
        Intimidation = 8,
        Investigation = 9,
        Medicine = 10,
        Nature = 11,
        Perception = 12,
        Performance = 13,
        Persuasion = 14,
        Religion = 15,
        Sleight_of_Hand = 16,
        Stealth = 17,
        Survival = 18,
    }

}