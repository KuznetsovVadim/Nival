using Assets.Scripts.Helpers;
using System;

namespace Assets.Scripts.Models
{
    public struct PointModel: IEquatable<PointModel>
    {
        public int X { get; private set; }
        public int Z { get; private set; }

        public bool Blocked { get; private set; }

        public int StepNumber { get; private set; }

        public Orientation Orientation { get; private set; }

        public PointModel(int X, int Z, int Side = 0, bool Blocked = false)
        {
            this.X = X;
            this.Z = Z;
            this.Blocked = Blocked;
            Orientation = Orientation.None;
            StepNumber = 0;

            if(Side != 0)
            {
                Orientation = SetOrientation(Side);
            }
        }

        public void SetStep(int StepNumber)
        {
            this.StepNumber = StepNumber;
        }

        public bool Equals(PointModel other)
        {
            if (other.X == X & other.Z == Z) return true;
            return false;
        }

        private Orientation SetOrientation(int Side)
        {
            if ((X > 0 & X < Side) & (Z > 0 & Z < Side))
            {
                return Orientation.Center;
            }
            else
            {
                if (X == 0 & (Z > 0 & Z < Side))
                {
                    return Orientation.LeftSide;
                }
                if (X == Side & (Z > 0 & Z < Side))
                {
                    return Orientation.RightSide;
                }
                if (Z == 0 & (X > 0 & X < Side))
                {
                    return Orientation.BottonSide;
                }
                if (Z == Side & (X > 0 & X < Side))
                {
                    return Orientation.TopSide;
                }
                if (X == Side & Z == Side)
                {
                    return Orientation.TopRightCorner;
                }
                if (X == 0 & Z == Side)
                {
                    return Orientation.TopLeftCorner;
                }
                if (X == 0 & Z == 0)
                {
                    return Orientation.DownLeftCorner;
                }
                if (X == Side & Z == 0)
                {
                    return Orientation.DownRightCorner;
                }
            }
            return Orientation.None;
        }
    }
}
