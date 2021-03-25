using System;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public struct FeetDistance
    {
        public const string SUFFIX = "ft.";

        [SerializeField] public int ft;

        public int Cells
        {
            get => ft / CellDistance.FEET_PER_CELL;
            set => ft = value * CellDistance.FEET_PER_CELL;
        }

        public static FeetDistance operator +(FeetDistance feet, FeetDistance toAdd)
        {
            feet.ft += toAdd.ft;
            return feet;
        }

        public static FeetDistance operator +(FeetDistance feet, CellDistance cells)
        {
            feet += cells.TotalFeet;
            return feet;
        }

        public override string ToString() => "{0} {1}".F(ft, SUFFIX);
    }

    [Serializable]
    public struct CellDistance : IPEGI_ListInspect
    {
        public const int FEET_PER_CELL = 5;

        [SerializeField] private bool _diagonalHalf;
        [SerializeField] private int _nonDIagonal;

        public int TotalCells => _nonDIagonal;

        public FeetDistance TotalFeet => new FeetDistance() { ft = TotalCells * FEET_PER_CELL };

        public void AddCells(int count = 1, bool diagonal = false)
        {
            if (!diagonal)
            {
                _nonDIagonal += count;
                return;
            }

            _nonDIagonal += (count / 2) * 2;

            if (count % 2 > 0)
            {

                if (_diagonalHalf)
                {
                    _nonDIagonal += 1;
                    _diagonalHalf = false;
                }
                else
                {
                    _diagonalHalf = true;
                }
            }
        }

        private void AddFloorToLowest(FeetDistance feet)
        {
            _nonDIagonal += feet.ft / FEET_PER_CELL;
        }

        public static CellDistance operator +(CellDistance cell, FeetDistance feet)
        {
            cell.AddFloorToLowest(feet);
            return cell;
        }

        public static CellDistance FromCells(int count)
        {
            var cd = new CellDistance();
            cd._nonDIagonal += count;
            return cd;
        }

        public CellDistance Half()
        {
            _nonDIagonal /= 2;
            _diagonalHalf = false;
            return this;
        }

        public static CellDistance FromFeet(FeetDistance dist) => new CellDistance() + dist;
        
        public override string ToString() => TotalFeet.ToString();

        public void InspectInList(ref int edited, int ind)
        {
            "Cells".edit(50, ref _nonDIagonal);
            "Feet: {0}".F(ToString()).write();
        }
    }
}
