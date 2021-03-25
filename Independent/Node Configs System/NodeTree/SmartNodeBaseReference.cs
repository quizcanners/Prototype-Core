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
                Id = book.NameForInspector;
            }
        }

        public partial class Node {

            [Serializable]
            public class Reference : IPEGI, IPEGI_ListInspect, IGotReadOnlyName
            {
                [SerializeField] private BookReference _book;
                [SerializeField] private int _nodeIndex = -1;
                [SerializeField] private int _nodeVersion = -1;

                public int NodeIndex => _nodeIndex;
                public bool IsReferenceTo(Node node) => node != null && node._version == _nodeVersion && node._index == _nodeIndex;
                public bool SameAs(Reference reff) => reff._book.SameAs(_book) && reff._nodeIndex == _nodeIndex && reff._nodeVersion == _nodeVersion;
                public Node GetNode() => Service.Get<ConfigNodesService>()[this];
                public ConfigBook GetBook() => _book.GetEntity();

                #region Inspector
                public void Inspect()
                {
                    var node = GetNode();
                    if (node != null)
                    {
                        node.Nested_Inspect();
                    }
                    else
                    {
                        "No node".writeWarning();
                        _book.Inspect();
                    }
                }
                public void InspectInList(ref int edited, int ind)
                {
                    var book = _book.GetEntity();

                    if (!book)
                        _book.InspectInList(ref edited, ind);
                    else 
                    {
                        var node = GetNode();
                        node.InspectInList(ref edited, ind);
                    }
                }

                public string GetNameForInspector()
                {
                    var n = GetNode();
                    if (n != null)
                        return n.GetNameForInspector();

                    var b = GetBook();
                    if (b)
                        return "{0}-> ???".F(b.GetNameForInspector());

                    return "NO BOOK";
                }
                #endregion

                public Reference(Node node)
                {
                    _nodeIndex = node.IndexForInspector;
                    _nodeVersion = node._version;
                    _book = new BookReference(node.GetBook());
                }
                public Reference(ConfigBook book)
                {
                    _nodeIndex = -1;
                    _nodeVersion = book._nodesVersion;
                    _book = new BookReference(book);
                }
            }
        }
    }
}

