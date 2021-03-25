
using QuizCanners.Inspect;

namespace Dungeons_and_Dragons
{
    [System.Serializable]
    public class SkillCheck : IPEGI
    {
        public Skill SkillToCheck = Skill.Perception;
        public int DC = 15;

        public bool Check(Creature creature, Influence influence) => creature.Roll(SkillToCheck, influence) >= DC;

        public void Inspect()
        {
            "DC".edit(40, ref DC, 50);
            "    ".write(50);
            pegi.editEnum(ref SkillToCheck, width: 90);
          
        }
    }
}