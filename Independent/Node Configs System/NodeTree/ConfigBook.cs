using QuizCanners.Inspect;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.NodeNotes
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/Node Configs/" + FILE_NAME)]
    public partial class ConfigBook : ScriptableObject, IPEGI, IPEGI_ListInspect, IGotName
    {
        public const string FILE_NAME = "Node Book";
        public const string KEY_SUFFIX = ".key";

        [SerializeField] private int _nodesVersion = 0;
        [SerializeField] private int _freeNodeIndex;
        [SerializeField] private List<Node> _nodes = new List<Node>();

        public Node this[Node.Reference reff]
        {
            get
            {
                var n = _nodes[reff.NodeIndex];
                return reff.IsReferenceTo(n) ? n : null;
            }
        }

        #region Inspector
        private readonly CollectionMetaData _metaData = new CollectionMetaData("Nodes", allowDeleting: false, showAddButton: false);
        private int _inspectedStuff = -1;

        public string NameForInspector 
        { 
            get => (this) ? name : "NULL"; 
            set => QcUnity.RenameAsset(this,value); 
        }

        public void Inspect()
        {

            if (Service.Collector.InspectionWarningIfMissing<ConfigNodesService>().nl()) 
                return;

            if (_inspectedStuff == -1 && _nodes.Count > 0 && "Clear".ClickConfirm(confirmationTag: "DelBook", toolTip: "This will destroy all the nodes. Are you sure?"))
            {
                _nodes.Clear();
                _freeNodeIndex = 0;
                _nodesVersion ++;
            }

            pegi.nl();

            if (_nodes.Count == 0)
            {
                if ("Create ROOT node".Click())
                    new Node(this);
            }
            else
            {
                if ("All Nodes".isEntered(ref _inspectedStuff, 0).nl())
                    _metaData.edit_List(_nodes).nl();

                if ("Tree".isEntered(ref _inspectedStuff, 1).nl()) 
                    _nodes[0].Nested_Inspect();
            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            var tmp = NameForInspector.Replace(KEY_SUFFIX, ""); //name;

            if (pegi.editDelayed(ref tmp))
                NameForInspector = tmp + KEY_SUFFIX;
            
            if (icon.Enter.Click())
                edited = ind;

            this.ClickHighlight();

        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(ConfigBook))] internal class NodeBookDrawer : PEGI_Inspector_Override { }

}
