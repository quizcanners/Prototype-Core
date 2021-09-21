using QuizCanners.Inspect;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Utils;
using QuizCanners.Migration;
using System;

namespace QuizCanners.IsItGame.NodeNotes
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/Node Configs/" + FILE_NAME)]
    public partial class ConfigBookScriptableObject : ScriptableObject, IPEGI, IPEGI_ListInspect, IGotName, ICfg, ISerializationCallbackReceiver
    {
        public const string FILE_NAME = "Node Book";
        public const string KEY_SUFFIX = ".key";

        private Node _rootNode = new Node();
        [NonSerialized] private List<Node> _cachedAllNodes = new List<Node>();
        public int Version { get; private set; }

        public void OnNodeTreeChanged()
        {
            Version++;
            _cachedAllNodes = new List<Node>();
            _encoded = false;
        }
      
        public List<Node> GetAllNodes() 
        {
            if (_cachedAllNodes.IsNullOrEmpty()) 
            {
                var node = GetRootNode();
                node.PopulateAllNodes(_cachedAllNodes);
            }

            return _cachedAllNodes;
        }

        private Node GetRootNode() 
        {
            if (_encoded)
            {
                this.DecodeFull(_encodedNodes);
                OnNodeTreeChanged();
            }

            return _rootNode;
        }

        private Node RootNode
        {
            set 
            {
                _rootNode = value;
                _encoded = false;
            }
        }

        [SerializeField] private CfgData _encodedNodes;
        [SerializeField] private bool _encoded;
        [SerializeField] private int _freeNodeIndex;

        public void OnBeforeSerialize()
        {
            if (!_encoded)
            {
                _encodedNodes = Encode().CfgData;
                _encoded = true;
            }
        }

        public void OnAfterDeserialize() { }

        public NodesChain this[Node.Reference reff]
        {
            get
            {
                NodesChain c = new NodesChain(this);
                GetRootNode().TryGet(reff, c);
                return c;
            }
        }


        private void RepopulateNodesChain(NodesChain c) => GetRootNode().TryGet(c.GetReferenceToLastNode(), c);
        
        #region Encode & Decode 

        public CfgEncoder Encode() =>new CfgEncoder()
                .Add("n", _rootNode);

        public void Decode(string key, CfgData data)
        {
            switch (key) 
            {
                case "n": _rootNode.DecodeFull(data); break;
            }
        }

        #endregion

        #region Inspector

        private static NodesChain _chain_ForInspector;

        private int _inspectedStuff = -1;

        public string NameForInspector 
        { 
            get => (this) ? name : "NULL"; 
            set => QcUnity.RenameAsset(this,value); 
        }

        public void Inspect()
        {
            using (_chain_ForInspector = new NodesChain(this))
            {
                if (Service.Collector.InspectionWarningIfMissing<ConfigNodesService>().nl())
                    return;

                if (_inspectedStuff == -1 && "Clear".ClickConfirm(confirmationTag: "DelBook", toolTip: "This will destroy all the nodes. Are you sure?"))
                {
                    _freeNodeIndex = 0;
                    RootNode = new Node(this);
                    OnNodeTreeChanged();
                }

                GetRootNode().Nested_Inspect();

                pegi.nl();
            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            using (_chain_ForInspector = new NodesChain(this))
            {
                var tmp = NameForInspector.Replace(KEY_SUFFIX, ""); //name;

                if (pegi.editDelayed(ref tmp))
                    NameForInspector = tmp + KEY_SUFFIX;

                if (icon.Enter.Click())
                    edited = ind;

                this.ClickHighlight();
            }
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(ConfigBookScriptableObject))] internal class NodeBookDrawer : PEGI_Inspector_Override { }

}
