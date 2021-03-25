using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class Campaign : IsItGameClassBase, IPEGI, ISerializationCallbackReceiver
    {
        public Dungeons_and_Dragons.CharacterSmartId MainHero = new Dungeons_and_Dragons.CharacterSmartId(); //Character MainHero = new Dungeons_and_Dragons.Character();
        public Dungeons_and_Dragons.PartySmartId Allies = new Dungeons_and_Dragons.PartySmartId();

        [SerializeField] private string _triggerValuesJson;


        public void OnBeforeSerialize()
        {
            Utils.Service.Try<NodeNotes.TriggerValuesService>(s => _triggerValuesJson = JsonUtility.ToJson(s.Values));
        }

        public void OnAfterDeserialize()
        {
            Utils.Service.Try<NodeNotes.TriggerValuesService>(s => JsonUtility.FromJsonOverwrite(_triggerValuesJson, s.Values));
        }

        private int _inspectedStuff = -1;
        public void Inspect()
        {
            int section = -1;

            "Main Hero".enter_Inspect(MainHero, ref _inspectedStuff, ++section).nl();
            "Allies".enter_Inspect(Allies, ref _inspectedStuff, ++section).nl();
        }
    }
}
