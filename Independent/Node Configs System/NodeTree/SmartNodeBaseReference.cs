using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

    public partial class ConfigBook
    {
        [Serializable]
        public class BookReference : SmartStringIdGeneric<ConfigBook>,  IPEGI
        {
            public bool IsReferenceOf(ConfigBook book) => book.NameForInspector.Equals(Id);
            protected override SerializableDictionary<string, ConfigBook> GetEnities() => Service.Get<ConfigNodesService>().books;
            public BookReference(ConfigBook book) 
            {
                if (book!= null)
                    Id = book.NameForInspector;
            }

            public BookReference() { }
        }

        public partial class Node 
        {
            [Serializable]
            public class FullReference : IPEGI, IPEGI_ListInspect, IGotReadOnlyName, INeedAttention
            {
                [SerializeField] private BookReference _book;
                [SerializeField] private int _nodeIndex = -1;
                [SerializeField] private int _nodeVersion = -1;

                public ConfigBook Book 
                {
                    get => _book.GetEntity();
                    set 
                    {
                        _book = new BookReference(value);
                    }
                }

                public Node Node 
                {
                    get => Service.Get<ConfigNodesService>()[this];//GetNode();
                    set
                    {
                        if (value == null) 
                        {
                            _book = new BookReference();
                            _nodeIndex = -1;
                            _nodeVersion = -1;
                        } else 
                        {
                            _nodeIndex = value.IndexForInspector;
                        }
                    }
                }

                public int NodeIndex => _nodeIndex;
                public bool IsReferenceTo(Node node) => node != null && node._index == _nodeIndex;
                public bool SameAs(FullReference reff) => reff._book.SameAs(_book) && reff._nodeIndex == _nodeIndex && reff._nodeVersion == _nodeVersion;
                public ConfigBook GetBook() => _book.GetEntity();

                #region Inspector

                private int _inspectedStuff = -1;
                public void Inspect()
                {
                    var book = GetBook();

                    if (book == null || "Book ({0})".F(_book.GetNameForInspector()).isEntered(ref _inspectedStuff, 0).nl())
                        _book.Inspect();
                    
                    if ("Node ({0})".F(Node.GetNameForInspector()).isEntered(ref _inspectedStuff, 1))
                    {
                        pegi.nl();
                       /* if (book != null)
                        {
                            if ("Node".select_Index(ref _nodeIndex, book._nodes))
                                Node = book._nodes[_nodeIndex];
                        }*/

                        if (Node != null)
                            Node.Nested_Inspect();
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
                        if (Node != null)
                            Node.InspectInList(ref edited, ind);
                        else
                        {
                           /* if ("Node".select_Index(60, ref _nodeIndex, book._nodes))
                                Node = book._nodes[_nodeIndex];*/

                            if (icon.Enter.Click())
                                edited = ind;
                        }
                    }   
                }

                public string GetNameForInspector()
                {
                    var n = Node;

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

                    if (Node == null)
                        return "Node {0} not found".F(_nodeIndex);

                    return null;
                }
                #endregion



                public FullReference(Node node)
                {
                    Node = node;
                }
                public FullReference(ConfigBook book)
                {
                    _nodeIndex = -1;
                    _book = new BookReference(book);
                }

                public FullReference()
                {
                    _book = new BookReference();
                }
            }
        }
    }
}

