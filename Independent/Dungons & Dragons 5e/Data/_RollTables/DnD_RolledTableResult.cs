using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    public class RolledTable
    {
        public class Result : ICfg
        {
            private RollResult _value;
            internal List<Result> SubResultsList = new List<Result>();

            public Result SubResult => SubResultsList.GetOrCreate(0);

            public bool IsRolled;
            public RollResult Roll 
            { 
                get => _value; 
                set 
                { 
                    _value = value;
                    SubResultsList.Clear();
                    IsRolled = true; 
                } 
            }

            #region Encode & Decode
            public CfgEncoder Encode()
            {
                var cody = new CfgEncoder()
                .Add_Bool("r", IsRolled);

                if (IsRolled)
                    cody.Add("v", _value.Value);


                if (SubResultsList.Count > 0) 
                {
                    for (int i = SubResultsList.Count-1; i>=0; i--) 
                    {
                        if (!SubResultsList[i].IsRolled)
                            SubResultsList.RemoveAt(i);
                        else
                            break;
                    }
                }

                if (IsRolled || SubResultsList.Count > 0)
                {
                    cody.Add("p", SubResultsList);
                }
                
                return cody;
            }
            public void Decode(string key, CfgData data)
            {
                switch (key) 
                {
                    case "r": IsRolled = data.ToBool(); break;
                    case "v": _value = RollResult.From(data.ToInt()); break;
                    case "p": data.ToList(out SubResultsList); break;
                }
            }
            #endregion

            public bool TryGetConcept<CT>(out CT value, List<RandomElementsRollTables> tables) where CT: IComparable
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    var t = tables[i];
                    var r = SubResultsList.TryGet(i);

                    if (r != null)
                    {
                        if (t.TryGetConcept(out value, r))
                            return true;
                    }
                }

                value = default(CT);
                return false;

            }

            public IDisposable AddAndUse (RandomElementsRollTables table) => new ResultChainToken(this, table);

            private static List<ResultChainToken> _tablesChain = new List<ResultChainToken>();

            public bool TryGetConceptFromUpperChain<T>(out T value) where T : IComparable
            {
                for (int i = _tablesChain.Count - 1; i >= 0; i--)
                {
                    ResultChainToken chainToken = _tablesChain[i];

                    if (chainToken.Table.TryGetConcept<T>(out value, chainToken.Result))
                    {
                        return true;
                    }
                }

                value = default(T);
                return false;
            }

            private class ResultChainToken : IDisposable
            {
                public Result Result;
                public RandomElementsRollTables Table;

                public void Dispose() => _tablesChain.Remove(this);
                
                public ResultChainToken(Result res, RandomElementsRollTables table) 
                {
                    Result = res;
                    Table = table;

                    _tablesChain.Add(this);
                }
            }
        }

        public struct BranchIndex { public int Index; }

    }
}