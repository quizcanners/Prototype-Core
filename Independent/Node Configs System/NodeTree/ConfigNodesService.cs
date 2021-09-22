using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    [ExecuteAlways]
    public partial class ConfigNodesService : Service.BehaniourBase, IPEGI
    {
        [SerializeField] internal BooksDictionary books = new BooksDictionary();
        [SerializeField] private ConfigBookScriptableObject.Node.Reference _reference;


        public void SetCurrent(ConfigBookScriptableObject.NodesChain chain) => _reference = chain.GetReferenceToLastNode();
        public bool IsCurrent(ConfigBookScriptableObject.Node node) => _reference != null && _reference.IsReferenceTo(node);
        public bool IsCurrent(ConfigBookScriptableObject.Node.Reference reff) => _reference != null && _reference.SameAs(reff);

        public bool AnyEntered
        {
            get
            {
                if (_reference == null)
                    return false;

                var c = _reference.GenerateNodeChain();

                return c != null && c.LastNode != null;
            }
        }

        [Serializable]
        internal class BooksDictionary: SerializableDictionary<string, ConfigBookScriptableObject> 
        {
            protected override CollectionMetaData CollectionMeta
            {
                get
                {
                    if (_collectionMeta == null)
                    {
                        _collectionMeta = new CollectionMetaData(labelName: "Node Books", showAddButton: false) { ElementName = ElementName };
                    }
                    return _collectionMeta;
                }
            }


            public override void Inspect()
            {
                base.Inspect();

                if (!CollectionMeta.IsInspectingElement)
                {
                    ConfigBookScriptableObject tmp = null;
                    if ("Add Book".edit(90, ref tmp).nl() && tmp && ContainsKey(tmp.name) == false)
                        Add(tmp.name, tmp);
                }
            }
        }

        public ConfigBookScriptableObject.NodesChain this[ConfigBookScriptableObject.Node.Reference rff] 
        {
            get {
                var book = rff.GetBook();
                return book ? book[rff] : null;
            }
        }

        #region Inspect

       // private int _inspectedStuff = -1;

        public override void Inspect()
        {
            books.Nested_Inspect();
           // pegi.nl();
           // "All Books".enter_Inspect(books, ref _inspectedStuff, 0).nl();

           // if ("Current".isConditionally_Entered(AnyEntered, ref _inspectedStuff, 1).nl())
             //   _reference.Nested_Inspect();
        }
        #endregion
    }


    [PEGI_Inspector_Override(typeof(ConfigNodesService))] internal class ConfigNodesManagerDrawer : PEGI_Inspector_Override { }


}