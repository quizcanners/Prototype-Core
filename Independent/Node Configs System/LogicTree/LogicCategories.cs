using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.NodeNotes
{

    public interface ICategorized  {
        List<PickedCategory> MyCategories { get; set; }
    } 

    public class CategoryRoot<T> : ICfgCustom, IPEGI where T : ICategorized {

        public Countless<Category<T>> allSubs = new Countless<Category<T>>();

        public int unusedIndex;

        public List<Category<T>> subCategories = new List<Category<T>>();

        #region Inspect

 
        private int inspected = -1;
        
        public bool SelectCategory(PickedCategory pc)
        {
            var changed = pegi.ChangeTrackStart();

            var categoryFound = false;

            if (pc.path.Count > 0)
            {
                var ind = pc.path[0];

                foreach (var t in subCategories)
                    if (t.IndexForInspector == ind) {
                        categoryFound = true;
                        t.SelectCategory(pc, 1);
                        break;
                    }
            }

            if (categoryFound) return changed;

            int tmp = -1;
            if ("Category".select_Index(ref tmp, subCategories)) {
                var c = subCategories.TryGet(tmp);
                if (c!= null)
                    pc.path.ForceSet(0,c.IndexForInspector);
            }


            return changed;
        }
        
        public virtual void Inspect()
        {
            current = this;

            "Categories".edit_List(subCategories, ref inspected).nl();

            current = null;
        }
        #endregion

        #region Encode & Decode
        public static CategoryRoot<T> current = new CategoryRoot<T>();

       // public void Decode(CfgData data) => this.DecodeTagsFrom(data);

        public virtual CfgEncoder Encode()
        {
            current = this;

            var cody = new CfgEncoder()//this.EncodeUnrecognized()
                .Add("s", subCategories)
                .Add_IfNotNegative("i", inspected)
                .Add("fi", unusedIndex);

            current = null;

            return cody;
        }

        public virtual void Decode(CfgData data)
        {
            current = this;
            this.DecodeTagsFrom(data);
            current = null;
        }

        public virtual void Decode(string tg, CfgData data) {

            switch (tg) {
                case "s": data.ToList(out subCategories); break;
                case "i": inspected = data.ToInt(); break;
                case "fi": unusedIndex = data.ToInt(); break;
            }
        }
        #endregion
    }

    public class Category<T> : ICfgCustom, IGotName, IGotIndex, IPEGI where T: ICategorized
    {

        public string NameForInspector { get; set; }
        public int IndexForInspector { get; set; }
        public List<Category<T>> subCategories = new List<Category<T>>();
        public List<T> elements = new List<T>();

        public Category() {
            var r = CategoryRoot<T>.current;

            r.allSubs[r.unusedIndex] = this;
            IndexForInspector = r.unusedIndex;
            r.unusedIndex++;
        }

        #region Inspect

        private int _inspected = -1;
        
        public bool Select(ref T val) => pegi.select(ref val, elements);
        
        public virtual void Inspect() {

            if (_inspected == -1) {
                var n = NameForInspector;
                if ("Name".edit(ref n).nl())
                    NameForInspector = n;
            }

            "Sub".edit_List(subCategories, ref _inspected).nl();
        }

        public bool SelectCategory(PickedCategory pc, int depth)
        {

            var changed = pegi.ChangeTrackStart();

            var categoryFound = false;

            if (pc.path.Count > depth) {

                NameForInspector.nl();

                if (pc.path.Count == depth + 1 && icon.Exit.Click())
                    pc.path.RemoveLast();
                else {
                    var ind = pc.path[depth];

                    foreach (var t in subCategories)
                        if (t.IndexForInspector == ind) {
                            categoryFound = true;
                            t.SelectCategory(pc, depth + 1);
                            break;
                        }
                }
            }

            if (categoryFound) return false;

            NameForInspector.write(PEGI_Styles.ClickableText);

            int tmp = -1;
            if ("Sub Category".select_Index(ref tmp, subCategories).nl())
            {
                var c = subCategories.TryGet(tmp);
                if (c != null)
                    pc.path.ForceSet(depth, c.IndexForInspector);
            }

            return changed;
        }

#endregion

#region Encode & Decode

        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("n", NameForInspector)
            .Add("i", IndexForInspector)
            .Add_IfNotEmpty("s", subCategories)
            .Add_IfNotNegative("in",_inspected);

        public virtual void Decode(CfgData data)
        {
            this.DecodeTagsFrom(data);
            CategoryRoot<T>.current.allSubs[IndexForInspector] = this;
        }

        public virtual void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "n": NameForInspector = data.ToString(); break;
                case "s": data.ToList(out subCategories); break;
                case "i":  IndexForInspector = data.ToInt(); break;
                case "in": _inspected = data.ToInt(); break;
            }
        }

#endregion
    }

    public class PickedCategory: ICfg {

        public List<int> path = new List<int>();
        
        public bool Inspect<T>() where T: ICategorized => CategoryRoot<T>.current.SelectCategory(this);

#region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotEmpty("p", path);

        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "p": data.ToList(out path); break;
            }
        }
        
        #endregion


    }

}