using QuizCanners.Inspect;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Utils;
using QuizCanners.Migration;

namespace QuizCanners.IsItGame.NodeNotes
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/Node Configs/" + FILE_NAME)]
    public partial class ConfigBook : ScriptableObject, IPEGI, IPEGI_ListInspect, IGotName, ICfg
    {
        public const string FILE_NAME = "Node Book";
        public const string KEY_SUFFIX = ".key";

        [SerializeField] private int _freeNodeIndex;
        
        private Node _rootNode = new Node();

        public Node this[Node.FullReference reff]
        {
            get
            {
                /*var n = _nodes.TryGet(reff.NodeIndex);
                return reff.IsReferenceTo(n) ? n : null;*/
                return null;
            }
        }

        #region Encode & Decode 

        public CfgEncoder Encode()
        {
            var cfg = new CfgEncoder();

            return cfg;
        }

        public void Decode(string key, CfgData data)
        {

        }

        #endregion

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

            if (_inspectedStuff == -1 && "Clear".ClickConfirm(confirmationTag: "DelBook", toolTip: "This will destroy all the nodes. Are you sure?"))
            {
                _rootNode = new Node();
                _freeNodeIndex = 0;
            }

            pegi.nl();
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
