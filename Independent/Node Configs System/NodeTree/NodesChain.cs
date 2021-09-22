using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;

namespace QuizCanners.IsItGame.NodeNotes
{

    public partial class ConfigBookScriptableObject
    {
        public class NodesChain : IDisposable, IPEGI, IPEGI_ListInspect, IGotReadOnlyName
        {
            private List<NodeChainToken> _chain = new List<NodeChainToken>();
            public ConfigBookScriptableObject Book { get; private set; }

            public List<NodeChainToken> Nodes
            {
                get
                {
                    if (_version != Book.Version)
                    {
                        _version = Book.Version;
                        _chain.Clear();
                        Book.RepopulateNodesChain(this);
                    }


                    return _chain;
                }
            }

            public NodesChain GetNodeInChain(int index) 
            {
                var sn = _chain.TryGet(index);
                if (sn == null)
                    return null;

                return Book[new Node.Reference(sn.Node, Book)];
            }

            public Node LastNode 
            {
                get
                {
                    if (Nodes.Count == 0)
                        return null;

                    return _chain[_chain.Count - 1].Node;
                }
            }

            private int _version;

          

            public Node.Reference GetReferenceToLastNode()
            {
                var n = LastNode;
                return new Node.Reference(LastNode, Book);
            }

            private bool TryGetConfigFromChain(ITaggedCfg val, out CfgData dta)
            {
                for (int i = _chain.Count - 1; i >= 0; i--)
                    if (_chain[i].Node.TryGetConfig(val, out dta))
                        return true;

                return false;
            }

            public IDisposable AddAndUse(Node node) => new NodeChainToken(node, this);



            public void Dispose()
            {
                Book = null;
            }

            public void Inspect()
            {
                if (LastNode != null)
                {
                    "Chain: {0} el".F(_chain.Count).nl();

                    var oldChain = _chain_ForInspector;
                    _chain_ForInspector = this;

                    LastNode.Inspect(this);

                    _chain_ForInspector = oldChain;
                }
                else
                    "Empty Node Chain".writeWarning();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (LastNode != null)
                    LastNode.InspectInList(ref edited, index);
                else
                    "Empty Node Chain".writeWarning();
            }

            public string GetReadOnlyName()
            {
                if (LastNode != null)
                    return "Chain [{0}] -> {1}".F(_chain.Count, LastNode.GetNameForInspector());
                else
                    return "Empty Chain";
            }

            public class NodeChainToken : IDisposable
            {
                public Node Node;
                private NodesChain _chain;

                public void Dispose() => _chain._chain.Remove(this);

                public NodeChainToken(Node node, NodesChain chain)
                {
                    Node = node;
                    _chain = chain;
                    chain._chain.Add(this);
                }
            }

            public NodesChain(ConfigBookScriptableObject book)
            {
                Book = book;
                _version = book.Version;
            }
        }
    }
}
