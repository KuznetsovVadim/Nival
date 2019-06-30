using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Управляет состоянием игрового поля
    /// </summary>
    public class Field
    {
        public FieldPoint[,] FieldMatrix { get; private set; }

        public List<FieldPoint> MarkedCells { get; private set; }
        
        public event FieldChanged FieldChanged;

        /// <summary>
        /// Создать экземпляр класса управляющего состоянием игрового поля
        /// </summary>
        /// <param name="Field">Массив клеток игрового поля</param>
        public Field(Cell[,] Field)
        {
            CreateMatrix(Field);

            MarkedCells = new List<FieldPoint>(Field.Length);

            foreach (var Cell in Field)
            {
                Cell.OnClickEvent += CellChanged;
            }
        }
        
        /// <summary>
        /// Метод учета изменений в ячейке игрового поля
        /// </summary>
        /// <param name="Cell">Ячейка в которой произошли изменения</param>
        /// <param name="Changies">Вид изменений</param>
        private void CellChanged(Cell Cell, CellChangies Changies)
        {
            switch (Changies)
            {
                case CellChangies.OneWay:

                    if(Cell.Marked)
                    {
                        MarkedCells.Add(FieldMatrix[Cell.X, Cell.Z].Point(false, true));
                        FieldChanged?.Invoke(null, MarkedCells);
                    }
                    else if(Cell.Blocked)
                    {
                        ChangeMatrix(FieldMatrix[Cell.X, Cell.Z].Point(true, false));
                        FieldChanged?.Invoke(FieldMatrix, null);
                    }
                    else
                    {
                        ChangeMatrix(FieldMatrix[Cell.X, Cell.Z].Point(false, false));
                        if(MarkedCells.Contains(FieldMatrix[Cell.X, Cell.Z]))
                        {
                            MarkedCells.Remove(FieldMatrix[Cell.X, Cell.Z]);
                            
                        }
                        FieldChanged?.Invoke(FieldMatrix, MarkedCells);
                    }

                    break;

                case CellChangies.TwoWays:

                    if(Cell.Marked)
                    {
                        MarkedCells.Add(FieldMatrix[Cell.X, Cell.Z].Point(false, true));
                        ChangeMatrix(FieldMatrix[Cell.X, Cell.Z]);

                        FieldChanged?.Invoke(FieldMatrix, MarkedCells);
                    }
                    else
                    {
                        MarkedCells.Remove(FieldMatrix[Cell.X, Cell.Z].Point(true, false));
                        ChangeMatrix(FieldMatrix[Cell.X, Cell.Z]);

                        FieldChanged?.Invoke(FieldMatrix, MarkedCells);
                    }

                    break;
            }
        }
        
        /// <summary>
        /// Метод изменения матрицы игрового поля
        /// </summary>
        /// <param name="Model"></param>
        private void ChangeMatrix(FieldPoint Model)
        {
            FieldMatrix[(int)Model.Position.x, (int)Model.Position.z] = Model;
        }

        /// <summary>
        /// Метод создания матрицы игрового поля
        /// </summary>
        private void CreateMatrix(Cell[,] Field)
        {
            int Side = (byte)Field.GetLength(0);

            FieldMatrix = new FieldPoint[Side, Side];

            for (int i = 0; i < Side; i++)
            {
                for (int j = 0; j < Side; j++)
                {
                    FieldMatrix[i, j] = Field[i, j].CreatePoint(Side-1);
                }
            }
        }
    }
}
