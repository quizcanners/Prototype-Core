using QuizCanners.Inspect;
using UnityEngine;
using QuizCanners.Migration;

namespace QuizCanners.IsItGame
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/" + FILE_NAME)]
    public class SceneLighting_Configurations : ConfigurationsSO_Generic<WeatherConfig>, IPEGI
    {
        public const string FILE_NAME = "Scene Lighting Config";


    }

    public class WeatherConfig : Configuration
    {
        public static Configuration activeConfig;

        public override Configuration ActiveConfiguration
        {
            get { return activeConfig; }
            set
            {
                activeConfig = value;
                SceneLightingManager.inspected.Decode(value.data);
            }
        }

        public override CfgEncoder EncodeData() => SceneLightingManager.inspected.Encode();

    }
}