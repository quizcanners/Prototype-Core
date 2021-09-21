using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

    public partial class ConfigBookScriptableObject
    {
        [Serializable]
        public class BookReference : SmartStringIdGeneric<ConfigBookScriptableObject>,  IPEGI
        {
            public bool IsReferenceOf(ConfigBookScriptableObject book) => book.NameForInspector.Equals(Id);
            protected override SerializableDictionary<string, ConfigBookScriptableObject> GetEnities() => Service.Get<ConfigNodesService>().books;
            public BookReference(ConfigBookScriptableObject book) 
            {
                if (book!= null)
                    Id = book.NameForInspector;
            }

            public BookReference() { }
        }

        public partial class Node 
        {
            [Serializable]
            public class Reference : IPEGI, IPEGI_ListInspect, IGotReadOnlyName, INeedAttention
            {
                [SerializeField] private BookReference _book;
                [SerializeField] public int NodeIndex = -1;

                [NonSerialized] private NodesChain _cachedChain;

                public ConfigBookScriptableObject Book 
                {
                    get => _book.GetEntity();
                    set 
                    {
                        _book = new BookReference(value);
                    }
                }

                public NodesChain GenerateNodeChain()
                {
                    if (_cachedChain == null)
                        _cachedChain = Service.Get<ConfigNodesService>()[this];
                    
                    return _cachedChain;
                }

                public bool IsReferenceTo(Node node) => node != null && node._index == NodeIndex;
                public bool SameAs(Reference reff) => reff._book.SameAs(_book) && reff.NodeIndex == NodeIndex;
                public ConfigBookScriptableObject GetBook() => _book.GetEntity();

                #region Inspector

                private int _inspectedStuff = -1;
                public void Inspect()
                {
                    var book = GetBook();

                    if (book == null || "Book ({0})".F(_book.GetNameForInspector()).isEntered(ref _inspectedStuff, 0).nl())
                        _book.Inspect();
                    
                    var chain = GenerateNodeChain();

                    if ("Node ({0})".F(chain.GetNameForInspector()).isEntered(ref _inspectedStuff, 1))
                    {
                        pegi.nl();
                        if (book != null)
                            "Node".select_iGotIndex(ref NodeIndex, book.GetAllNodes());
                                
                        if (chain != null)
                            chain.Nested_Inspect();
                    }

                    pegi.nl();
                }

                public void InspectInList(ref int edited, int ind)
                {
                    var book = _book.GetEntity();

                    if (!book)
                        _book.InspectInList(ref edited, ind);
                    else 
                    {
                        "Node".select_iGotIndex(60, ref NodeIndex, book.GetAllNodes());

                        if (icon.Enter.Click())
                            edited = ind;
                    }   
                }

                public string GetNameForInspector()
                {
                    var n = GenerateNodeChain();

                    if (n != null)
                        return n.GetNameForInspector();

                    var b = GetBook();
                    if (b)
                        return "{0}-> ???".F(b.GetNameForInspector());

                    return "NO BOOK";
                }

                public string NeedAttention()
                {
                    var b = _book.NeedAttention();

                    if (b.IsNullOrEmpty() == false)
                        return b;

                    if (GenerateNodeChain().LastNode == null)
                        return "Node {0} not found".F(NodeIndex);

                    return null;
                }
                #endregion


                public Reference(Node node, ConfigBookScriptableObject book)
                {
                    NodeIndex = node.IndexForInspector;
                    _book = new BookReference(book);
                }
                public Reference(ConfigBookScriptableObject book)
                {
                    NodeIndex = -1;
                    _book = new BookReference(book);
                }

                public Reference()
                {
                    _book = new BookReference();
                }
            }
        }
    }
}

