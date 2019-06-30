using Assets.Scripts.Models;
using System;
using System.Collections;
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
        private FieldPoint[,] FieldMatrix;

        /// <summary>
        /// Позиция в отмеченной ячеке на игровом поле
        /// </summary>
        private Vector3 MarkedPosition;

        /// <summary>
        /// Отмеченная ячека на игровом поле
        /// </summary>
        private FieldPoint MarkedCell;

        public event GetReserve GetReserveMarkedPoint;

        #region Поиск пути и положение

        /// <summary>
        /// Конечная ячейка назначения юнита на игровом поле
        /// </summary>
        public FieldPoint TargetCell { get; private set; }

        /// <summary>
        /// Массив ячеек пути юнита до точки назначения
        /// </summary>
        private FieldPoint[] WayPoints;

        /// <summary>
        /// Текущая ячека, в которой находится юнит
        /// </summary>
        private FieldPoint CurrentCell;

        /// <summary>
        /// Текущая позиция юнита
        /// </summary>
        private Vector3 CurrentPosition;

        /// <summary>
        /// Ячейка игрового поля, в которой произошли последние изменения
        /// </summary>
        private FieldPoint LastChangedCell;

        /// <summary>
        /// Следующая ячейка по пути маршрута юнита
        /// </summary>
        private FieldPoint NextCell;

        /// <summary>
        /// Текущее направление юнита
        /// </summary>
        private Vector3 CurrentDirection;

        /// <summary>
        /// Временная точка
        /// </summary>
        private FieldPoint Temp;

        #endregion

        private int FieldSide;

        private int Counter;
        
        private bool Roam = true;

        private bool ReadyToMove = false;

        private bool CellToMove = false;

        private bool DeadEnd = false;

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
        }

        /// <summary>
        /// Получает стартовые данные для юнита
        /// </summary>
        /// <param name="FieldMatrix">Матрица игрового поля</param>
        public void GetStartData(FieldPoint[,] FieldMatrix)
        {
            this.FieldMatrix = FieldMatrix;

            CurrentCell = FieldMatrix[(int)UnitObject.transform.position.x, (int)UnitObject.transform.position.z];

            CurrentPosition = CurrentCell.Position;

            Counter = 0;

            FieldSide = FieldMatrix.GetLength(0);
            
            FirstWave();
        }

        /// <summary>
        /// Получает обновленную матрицу игрового поля и ячейку, в которой произошло последнее изменение
        /// </summary>
        /// <param name="ChangedCell">Ячейка с изменениями</param>
        /// <param name="FieldMatrix">Матрица поля</param>
        public void GetMatrixUpdate(FieldPoint[,] FieldMatrix)
        {
            this.FieldMatrix = FieldMatrix;
            DeadEnd = false;
        }

        /// <summary>
        /// Метод получения отмеченной точки на карте
        /// </summary>
        /// <param name="MarkedPoint">Отмеченная на карте точка</param>
        public void GetMarkedPoint(FieldPoint MarkedCell)
        {
            this.MarkedCell = MarkedCell;

            if(!Roam & (MarkedCell !=null && !MarkedCell.Equals(TargetCell)))
            {
                CanMove = false;
                ReadyToMove = false;
            }
        }

        /// <summary>
        /// Переключение состояния юнита
        /// </summary>
        public void SwitchBehavior()
        {
            Roam = !Roam;
            CanMove = false;
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
            if (!ReadyToMove) return;
            GetDirection();
            Move(Time);
        }

        /// <summary>
        /// Проверяем ближайшую точку маршрута
        /// </summary>
        private void CheckNextPoint()
        {
            if(NextCell.Blocked)
            {
                CanMove = false;
            }
        }

        /// <summary>
        /// Обновляем текущее положение юнита
        /// </summary>
        private void UpdateCurrentPosition()
        {
            CurrentPosition = UnitObject.transform.position;
        }

        /// <summary>
        /// Получаем следующую точку назначения
        /// </summary>
        private void GetNextCellToMove()
        {
            if(CellToMove) return;
            if (WayPoints != null && Counter <= WayPoints.Length - 1)
            {
                if(WayPoints[Counter].Blocked)
                {
                    CanMove = false;
                    ReadyToMove = false;
                    
                    if(WayPoints[Counter].Equals(TargetCell))
                    {
                        TargetCell = null;
                    }
                    else
                    {
                        NextCell = WayPoints[Counter];
                    }
                    
                }
                else
                {
                    NextCell = WayPoints[Counter];
                    Counter++;
                    CellToMove = true;
                    ReadyToMove = true;
                }
            }
            else
            {
                CanMove = false;
                ReadyToMove = false;
            }
        }

        /// <summary>
        /// Получение направления движения
        /// </summary>
        private void GetDirection()
        {
            CurrentDirection = NextCell.Position - CurrentPosition;
        }

        /// <summary>
        /// Перемещение персонажа
        /// </summary>
        /// <param name="Time">Время</param>
        private void Move(float Time)
        {
            if (Vector3.Distance(CurrentPosition, NextCell.Position) > 0.05f)
            {
                UnitObject.transform.Translate(CurrentDirection.normalized * (1.50f * Time), Space.World);
            }
            else
            {
                UnitObject.transform.position = NextCell.Position;
                CurrentCell = NextCell;
                if(CurrentCell.Equals(TargetCell))
                {
                    CanMove = false;
                    Counter = 0;
                    TargetCell = null;

                }
                CellToMove = false;
            }
        }

        /// <summary>
        /// Получение случайной точке на игровом поле, в качестве точки назначения
        /// </summary>
        private void GetRandomPoint(RandomMode RandomMode)
        {
            Temp = FieldMatrix[Random.Range(0, FieldSide), Random.Range(0, FieldSide)];

            switch (RandomMode)
            {
                case RandomMode.All:

                    if (Temp.Equals(CurrentCell) | Temp.Blocked)
                    {
                        GetRandomPoint(RandomMode);
                    }
                    TargetCell = Temp;

                    break;

                case RandomMode.NoMarked:

                    if (Temp.Equals(CurrentCell) | Temp.Blocked | Temp.Marked)
                    {
                        GetRandomPoint(RandomMode);
                    }
                    TargetCell = Temp;

                    break;
            }
            
        }

        /// <summary>
        /// Запускает волновой алгоритм поиска пути
        /// </summary>
        private void Wave()
        {
            if(DeadEnd)return;

            switch(Roam)
            {
                case true:

                    if (TargetCell == null)
                    {
                        GetRandomPoint(RandomMode.All);

                        WayPoints = WaveAlgorithm.ShortWay(FieldMatrix, CurrentCell, TargetCell);

                        if(WayPoints == null)
                        {
                            DeadEnd = WaveAlgorithm.NoWay(FieldMatrix, CurrentCell);
                            if(DeadEnd)
                            {
                                NextCell = CurrentCell;
                                TargetCell = null;
                                return;
                            }
                            GetRandomPoint(RandomMode.All);
                            Wave();
                        }

                        CellToMove = false;
                        Counter = 0;

                        GetNextCellToMove();

                        CanMove = true;

                        break;
                    }

                    if (NextCell.Blocked | WayPoints == null)
                    {
                        WayPoints = WaveAlgorithm.ShortWay(FieldMatrix, CurrentCell, TargetCell);

                        if(WayPoints == null)
                        {
                            DeadEnd = WaveAlgorithm.NoWay(FieldMatrix, CurrentCell);
                            if(DeadEnd)
                            {
                                NextCell = CurrentCell;
                                TargetCell = null;
                                return;
                            }
                            GetRandomPoint(RandomMode.All);
                            Wave();
                        }

                        CellToMove = false;
                        Counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }

                    CanMove = true;

                    break;

                case false:

                    if(MarkedCell == null)
                    {
                        GetRandomPoint(RandomMode.NoMarked);

                        WayPoints = WaveAlgorithm.ShortWay(FieldMatrix, CurrentCell, TargetCell);

                        if (WayPoints == null)
                        {
                            DeadEnd = WaveAlgorithm.NoWay(FieldMatrix, CurrentCell);
                            if (DeadEnd) return;
                        }

                        CellToMove = false;
                        Counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }

                    else
                    {
                        TargetCell = MarkedCell;

                        WayPoints = WaveAlgorithm.ShortWay(FieldMatrix, CurrentCell, TargetCell);

                        if(WayPoints == null)
                        {
                            DeadEnd = WaveAlgorithm.NoWay(FieldMatrix, CurrentCell);
                            if (DeadEnd) return;
                            MarkedCell = GetReserveMarkedPoint?.Invoke();
                            if (MarkedCell == null) return;
                            Wave();
                        }

                        CellToMove = false;
                        Counter = 0;

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
            GetRandomPoint(RandomMode.All);

            WayPoints = WaveAlgorithm.ShortWay(FieldMatrix, CurrentCell, TargetCell);

            GetNextCellToMove();
        }
    }
}
    
