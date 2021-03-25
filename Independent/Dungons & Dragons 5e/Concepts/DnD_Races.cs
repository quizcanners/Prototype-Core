using System.Collections.Generic;

namespace Dungeons_and_Dragons
{

    public static class CharacterRaceExtensions 
    {
        private static readonly Dictionary<Race, CharacterRace> allClasses = new Dictionary<Race, CharacterRace>()
        {
            {Race.Dwarf, new Dwarf() },
            {Race.Elf , new Elf() },
            {Race.Halfling, new Halfling() },
            {Race.Human , new Human() },
            {Race.Dragonborn, new Dragonborn() },
            {Race.Gnome , new Gnome() },
            {Race.Half_Elf, new Half_Elf() },
            {Race.Half_Ork , new Half_Ork() },
            {Race.Tiefling, new Tiefling() },

        };

        public static CharacterRace Get(this Race race) => allClasses[race];
    } 


    public enum Race { Dwarf, Elf, Halfling, Human, Dragonborn, Gnome, Half_Elf, Half_Ork, Tiefling }

    public abstract class CharacterRace
    {
        public virtual int StatIncrease(Stat stat) => 0;

        public virtual Proficiency this[Stat stat] => Proficiency.None;

        public virtual CellDistance WalkingSpeed => CellDistance.FromCells(6);

        public virtual Proficiency this[Skill skill] => Proficiency.None;

    }

         public class Dwarf : CharacterRace { }
    
         public class Elf : CharacterRace { }
    
         public class Halfling : CharacterRace { }
     public class Human : CharacterRace { }
    public class Dragonborn : CharacterRace { }
    public class Gnome : CharacterRace { }
     public class Half_Elf : CharacterRace { }
     public class Half_Ork : CharacterRace { }
     public class Tiefling : CharacterRace { }


}
