using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class AdventureLocationsService : IsItGameServiceBase
    {

        [SerializeField] private DictionaryOfLocations _locations = new DictionaryOfLocations();
        [Serializable] private class DictionaryOfLocations : SerializableDictionary<string, AdventureNodesLocationScriptableObject> { }

        #region Inspector
        private int _inspectedLocation = -1;
        public override void Inspect()
        {
            pegi.nl();

            "Locations".edit_Dictionary(_locations, ref _inspectedLocation).nl();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(AdventureLocationsService))] internal class AdventureNodesServiceDrawer : PEGI_Inspector_Override { } 
}
