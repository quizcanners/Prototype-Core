using QuizCanners.Migration;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public class ClassFeatureStates : SerializableDictionaryForTaggedTypesEnum<ClassFeature.Base> 
    { 
    }

    public class ClassFeature
    {
        public abstract class Base : IGotClassTag
        {
            public abstract string ClassTag { get; }
        }

        [Serializable]
        [TaggedTypes.Tag(CLASS_KEY)]
        public class Proficiencies : Base, IPEGI
        {
            public const string CLASS_KEY = "Proficiencies";
            public override string ClassTag => CLASS_KEY;

            public SkillSet Skills = new SkillSet();

            public void Inspect() => Skills.Nested_Inspect();
        }

        [Serializable]
        [TaggedTypes.Tag(CLASS_KEY)]
        public class Spellcasting : Base
        {
            public const string CLASS_KEY = "Spellcasting";
            public override string ClassTag => CLASS_KEY;
        }

        [Serializable]
        [TaggedTypes.Tag(CLASS_KEY)]
        public class SpiritTotem : Base
        {
            public const string CLASS_KEY = "Spirit_Totem";
            public override string ClassTag => CLASS_KEY;
        }
    }
}