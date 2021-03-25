using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    public enum Drop { None = 0, Highest = 1, Lowest = 2 }

    public enum Influence { None = 0, Disadvantage = 1, Advantage = 2, Cancelled = 3 }

    public enum Dice
    {
        D2 = 2, D4 = 4, D6 = 6, D8 = 8, D10 = 10, D12 = 12, D20 = 20, D100 = 100
    }
    

    public static class DiceUtils
    {
        #region Inspect
        public static bool Inspect(this List<Dice> list)
        {
            var changed = pegi.ChangeTrackStart();
            int toDelete = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                        
                if (icon.Delete.Click())
                    toDelete = i;
                        
                if (pegi.editEnum(ref d))
                    list[i] = d;
                        
                pegi.nl();
            }

            if (toDelete != -1)
                list.RemoveAt(toDelete);

            if ("Add Dice".Click())
            {
                list.Add(list.Count == 0 ? Dice.D20 : list.Last());
            }
            
            return changed;
        }
        
        #endregion

        public static RollResult Roll(this List<Dice> list)
        {
            RollResult sum = new RollResult();

            if (!list.IsNullOrEmpty())
                foreach (var dice in list)
                    sum += dice.Roll();
            
            return sum;
        }
        
        public static RollResult MinRoll(this List<Dice> list)
        {
            if (list.IsNullOrEmpty())
                return new RollResult();
                
            return RollResult.From(list.Count);
        }
        
        public static RollResult MaxRoll(this List<Dice> list)
        {
            if (list.IsNullOrEmpty())
                return new RollResult();

            RollResult max = new RollResult();
            
            foreach(var el in list)
                max += (int)el;
            
            return max;
        }
        
        public static string ToDescription(this List<Dice> list, bool showPossibiliesNumber = false)
        {    
            if (list.IsNullOrEmpty())
                return "NO DICE";
            
            StringBuilder sb = new StringBuilder(list.Count * 4);

            for (int i=0; i< list.Count; i++)
            {
                if (i>0)
                    sb.Append('+');
                sb.Append(list[i].ToString());
            }
            
            if (list.Count > 1) 
            {
                sb.Append('=')
                    .Append(list.MaxRoll())
                    ;

                if (showPossibiliesNumber)
                {
                    sb.Append(" (")
                        .Append((list.MaxRoll() - list.MinRoll() + 1).ToString())
                        .Append(" values)");
                }

            }

        

            return sb.ToString();
        }
        
        public static RollResult Roll(this Dice dice, Influence influence, int diceCount = 1)
        {
            if (influence == Influence.None)
                return dice.Roll(diceCount);

            RollResult sum = new RollResult();

            Drop drop = influence == Influence.Advantage ? Drop.Lowest : Drop.Highest;

            for (int i = 0; i < diceCount; i++)
            {
                sum += dice.Roll(diceCount: 2, drop);
            }

            return sum;
        }

        public static RollResult Roll(this Dice dice, int diceCount, Drop drop)
        {
            if (drop == Drop.None)
                return dice.Roll(diceCount);

            RollResult sum = new RollResult();
            RollResult toDrop = new RollResult();

            for (int i = 0; i < diceCount; i++)
            {
                RollResult roll = Roll(dice);

                bool needToDrop = toDrop == 0 || (drop == Drop.Highest && roll > toDrop) || (drop == Drop.Lowest && roll < toDrop);

                if (needToDrop)
                {
                    sum += toDrop;
                    toDrop = roll;
                }
                else
                {
                    sum += roll;
                }
            }

            return sum;
        }

        public static RollResult Roll(this Dice dice, int diceCount = 1)
        {
            int value = (int)dice;

            if (diceCount == 1)
                return Roll_Internal(value);

            RollResult sum = new RollResult();

            for (int i = 0; i < diceCount; i++)
            {
                sum += Roll_Internal(value);
            }

            return sum;
        }

        private static RollResult Roll_Internal(int value) 
            => RollResult.From(Random.Range(minInclusive: 1, maxExclusive: value + 1));

        public static int AvargeRoll(this Dice dice, int diceCount = 1) =>
             ((int)dice * diceCount + diceCount) / 2;

        public static Influence And(this Influence A, Influence B)
        {
            if (A == Influence.Cancelled || B == Influence.Cancelled)
                return Influence.Cancelled;

            if (A == Influence.None)
                return B;

            if (B == Influence.None)
                return A;

            if (A == B)
                return A;

            return Influence.Cancelled;
        }
    }
}

