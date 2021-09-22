using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    public partial class ConfigBookScriptableObject
    {

        public partial class Node : IGotIndex, IGotName, IGotCount, ICfg, IPEGI_ListInspect, IPEGI
        {
            private string _name = "UNNAMED";
            private int _index;
            private List<Node> _childNodes = new List<Node>();
            private ConfigsDictionary configs = new ConfigsDictionary();

            public bool TryGet(Reference reff, NodesChain result) 
            {
                var token = result.AddAndUse(this);

                if (_index == reff.NodeIndex) 
                    return true;
                
                foreach (var n in _childNodes)
                    if (n.TryGet(reff, result))
                        return true;

                token.Dispose();

                return false;
            }

            internal void PopulateAllNodes(List<Node> list) 
            {
                list.Add(this);
                foreach (var n in _childNodes)
                    n.PopulateAllNodes(list);
            }

            #region Encode & Decode

            public CfgEncoder Encode()
            {
                var cody = new CfgEncoder()
                    .Add_String("n", _name)
                    .Add("i", _index)
                    .Add("c", _childNodes)
                    .Add("cfgs", configs);

                return cody;
            }


            public void Decode(string key, CfgData data)
            {
                switch (key) {
                    case "n": _name = data.ToString(); break;
                    case "i": _index = data.ToInt(); break;
                    case "c": data.ToList(out _childNodes); break;
                    case "cfgs": data.ToDictionary(out configs); break;
                }
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

            internal bool TryGetConfig (ITaggedCfg val, out CfgData dta) 
            {
                if (configs.TryGetValue(val.TagForConfig, out dta)) 
                    return true;

                return false;
            }

            public void SetConfigOnTheNode(ITaggedCfg val, CfgData dta)
            {
                configs[val.TagForConfig] = dta;
                Service.Try<ConfigNodesService>(s => s.SetToDirty());
            }

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

            private void Initialize(ConfigBookScriptableObject book) 
            {
                _index = book._freeNodeIndex;
                book._freeNodeIndex++;
                book.OnNodeTreeChanged();
            }

            internal Node(ConfigBookScriptableObject book)
            {
                Initialize(book);
                _name = IndexForInspector.ToString();
            }

            public Node()
            {
                _name = "ROOT";
            }

            #region Inspector

            [NonSerialized] private CollectionMetaData _collectionMeta;
            public int GetCount() => _childNodes.Count;

            [NonSerialized] private bool _showConfigs;
            private static string _inspectedService = "";

            private string CfgsCount => configs.Count == 0 ? "" :
                (" " + (configs.Count == 1 ? configs.GetElementAt(0).Key : configs.Count.ToString()));

            public void Inspect(NodesChain myChain) 
            {
                if (_collectionMeta == null)
                    _collectionMeta = new CollectionMetaData(_name, showAddButton: false, allowDeleting: false, showEditListButton: false);

                if (_collectionMeta.IsInspectingElement == false)
                    InspectSetCurrentOptions();

                pegi.nl();

                if ("Configurations {0}".F(CfgsCount).isConditionally_Entered(canEnter: Mgmt.IsCurrent(this), ref _showConfigs).nl())
                {
                    var lst = Service.GetAll<ITaggedCfg>();

                    bool inspectingService = !_inspectedService.IsNullOrEmpty();

                    if (inspectingService && icon.Back.Click() || "Exit {0}".F(_inspectedService).ClickLabel())
                        _inspectedService = "";

                    pegi.nl();

                    foreach (var s in lst)
                    {
                        var tag = s.TagForConfig;

                        if (inspectingService)
                        {
                            if (tag.Equals(_inspectedService))
                            {
                                pegi.Try_Nested_Inspect(s);
                            }
                        }
                        else
                        {
                            if (configs.ContainsKey(tag))
                            {
                                if (icon.Save.Click("Save Changes"))
                                    configs[tag] = s.Encode().CfgData;
                            }
                            else
                            {
                                if (icon.SaveAsNew.Click("Create Settings Override for this node"))
                                    configs[tag] = s.Encode().CfgData;
                            }

                            // TODO: Fallback to parent nodes to save changes
                            if (icon.Enter.Click() || s.GetNameForInspector().ClickLabel())
                                _inspectedService = s.TagForConfig;

                        }

                        pegi.nl();
                    }
                }

                if (!_showConfigs)
                {
                  

                    _collectionMeta.Label = _name;
                    _collectionMeta.edit_List(_childNodes).nl();

                    if (_collectionMeta.IsInspectingElement == false && "Add Node".Click().nl())
                        _childNodes.Add(new Node(_chain_ForInspector.Book));
                }

              
            }

            public void Inspect()
            {
                using (InspectChainUse())
                {
                    Inspect(_chain_ForInspector);
                }
            }

            private void InspectSetCurrentOptions() 
            {
                if (Mgmt.AnyEntered == false)
                {
                    if (icon.Play.Click("Enter this Node"))
                        Mgmt.SetCurrent(_chain_ForInspector);
                }
                else if (Mgmt.IsCurrent(this))
                {
                    if (_chain_ForInspector.Nodes.Count > 1)
                    {
                        if ("To Prev".Click())
                            Mgmt.SetCurrent(_chain_ForInspector.GetNodeInChain(_chain_ForInspector.Nodes.Count - 2));
                    }
                    else
                        icon.Active.draw();
                }
                else if (IsEntered)
                {
                    if ("Back Here".Click())
                        Mgmt.SetCurrent(_chain_ForInspector);
                }
                else if ("Set".Click())
                    Mgmt.SetCurrent(_chain_ForInspector);
            }

            public void InspectInList(ref int edited, int ind)
            {

                using (InspectChainUse())
                {
                    if (icon.Enter.Click())
                        edited = ind;

                    if ("ID {0}  [{1} brchs{2}]".F(_index, GetCount(), CfgsCount).ClickLabel(width: 120))
                        edited = ind;

                    pegi.inspect_Name(this);

                    InspectSetCurrentOptions();
                }
            }

            private IDisposable InspectChainUse() => _chain_ForInspector.AddAndUse(this);

            #endregion

            private class ConfigsDictionary : SerializableDictionary<string, CfgData> { }
        }
    }
}