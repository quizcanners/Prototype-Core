using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace Dungeons_and_Dragons
{
    public enum Sense
    {
        Blindsight = 0,
        Darkvision = 1,
        Tremorsense = 2,
        Truesight = 3,
    }


    [System.Serializable]
    public class SensesDictionary: SerializableDictionary_ForEnum<Sense, CellDistance>  
    {
        const int DEFAULT_CELL_COUNT = 12;

        public override void Create(Sense key)
        {
            this[key] = CellDistance.FromCells(DEFAULT_CELL_COUNT);
        }

        protected override void InspectElementInList(Sense key, int index)
        {
            var change = pegi.ChangeTrackStart();

            CellDistance value = this.TryGet(key);

            bool gotIt = value.TotalCells > 0;

            string name = key.ToString().SimplifyTypeName();

            if (name.toggleIcon(ref gotIt, hideTextWhenTrue: true))
                value = CellDistance.FromCells(gotIt ? DEFAULT_CELL_COUNT : 0);
            
            if (gotIt)
            {
                name.write(120);
                pegi.Inspect_AsInList_Value(ref value);
            }

            if (change)
            {
                if (value.TotalCells>0)
                    this[key] = value;
                else
                    Remove(key);
            }

        }
    }

}
