using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Party : IPEGI, IGotName, IGotCount
    {
        [SerializeField] private string _partyName = "Unnamed";
        public List<CharacterSmartId> PartyMembers = new List<CharacterSmartId>();

        [SerializeField] private int _inspecedPartyMember = -1;

        public string NameForInspector 
        { 
            get => _partyName; 
            set => _partyName = value; 
        }

        public void Inspect()
        {
            if (_inspecedPartyMember == -1)
            {
                "Party Name (Key)".editDelayed(ref _partyName).nl();
            }

            "Members".edit_List(PartyMembers, ref _inspecedPartyMember).nl();
        }

        public int GetCount() => PartyMembers.Count;
    }


    [Serializable]
    public class PartySmartId : DnD_SmartId<Party>
    {
        protected override SerializableDictionary<string, Party> GetEnities() => Data.Parties;
    }

    [Serializable]
    public class PartiesDictionary : SerializableDictionary<string, Party> { } 
}
