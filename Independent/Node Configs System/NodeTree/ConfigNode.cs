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
        [Serializable]
        public partial class Node : IPEGI, IGotIndex, IPEGI_ListInspect, IGotName, IGotCount
        {
            [SerializeField] private string _name;
            [SerializeField] private int _index;
            [SerializeField] private int _version;
            [SerializeField] private Reference _parentNode;
            [SerializeField] private List<Reference> _childNodes = new List<Reference>();
            [SerializeField] private ConfigsDictionary configs = new ConfigsDictionary();

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
                    var n = ch.GetNode();

                    if (n.IsEntered_Internal())
                        return true;
                }
                return false;
            }


            public bool TryGetConfig (ITaggedCfg val, out CfgData dta) 
            {
                if (configs.TryGetValue(val.TagForConfig, out dta)) 
                    return true;

                var parent = _parentNode.GetNode();

                if (parent != null)
                    return parent.TryGetConfig(val, out dta);

                return false;
            }

            public bool TryUpdateConfigUpTheHierarchy(ITaggedCfg val, CfgData dta)
            {
                if (configs.ContainsKey(val.TagForConfig))
                {
                    SetConfigOnTheNode(val, dta);
                    return true;
                }

                var parent = _parentNode.GetNode();

                if (parent != null)
                    return parent.TryUpdateConfigUpTheHierarchy(val, dta);
                
                return false;
            }

            public void SetConfigOnTheNode(ITaggedCfg val, CfgData dta)
            {
                configs[val.TagForConfig] = dta;
                Service.Try<ConfigNodesService>(s => s.SetToDirty());
            }

            public Reference GetReference => new Reference(this);

            public ConfigBook GetBook() => _parentNode.GetBook();

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
                _version = book._nodesVersion;
                book._freeNodeIndex++;
                book._nodesVersion++;

                book._nodes.Add(this);
               
            }

            internal Node(ConfigBook book)
            {
                Initialize(book);
                _parentNode = new Reference(book);
                _name = "ROOT";
            }

            public Node(Node parent)
            {
                Initialize(parent.GetBook());
                _parentNode = new Reference(parent);
                parent._childNodes.Add(new Reference(this));
                _name = "Node {0}".F(_index);
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

                    if (_collectionMeta.IsInspectingElement == false && "Add Node".Click().nl())
                        new Node(this);
                }
            }

            public void InspectInList(ref int edited, int ind)
            {
                if (Mgmt.AnyEntered == false)
                {
                    if (icon.Play.Click("Enter this Node"))
                        Mgmt.SetCurrent(this);
                }
                else if (Mgmt.IsCurrent(this))
                {
                    if (_parentNode.GetNode() != null)
                    {
                        if ("To Prev".Click())
                            Mgmt.SetCurrent(_parentNode.GetNode());
                    }
                    else
                        icon.Active.draw();

                }
                else if (IsEntered)
                {
                    if ("Back Here".Click())
                        Mgmt.SetCurrent(this);
                }
                else if ("Set".Click())
                    Mgmt.SetCurrent(this);

                "ID {0}  [{1} brchs]".F(_index, GetCount()).write(90);

                pegi.inspect_Name(this);

                if (icon.Enter.Click())
                    edited = ind;
            }
            #endregion

            private class ConfigsDictionary : SerializableDictionary<string, CfgData> { }
        }
    }
}