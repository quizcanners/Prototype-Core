using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.NodeNotes;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [ExecuteAlways]
    public class AdventureLocationsService : IsItGameServiceBase, ITaggedCfg
    {
        public string TagForConfig => "AdvLocations";


        [SerializeField] private DictionaryOfLocations _locations = new DictionaryOfLocations();
        [Serializable] private class DictionaryOfLocations : SerializableDictionary<string, AdventureNodesLocationScriptableObject> { }


        // Cfg:
        [NonSerialized] private string _currentLocation = "";
        [NonSerialized] public List<TableRollResult> RollResults = new List<TableRollResult>();

        #region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
           .Add_String("l", _currentLocation)
            .Add("rs", RollResults);

        public void Decode(string key, CfgData data)
        {
            switch (key) 
            {
                case "l": _currentLocation = data.ToString(); break;
                case "rs": data.ToList(out RollResults); break;
            }
        }

        #endregion

        #region Inspector
        private int _inspectedStuff = -1;
        private int _inspectedLocation = -1;
        private int _inspectedRoll = -1;
        public override void Inspect()
        {
            pegi.nl();

            if (_inspectedStuff == -1)
            {
                if (Service.Get<ConfigNodesService>().AnyEntered)
                {
                    "Current Location".select(ref _currentLocation, _locations);
                    "Roll Results".edit_List(RollResults, ref _inspectedRoll);
                }
                else
                    "No node entered to show Configurable options".writeHint();
            } 
            

            if ("Locations".isEntered(ref _inspectedStuff, 0).nl())
                "Locations".edit_Dictionary(_locations, ref _inspectedLocation).nl();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(AdventureLocationsService))] internal class AdventureNodesServiceDrawer : PEGI_Inspector_Override { } 
}
