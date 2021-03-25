using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class SettingsWindowVIew : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            pegi.nl();

            var s = Service.Try<SoundsService>(s =>
            {
                var snd = s.WantSound;
                if ("Sound".toggleIcon(ref snd).nl())
                    Service.Get<SoundsService>().WantSound = s;
            });
        }
    }

    [PEGI_Inspector_Override(typeof(SettingsWindowVIew))]
    internal class SettingsWindowVIewDrawer : PEGI_Inspector_Override { }
}