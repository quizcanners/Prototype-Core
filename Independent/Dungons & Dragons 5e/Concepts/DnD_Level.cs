using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterLevel
    {
        public int Value;

        public void Up(int add = 1)
        {
            Value = Mathf.Min(20, Value + add);
        }

        public static implicit operator int(CharacterLevel l) => l.Value;

        public int ExperienceForNextLevel => GetExperienceForLevel(Value + 1);

        public int GetExperienceForLevel() => GetExperienceForLevel(Value);

        public int ProficiencyBonus
        {
            get
            {
                if (Value < 5) return 2;
                if (Value < 9) return 3;
                if (Value < 13) return 4;
                if (Value < 17) return 5;
                return 6;
            }
        }

        public int TierOfPlay
        {
            get
            {
                if (Value < 5) return 1;
                if (Value < 11) return 2;
                if (Value < 17) return 3;
                return 4;
            }
        }

        public int GetExperienceForLevel(int Level)
        {
            switch (Level)
            {
                case 1: return 0;
                case 2: return 300;
                case 3: return 900;
                case 4: return 2700;
                case 5: return 6500;
                case 6: return 14000;
                case 7: return 23000;
                case 8: return 34000;
                case 9: return 48000;
                case 10: return 64000;
                case 11: return 85000;
                case 12: return 100000;
                case 13: return 120000;
                case 14: return 140000;
                case 15: return 165000;
                case 16: return 195000;
                case 17: return 225000;
                case 18: return 265000;
                case 19: return 305000;
                case 20: return 355000;
                default:
                    Debug.LogError("No Exp Thold for Level {0}".F(Level));
                    return int.MaxValue;
            }
        }
    }



}
