using System.Collections.Generic;
using System.Text.RegularExpressions;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

//#pragma warning disable IDE0018 // Simplify 'default' expression


   // public sealed class TriggerGroup : ICfgCustom, IGotName, IGotIndex, IPEGI, IPEGI_ListInspect {

        //public static UnNullableCfg<TriggerGroup> all = new UnNullableCfg<TriggerGroup>();
       
        //private UnNullableCfg<Trigger> _triggers = new UnNullableCfg<Trigger>();

        //public readonly UnNullableCfg<UnNullableCfgLists<Values>> taggedInts = new UnNullableCfg<UnNullableCfgLists<Values>>();

        /*
        private string _name = "Unnamed_Triggers";
        private int _index;

        #region Getters & Setters 

        public int Count => _triggers.GetAllObjsNoOrder().Count;

        public Trigger this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    var ready = _triggers.GetIfExists(index);
                    if (ready != null)
                        return ready;

                    ready = _triggers[index];
                    ready.groupId = IndexForInspector;
                    ready.triggerId = index;

                    _listDirty = true;

                    return ready;
                }

                return null;
            }
        }

        public int IndexForInspector { get { return _index; } set { _index = value; } }

        public string NameForInspector { get { return _name; } set { _name = value; } }

        private void Add(string name, ValueIndex arg = null)
        {
            var ind = _triggers.AddNew();
            var t = this[ind];
            t.name = name;
            t.groupId = IndexForInspector;
            t.triggerId = ind;

            if (arg != null)
            {
                if (arg.IsBoolean)
                    t.Usage = TriggerUsage.Boolean;
                else
                    t.Usage = TriggerUsage.Number;

                arg.Trigger = t;
            }

            _lastUsedTrigger = ind;
            
            _listDirty = true;

        }

        #endregion

        #region Encode_Decode
        
        private const string TriggersRootFolder = "Triggers";

        private bool _triedToLoad;

        public void TryLoad()
        {
            if (_triedToLoad)
            {
                return;
            }

            _triedToLoad = true;

            if (Application.isEditor && this.TryLoadFromResources(TriggersRootFolder, _index.ToString()))
            {
                return;
            }

            this.LoadFromPersistentPath(TriggersRootFolder, _index.ToString());
        }

        public void SaveToFile()
        {
            this.SaveToPersistentPath(TriggersRootFolder, _index.ToString());
        }

        public CfgEncoder Encode() =>  new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("n", _name)
            .Add("ind", _index)
            .Add_IfNotDefault("t", _triggers)
            .Add("br", _browsedGroup)
            .Add_IfTrue("show", _showInInspectorBrowser)
            .Add("last", _lastUsedTrigger);

        public void Decode(string tg, CfgData data) {
            switch (tg) {
                case "n": _name = data.ToString(); break;
                case "ind": _index = data.ToInt(); break;
                case "t":
                    data.Decode(out _triggers);
                    foreach (var t in _triggers){
                        t.groupId = _index;
                        t.triggerId = _triggers.currentEnumerationIndex;
                    }
                    break;
                case "br": _browsedGroup = data.ToInt(); break;
                case "show": _showInInspectorBrowser = data.ToBool(); break;
                case "last": _lastUsedTrigger = data.ToInt(); break;
            }
        }

        public void Decode(CfgData data)
        {
            _listDirty = true;

            this.DecodeTagsFrom(data);
        }

        #endregion

        #region Inspector

        private static int _browsedGroup = -1;
        private bool _showInInspectorBrowser = true;
        private int _lastUsedTrigger;

        public Trigger LastUsedTrigger
        {
            get { return _triggers.GetIfExists(_lastUsedTrigger); }
            set { if (value != null) _lastUsedTrigger = value.triggerId; }
        }

        public static Trigger TryGetLastUsedTrigger() => Browsed?.LastUsedTrigger;
       
        public static TriggerGroup Browsed
        {
            get { return _browsedGroup >= 0 ? all[_browsedGroup] : null; }
            set { _browsedGroup = value?.IndexForInspector ?? -1; }
        }
        
        private bool _listDirty;

        private string _lastFilteredString = "";

        private readonly List<Trigger> _filteredList = new List<Trigger>();

        public List<Trigger> GetFilteredList(ref int showMax, bool showBooleans = true, bool showInts = true)
        {
            if (!_showInInspectorBrowser)
            {
                _filteredList.Clear();
                return _filteredList;
            }

            if (!_listDirty && _lastFilteredString.SameAs(Trigger.searchField))
                return _filteredList;
            _filteredList.Clear();
            foreach (var t in _triggers)
                if ((t.IsBoolean ? showBooleans : showInts) && t.SearchWithGroupName(_name)) {
                    showMax--;

                    Trigger.searchMatchesFound++;

                    _filteredList.Add(t);

                    if (showMax < 0)
                        break;
                }

            _lastFilteredString = Trigger.searchField;

            _listDirty = false;
            return _filteredList;
        }

        public void InspectInList(ref int edited, int ind) {
            this.inspect_Name();

            if (icon.Enter.ClickUnFocus())
                edited = ind;

            if (icon.Email.Click("Send this Trigger Group to somebody via email."))
                this.EmailData("Trigger Group {0} [index: {1}]".F(_name, _index), "Use this Trigger Group in your Node Books");
        }

        private bool _shareOptions;

        public void Inspect()  {


            "[{0}] Name:".F(_index).edit(70, ref _name).nl();

            if ("Share".isFoldout(ref _shareOptions).nl())
            {
                "Share:".write(
                    "Paste message full with numbers and lost of ' | ' symbols into the first line or drop file into second",
                    50);

                CfgData data;
                if (this.SendReceivePegi("Trigger Group {0} [{1}]".F(_name, _index), "Trigger Groups", out data))
                {
                    var tmp = new TriggerGroup();
                    tmp.Decode(data);
                    if (tmp._index == _index)
                    {

                        Decode(data);
                        Debug.Log("Decoded Trigger Group {0}".F(_name));
                    }
                    else
                        Debug.LogError("Pasted trigger group had different index, replacing");
                }
                pegi.line();
            }
                
            pegi.nl();

            "New Variable".edit(80, ref Trigger.searchField);
            AddTriggerToGroup_PEGI();

            _triggers.Nested_Inspect();

        }

        private bool AddTriggerToGroup_PEGI(ValueIndex arg = null)
        {
      
            if ((Trigger.searchMatchesFound != 0) || (Trigger.searchField.Length <= 3)) return false;
            
            var selectedTrig = arg?.Trigger;

            if (selectedTrig != null)
            {
                try
                {
                    if (Regex.IsMatch(selectedTrig.name, Trigger.searchField, RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
                catch {
                    //Debug.LogError(ex);
                    return false;
                }
                
            }
            
            var changed = pegi.ChangeTrackStart();

            if (icon.Add.ClickUnFocus("CREATE [" + Trigger.searchField + "]")) 
                Add(Trigger.searchField, arg);

            return changed; 
        }

        public static bool AddTrigger_PEGI(ValueIndex arg = null) {

            var changed = pegi.ChangeTrackStart();

            var selectedTrig = arg?.Trigger;

            if (Trigger.searchMatchesFound == 0 && selectedTrig != null && !selectedTrig.name.SameAs(Trigger.searchField)) {

                var goodLength = Trigger.searchField.Length > 3;

                pegi.nl();

                if (goodLength && icon.Replace.ClickConfirm("rnmTrg",
                    "Rename {0} if group {1} to {2}".F(selectedTrig.name, selectedTrig.Group.GetNameForInspector(), Trigger.searchField)
                    )) selectedTrig.Using().name = Trigger.searchField;
                
                var differentGroup = selectedTrig.Group != Browsed && Browsed != null;

                if (goodLength && differentGroup)
                    icon.Warning.draw("Trigger {0} is of group {1} not {2}".F(pegi.GetNameForInspector(selectedTrig), selectedTrig.Group.GetNameForInspector(), Browsed.GetNameForInspector()));

                var groupLost = all.GetAllObjsNoOrder();
                if (groupLost.Count > 0) {
                    var selected = Browsed?.IndexForInspector ?? -1;

                    if (InspectSelect(ref selected, all)) 
                        Browsed = all[selected];
                }
                else
                    "No Trigger Groups found".nl();

                if (goodLength)
                    Browsed?.AddTriggerToGroup_PEGI(arg);
            }

            pegi.nl();

            return changed;
        }

        public static bool InspectSelect<T>(ref int no, CountlessCfg<T> tree) where T : ICfgCustom, new()
        {
            List<int> inds;
            var objs = tree.GetAllObjs(out inds);
            var filtered = new List<string>(objs.Count + 1);
            var tmpindex = -1;
            for (var i = 0; i < objs.Count; i++)
            {
                if (no == inds[i])
                    tmpindex = i;
                filtered.Add(objs[i].GetNameForInspector());
            }

            if (pegi.select(ref tmpindex, filtered.ToArray()))
            {
                no = inds[tmpindex];
                return true;
            }
            return false;

        }


        #endregion

        public TriggerGroup() {

            _index = all.GetCount();
            
            TryLoad();
        }
    }*/
}

