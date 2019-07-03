using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Управляет поведением юнита
    /// </summary>
    public class Unit
    {
        /// <summary>
        /// Название юнита
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Ссылка на игровой объект юнита
        /// </summary>
        public GameObject UnitObject { get; private set; }

        /// <summary>
        /// Матрица игрового поля
        /// </summary>
        private FieldPoint[,] fieldMatrix;

        /// <summary>
        /// Отмеченная ячека на игровом поле
        /// </summary>
        public FieldPoint MarkedCell { get; private set; }

        /// <summary>
        /// Списко отмеченных точек
        /// </summary>
        private List<FieldPoint> markedCells;

        #region Поиск пути и положение

        /// <summary>
        /// Конечная ячейка назначения юнита на игровом поле
        /// </summary>
        private FieldPoint targetCell;

        /// <summary>
        /// Массив ячеек пути юнита до точки назначения
        /// </summary>
        private FieldPoint[] wayPoints;

        /// <summary>
        /// Текущая ячека, в которой находится юнит
        /// </summary>
        private FieldPoint currentCell;

        /// <summary>
        /// Текущая позиция юнита
        /// </summary>
        private Vector3 currentPosition;

        /// <summary>
        /// Следующая ячейка по пути маршрута юнита
        /// </summary>
        private FieldPoint nextCell;

        /// <summary>
        /// Текущее направление юнита
        /// </summary>
        private Vector3 currentDirection;

        /// <summary>
        /// Временная точка
        /// </summary>
        private FieldPoint temp;

        #endregion
        
        private int counter;

        public bool Roam { get; private set; } = true;

        public bool Follow { get; private set; } = true;

        private bool readyToMove = false;

        private bool cellToMove = false;

        private bool deadEnd = false;

        public bool CanMove { get; private set; } = false;

        private bool MarkedPointReached = false;

        private bool RandomMove = false;

        /// <summary>
        /// Создает модель юнита
        /// </summary>
        /// <param name="Name">Название юнита</param>
        /// <param name="UnitObject">Объект представляющий юнита на сцене</param>
        public Unit(string Name, GameObject UnitObject)
        {
            this.Name = Name;
            this.UnitObject = UnitObject;
            markedCells = new List<FieldPoint>();
        }

        /// <summary>
        /// Получает стартовые данные для юнита
        /// </summary>
        /// <param name="FieldMatrix">Матрица игрового поля</param>
        public void GetStartData(FieldPoint[,] FieldMatrix)
        {
            fieldMatrix = FieldMatrix;

            currentCell = FieldMatrix[(int)UnitObject.transform.position.x, (int)UnitObject.transform.position.z];

            currentPosition = currentCell.Position;

            Wave();
        }

        /// <summary>
        /// Получает обновленную матрицу игрового поля и ячейку, в которой произошло последнее изменение
        /// </summary>
        /// <param name="ChangedCell">Ячейка с изменениями</param>
        /// <param name="FieldMatrix">Матрица поля</param>
        public void GetMatrixUpdate(FieldPoint[,] FieldMatrix)
        {
            fieldMatrix = FieldMatrix;
            deadEnd = false;
        }

        /// <summary>
        /// Метод получения отмеченной точки на карте
        /// </summary>
        /// <param name="MarkedPoint">Отмеченная на карте точка</param>
        public void GetMarkedPointsUpdate(List<FieldPoint> markedCells)
        {
            this.markedCells = markedCells;
            RandomMove = false;
            if (!Roam)
            {
                if (currentCell == MarkedCell & markedCells.Contains(currentCell))
                {
                    markedCells.Remove(MarkedCell);
                    nextCell = currentCell;
                    CanMove = true;
                    return;
                }
                CanMove = false;
                MarkedPointReached = false;
            }
        }

        /// <summary>
        /// Переключение состояния юнита
        /// </summary>
        public void SwitchBehavior()
        {
            Roam = !Roam;
            CanMove = false;
            MarkedPointReached = false;
            RandomMove = false;
        }

        public void Update()
        {
            if (deadEnd) return;
            if (CanMove)
            {
                UpdateCurrentPosition();
                CellCheck();
                GetNextCellToMove();
            }
            else
            {
                Wave();
            }
        }

        public void LateUpdate(float Time)
        {
            if (!readyToMove) return;
            Move(Time);
        }

        /// <summary>
        /// Обновляем текущее положение юнита
        /// </summary>
        private void UpdateCurrentPosition()
        {
            currentPosition = UnitObject.transform.position;
        }

        /// <summary>
        /// Проверка следующей точки
        /// </summary>
        private void CellCheck()
        {
            switch (Roam)
            {
                case true:

                    if(nextCell.Blocked)
                    {
                        CanMove = false;
                        readyToMove = false;
                        return;
                    }
                    readyToMove = true;

                    break;

                case false:

                    if (MarkedPointReached & currentCell.Marked) return;

                    if (RandomMove)
                    {
                        if(nextCell.Blocked)
                        {
                            CanMove = false;
                            readyToMove = false;
                            return;
                        }
                        readyToMove = true;
                    }
                    else
                    {
                        if(nextCell.Blocked || !markedCells.Contains(targetCell) || !WaveAlgorithm.CanReach(fieldMatrix, currentCell, targetCell))
                        {
                            CanMove = false;
                            readyToMove = false;
                            return;
                        }
                        readyToMove = true;
                        
                    }
                    break;
            }
        }

        /// <summary>
        /// Получаем следующую точку назначения
        /// </summary>
        private void GetNextCellToMove()
        {
            if(cellToMove || !CanMove) return;
            else
            {
                if(counter < wayPoints.Length && !wayPoints[counter].Blocked)
                {
                    nextCell = wayPoints[counter];
                    cellToMove = true;
                    counter++;
                }
                else
                {
                    CanMove = false;
                }
            }
        }

        /// <summary>
        /// Получение направления движения
        /// </summary>
        private void GetDirection()
        {
            currentDirection = nextCell.Position - currentPosition;
        }

        /// <summary>
        /// Перемещение персонажа
        /// </summary>
        /// <param name="Time">Время</param>
        private void Move(float Time)
        {
            GetDirection();

            if (Vector3.Distance(currentPosition, nextCell.Position) > 0.05f)
            {
                UnitObject.transform.Translate(currentDirection.normalized * (1.5f * Time), Space.World);
            }
            else
            {
                currentCell = nextCell;
                UnitObject.transform.position = nextCell.Position;
                if(currentCell.Equals(targetCell))
                {
                    if(Roam)
                    {
                        CanMove = false;
                    }
                    else
                    {
                        if (currentCell.Marked & MarkedPointReached & !markedCells.Contains(currentCell)) return;
                        else
                        {
                            if(markedCells.Contains(currentCell) & currentCell.Marked & !MarkedPointReached)
                            {
                                markedCells.Remove(currentCell);
                                MarkedPointReached = true;
                                return;
                            }
                            else
                            {
                                CanMove = false;
                                MarkedPointReached = false;
                            }
                        }
                        
                    }
                }
                cellToMove = false;
            }
            readyToMove = false;
        }
        
        /// <summary>
        /// Запускает волну для поиска пути
        /// </summary>
        private void Wave()
        {
            if(deadEnd) return;
            switch(Roam)
            {
                case true:

                    wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, UnitState.Roam);

                    break;

                case false:
                    
                    if(markedCells.Count == 0)
                    {
                        RandomMove = true;
                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, UnitState.FirstFree);
                        break;
                    }

                    if (RandomMove)
                    {
                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, UnitState.FirstFree);
                        break;
                    }

                    wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, UnitState.Follow, markedCells);

                    break;
            }
            
            if(wayPoints == null)
            {
                deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                if (deadEnd) return;
                RandomMove = true;
                return;
            }
            else
            {
                switch (Roam)
                {
                    case true:

                        targetCell = wayPoints[wayPoints.Length - 1];
                        cellToMove = false;
                        counter = 0;
                        CanMove = true;
                        GetNextCellToMove();
                        break;

                    case false:

                        targetCell = wayPoints[wayPoints.Length - 1];
                        MarkedCell = targetCell;
                        cellToMove = false;
                        counter = 0;
                        CanMove = true;
                        GetNextCellToMove();
                        break;
                }
            }
        }
    }
}
    
