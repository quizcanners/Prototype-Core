using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/" + FILE_NAME)]
    public class GameEntities : ScriptableObject, IPEGI, ICfg
    {
        public const string FILE_NAME = "Game States";

        public UserInterfaceEntity UserInterface = new UserInterfaceEntity(); // Ui
        public ApplicationEntity Application = new ApplicationEntity(); // All users
        public PlayerEntity Player = new PlayerEntity();  // Specific user
        [SerializeField] private CfgData _cfg;

        // Cfg:


        private readonly QcFile.RelativeLocation _saveLocation = new QcFile.RelativeLocation(folderName: "Data", fileName: FILE_NAME, asBytes: false);
        
        public void Save()
        {
            _cfg = Encode().CfgData;
            QcFile.Save.ToPersistentPath.JsonTry(objectToSerialize: this, _saveLocation);
        }
        public void Load()
        {
            var tmp = this;
            QcFile.Load.FromPersistentPath.TryOverrideFromJson(_saveLocation, ref tmp);
            this.DecodeFull(_cfg);
        }

        #region Inspector
        private int _inspectedStuff = -1;
        public void Inspect()
        {
            if (_inspectedStuff == -1)
            {
                string tmp = Player.PlayerName;
                if ("User".select(ref tmp, Application.AvailableUsers).nl())
                {
                    Player.Save();
                    Player.Load(tmp);
                }
            }

            int section = -1;

            "Player".enter_Inspect(Player,                      ref _inspectedStuff, ++section).nl();
            "User Interface".enter_Inspect(UserInterface,       ref _inspectedStuff, ++section).nl();
            "Application".enter_Inspect(Application,            ref _inspectedStuff, ++section).nl();
           // "Values".enter_Inspect(NodeNotes.TriggerValues.Global,     ref _inspectedStuff, ++section).nl();
        }


        public CfgEncoder Encode() => new CfgEncoder();
        public void Decode(string key, CfgData data)
        {
          /*  switch (key) 
            {
               
            }*/
        }
        #endregion
    }
}