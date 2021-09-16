using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public class BigTextEditable : IPEGI
    {
        public string Value;
        [SerializeField] public bool Editing;

        public void Inspect()
        {
            if (Editing)
            {
                pegi.editBig(ref Value);
                if (icon.Done.Click())
                    Editing = false;

                "<- Click when you are done".writeHint();
                pegi.nl();
            }
            else
            {
                if (Value.IsNullOrEmpty())
                {
                    if ("Add Description".Click().nl())
                        Editing = true;
                }
                else
                {
                    if (icon.Edit.Click())
                        Editing = true;
                }
            }
        }

        public string GetRolledElementName(List<RolledTable.Result> results, List<RandomElementsRollTables> tables)
        {
            var arguments = new List<string>();

            for (int i = 0; i < tables.Count; i++)
            {
                var t = tables[i];
                var r = results.TryGet(i);

                if (!t) 
                {
                    arguments.Add("NULL TABLE");
                    continue;
                }

                if (r == null)
                {
                    arguments.Add(" Not rolled");
                    break;
                }
                arguments.Add(t.GetRolledElementName(r));
            }

            if (!Value.IsNullOrEmpty())
            {
                try
                {
                    var res = string.Format(Value, arguments.ToArray());
                    return res;
                }
                catch { }
            }

            var sb = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                var t = arguments[i];
                if (sb.Length > 0)
                    sb.Append(' ');

                sb.Append(t);
            }

            return sb.ToString();
        }
    }
}