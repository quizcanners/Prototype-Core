using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    public partial class ConfigBook
    {

        public partial class Node : IGotIndex, IGotName, IGotCount, ICfg, IPEGI, IPEGI_ListInspect
        {
            private string _name;
            private int _index;
            private List<Node> _childNodes = new List<Node>();
            private ConfigsDictionary configs = new ConfigsDictionary();

            #region Encode & Decode

            public CfgEncoder Encode()
            {
                var cody = new CfgEncoder()
                    .Add_String("n", _name)
                    .Add("i", _index)
                    .Add("c", _childNodes);

                return cody;
            }

            public void Decode(string key, CfgData data)
            {

            }

            #endregion


            private ConfigNodesService Mgmt => Service.Get<ConfigNodesService>();

            public bool IsEntered 
            { 
                get 
                {
                    if (Mgmt.AnyEntered) 
                        return IsEntered_Internal();
                    
                    return false;
                } 
            }

            private bool IsEntered_Internal() 
            {
                if (Mgmt.IsCurrent(this))
                    return true;

                foreach (var ch in _childNodes)
                {
                    if (ch.IsEntered_Internal())
                        return true;
                }
                return false;
            }

            public bool TryGetConfig (ITaggedCfg val, out CfgData dta) 
            {
                if (configs.TryGetValue(val.TagForConfig, out dta)) 
                    return true;

                return false;
            }

            public bool TryUpdateConfigUpTheHierarchy(ITaggedCfg val, CfgData dta)
            {
                if (configs.ContainsKey(val.TagForConfig))
                {
                    SetConfigOnTheNode(val, dta);
                    return true;
                }
                
                return false;
            }

            public void SetConfigOnTheNode(ITaggedCfg val, CfgData dta)
            {
                configs[val.TagForConfig] = dta;
                Service.Try<ConfigNodesService>(s => s.SetToDirty());
            }

            public FullReference GetReference => new FullReference(this);

            public int IndexForInspector
            {
                get => _index;
                set
                {
                    _index = value;
                }
            }

            public string NameForInspector
            {
                get => _name;
                set => _name = value;
            }

            private void Initialize(ConfigBook book) 
            {
                _index = book._freeNodeIndex;
                book._freeNodeIndex++;
            }

            internal Node(ConfigBook book)
            {
                Initialize(book);
                _name = "ROOT";
            }

            public Node()
            {

            }

            #region Inspector

            [NonSerialized] private CollectionMetaData _collectionMeta;
            public int GetCount() => _childNodes.Count;

            [NonSerialized] private bool _showConfigs;

            public void Inspect()
            {
                if (Mgmt.IsCurrent(this))
                {
                    if ("Configurations".isEntered(ref _showConfigs).nl())
                    {

                    }
                }
                else _showConfigs = false;

                if (!_showConfigs)
                {

                    if (_collectionMeta == null)
                        _collectionMeta = new CollectionMetaData(_name, showAddButton: false, allowDeleting: false, showEditListButton: false);

                    _collectionMeta.Label = _name;
                    _collectionMeta.edit_List(_childNodes).nl();

                   // if (_collectionMeta.IsInspectingElement == false && "Add Node".Click().nl())
                      //  new Node(this);
                }
            }

            public void InspectInList(ref int edited, int ind)
            {

                if (icon.Enter.Click())
                    edited = ind;

                if ("ID {0}  [{1} brchs]".F(_index, GetCount()).ClickLabel(width: 120))
                    edited = ind;

                pegi.inspect_Name(this);

                if (Mgmt.AnyEntered == false)
                {
                    if (icon.Play.Click("Enter this Node"))
                        Mgmt.SetCurrent(this);
                }
               /* else if (Mgmt.IsCurrent(this))
                {
                    if (_parentNode.Node != null)
                    {
                        if ("To Prev".Click())
                            Mgmt.SetCurrent(_parentNode.Node);
                    }
                    else
                        icon.Active.draw();
                }*/
                else if (IsEntered)
                {
                    if ("Back Here".Click())
                        Mgmt.SetCurrent(this);
                }
                else if ("Set".Click())
                    Mgmt.SetCurrent(this);

            }
            #endregion

            private class ConfigsDictionary : SerializableDictionary<string, CfgData> { }
        }
    }
}