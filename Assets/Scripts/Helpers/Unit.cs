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

        private int fieldSide;

        private int counter;

        public bool Roam { get; private set; } = true;

        private bool readyToMove = false;

        private bool cellToMove = false;

        private bool deadEnd = false;

        private bool noMarkedPointToReach = false;

        public bool CanMove { get; private set; } = true;

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

            counter = 0;

            fieldSide = FieldMatrix.GetLength(0);
            
            FirstWave();
        }

        /// <summary>
        /// Получает обновленную матрицу игрового поля и ячейку, в которой произошло последнее изменение
        /// </summary>
        /// <param name="ChangedCell">Ячейка с изменениями</param>
        /// <param name="FieldMatrix">Матрица поля</param>
        public void GetMatrixUpdate(FieldPoint[,] FieldMatrix)
        {
            fieldMatrix = FieldMatrix;
            noMarkedPointToReach = false;
            deadEnd = false;
        }

        /// <summary>
        /// Проверяет наличие отмеченной точки
        /// </summary>
        /// <returns></returns>
        public bool HasMarkedPoint()
        {
            if(MarkedCell != null && MarkedCell.Marked) return true;
            MarkedCell = null;
            return false;
        }

        /// <summary>
        /// Метод получения отмеченной точки на карте
        /// </summary>
        /// <param name="MarkedPoint">Отмеченная на карте точка</param>
        public void GetMarkedPointsUpdate(List<FieldPoint> markedCells)
        {
            this.markedCells = markedCells;
            noMarkedPointToReach = false;

            if (!Roam & (MarkedCell == null || !MarkedCell.Marked))
            {
                SetMarkedPoint();
            }
        }

        /// <summary>
        /// Переключение состояния юнита
        /// </summary>
        public void SwitchBehavior()
        {
            Roam = !Roam;
            CanMove = false;

            if(!Roam)
            {
                SetMarkedPoint();
            }
            else
            {
                MarkedCell = null;
                noMarkedPointToReach = false;
            }
        }

        /// <summary>
        /// Находим отмеченную точку
        /// </summary>
        private void SetMarkedPoint()
        {
            if(noMarkedPointToReach) return;

            for (int i = 0; i < markedCells.Count; i++)
            {
                if (CanReachPoint(markedCells[i]))
                {
                    MarkedCell = markedCells[i];
                    markedCells.Remove(markedCells[i]);
                    CanMove = false;
                    return;
                }
            }
            MarkedCell = null;
            noMarkedPointToReach = true;
        }

        public void Update()
        {
            UpdateCurrentPosition();

            CheckNextPoint();
            
            if(CanMove)
            {
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
            GetDirection();
            Move(Time);
        }

        /// <summary>
        /// Проверяем ближайшую точку маршрута
        /// </summary>
        private void CheckNextPoint()
        {
            if(nextCell.Blocked)
            {
                CanMove = false;
            }
        }

        /// <summary>
        /// Обновляем текущее положение юнита
        /// </summary>
        private void UpdateCurrentPosition()
        {
            currentPosition = UnitObject.transform.position;
        }

        /// <summary>
        /// Получаем следующую точку назначения
        /// </summary>
        private void GetNextCellToMove()
        {
            if(cellToMove) return;
            if (wayPoints != null && counter <= wayPoints.Length - 1)
            {
                if(wayPoints[counter].Blocked)
                {
                    CanMove = false;
                    readyToMove = false;
                    
                    if(wayPoints[counter].Equals(targetCell))
                    {
                        targetCell = null;
                    }
                    else
                    {
                        nextCell = wayPoints[counter];
                    }
                    
                }
                else
                {
                    nextCell = wayPoints[counter];
                    counter++;
                    cellToMove = true;
                    readyToMove = true;
                }
            }
            else
            {
                CanMove = false;
                readyToMove = false;
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
            if (Vector3.Distance(currentPosition, nextCell.Position) > 0.05f)
            {
                UnitObject.transform.Translate(currentDirection.normalized * (1.50f * Time), Space.World);
            }
            else
            {
                UnitObject.transform.position = nextCell.Position;
                currentCell = nextCell;
                if(currentCell.Equals(targetCell))
                {
                    CanMove = false;
                    counter = 0;
                    targetCell = null;

                }
                cellToMove = false;
            }
        }
        
        /// <summary>
        /// Получение случайной точке на игровом поле, в качестве точки назначения
        /// </summary>
        private void GetRandomPoint()
        {
            temp = fieldMatrix[Random.Range(0, fieldSide), Random.Range(0, fieldSide)];

            if (temp.Blocked)
            {
                GetRandomPoint();
            }
            targetCell = temp;

        }

        /// <summary>
        /// Запускает волновой алгоритм поиска пути
        /// </summary>
        private void Wave()
        {
            if(deadEnd)return;

            switch(Roam)
            {
                case true:

                    if (targetCell == null)
                    {
                        GetRandomPoint();

                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if(wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if(deadEnd)
                            {
                                nextCell = currentCell;
                                targetCell = null;
                                return;
                            }
                            GetRandomPoint();
                            Wave();
                        }

                        cellToMove = false;
                        counter = 0;

                        GetNextCellToMove();

                        CanMove = true;

                        break;
                    }

                    if (nextCell.Blocked | wayPoints == null)
                    {
                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if(wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if(deadEnd)
                            {
                                nextCell = currentCell;
                                targetCell = null;
                                return;
                            }
                            GetRandomPoint();
                            Wave();
                        }

                        cellToMove = false;
                        counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }

                    CanMove = true;

                    break;

                case false:

                    if(MarkedCell == null)
                    {
                        GetRandomPoint();

                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if (wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if (deadEnd) return;
                            GetRandomPoint();
                            Wave();
                        }

                        cellToMove = false;
                        counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }

                    else
                    {
                        if(!MarkedCell.Marked)
                        {
                            MarkedCell = null;
                            return;
                        }

                        targetCell = MarkedCell;

                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if(wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if (deadEnd) return;
                            if (!noMarkedPointToReach) SetMarkedPoint();
                            if (MarkedCell == null) return;
                            Wave();
                        }

                        cellToMove = false;
                        counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Рассчитывает начальную точку назначения и путь к ней
        /// </summary>
        private void FirstWave()
        {
            GetRandomPoint();

            wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

            GetNextCellToMove();
        }

        /// <summary>
        /// Проверяет возможность дойти до точки
        /// </summary>
        /// <param name="targetCell"></param>
        /// <returns></returns>
        private bool CanReachPoint(FieldPoint targetCell)
        {
            if (!targetCell.Marked) return false;
            return WaveAlgorithm.CanReach(fieldMatrix, currentCell, targetCell);
        }
    }
}
    
