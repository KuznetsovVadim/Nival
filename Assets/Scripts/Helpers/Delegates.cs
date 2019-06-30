using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public delegate void CellClicked(Cell Cell, CellChangies Changies);

    public delegate void FieldChanged(FieldPoint[,] FieldMatrix, List<FieldPoint> MarkedCells);

    public delegate void MapChanged(FieldPoint[,] FieldMatrix);

    public delegate void Point(Vector3 MarkedPoint);

    public delegate void UnitAction(bool IsRoam);

    public delegate FieldPoint GetReserve();

    public enum RandomMode
    {
        All,
        NoMarked
    }

    
    public enum MouseClick
    {
        LeftButton,
        RightButton
    }

    public enum CellChangies
    {
        OneWay,
        TwoWays
    }

    public enum WaveMode
    {
        Steps,
        Route
    }

    public enum Orientation
    {
        Center,
        TopSide,
        BottonSide,
        LeftSide,
        RightSide,
        TopRightCorner,
        TopLeftCorner,
        DownRightCorner,
        DownLeftCorner,
        None
    }
}
