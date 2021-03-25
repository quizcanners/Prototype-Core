using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerEntity : IsItGameClassBase, IPEGI
    {
        private const string SAVE_FOLDER = "Player State";

        public PermanentProgression PermanentProgression = new PermanentProgression();
        public Campaign Campaign = new Campaign();

        [SerializeField] private string _playerName = "";

        public string PlayerName => _playerName;

        private QcFile.RelativeLocation GetLocation(string playerName) => new QcFile.RelativeLocation(folderName: SAVE_FOLDER, fileName: playerName, asBytes: true);
        public void Save() => QcFile.Save.ToPersistentPath.JsonTry(objectToSerialize: this, GetLocation(PlayerName));
        
        public void Load(string playerName)
        {
            try
            {
                if (!QcFile.Load.FromPersistentPath.JsonTry(GetLocation(playerName), out PlayerEntity ent))
                {
                    Debug.LogError("Couldn't load " + playerName);
                    _playerName = playerName;
                    return;
                }

                GameEntities.Player = ent;

            } catch(Exception ex) 
            {
                Debug.LogException(ex);
                return;
            }

            _playerName = playerName;
        }

        #region Inspector
        private int _inspectedStuff = -1;

        public void Inspect()
        {
            int sectionIndex = -1;

            "Permanent Progression".enter_Inspect(PermanentProgression, ref _inspectedStuff, --sectionIndex).nl();
            "Campaign".enter_Inspect(Campaign, ref _inspectedStuff, --sectionIndex).nl();
            if ("Trigger Values".isEntered(ref _inspectedStuff, --sectionIndex).nl())
                Service.Try<NodeNotes.TriggerValuesService>(s => s.Values.Nested_Inspect());

            if (_inspectedStuff == -1)
            {
                if (icon.Save.Click())
                    Save();

                if (icon.Load.Click())
                    Load(PlayerName);
            }
        }

      
        #endregion
    }
}
