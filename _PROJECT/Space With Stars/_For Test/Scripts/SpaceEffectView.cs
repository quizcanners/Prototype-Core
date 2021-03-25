using Mushroom;
using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{

    public class SpaceEffectView : IsItGameOnGuiBehaviourBase, IPEGI
    {
        public override void Inspect()
        {
            Service.Try<SpaceAndStarsController>(s => s.Nested_Inspect());
        }
    }
}