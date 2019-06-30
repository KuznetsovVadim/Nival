using Assets.Scripts.Models;
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
        private FieldPoint markedCell;

        public event GetReserve GetReserveMarkedPoint;

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
        
        private bool roam = true;

        private bool readyToMove = false;

        private bool cellToMove = false;

        private bool deadEnd = false;

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
            this.fieldMatrix = FieldMatrix;
            deadEnd = false;
        }

        /// <summary>
        /// Метод получения отмеченной точки на карте
        /// </summary>
        /// <param name="MarkedPoint">Отмеченная на карте точка</param>
        public void GetMarkedPoint(FieldPoint MarkedCell)
        {
            this.markedCell = MarkedCell;

            if(!roam & (markedCell != null && !markedCell.Equals(targetCell)))
            {
                CanMove = false;
                readyToMove = false;
            }
        }

        /// <summary>
        /// Переключение состояния юнита
        /// </summary>
        public void SwitchBehavior()
        {
            roam = !roam;
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
        private void GetRandomPoint(RandomMode RandomMode)
        {
            temp = fieldMatrix[Random.Range(0, fieldSide), Random.Range(0, fieldSide)];

            switch (RandomMode)
            {
                case RandomMode.All:

                    if (temp.Equals(currentCell) | temp.Blocked)
                    {
                        GetRandomPoint(RandomMode);
                    }
                    targetCell = temp;

                    break;

                case RandomMode.NoMarked:

                    if (temp.Equals(currentCell) | temp.Blocked | temp.Marked)
                    {
                        GetRandomPoint(RandomMode);
                    }
                    targetCell = temp;

                    break;
            }
            
        }

        /// <summary>
        /// Запускает волновой алгоритм поиска пути
        /// </summary>
        private void Wave()
        {
            if(deadEnd)return;

            switch(roam)
            {
                case true:

                    if (targetCell == null)
                    {
                        GetRandomPoint(RandomMode.All);

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
                            GetRandomPoint(RandomMode.All);
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
                            GetRandomPoint(RandomMode.All);
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

                    if(markedCell == null)
                    {
                        GetRandomPoint(RandomMode.NoMarked);

                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if (wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if (deadEnd) return;
                        }

                        cellToMove = false;
                        counter = 0;

                        GetNextCellToMove();

                        CanMove = true;
                    }

                    else
                    {
                        targetCell = markedCell;

                        wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

                        if(wayPoints == null)
                        {
                            deadEnd = WaveAlgorithm.NoWay(fieldMatrix, currentCell);
                            if (deadEnd) return;
                            markedCell = GetReserveMarkedPoint?.Invoke();
                            if (markedCell == null) return;
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
            GetRandomPoint(RandomMode.All);

            wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, targetCell);

            GetNextCellToMove();
        }
    }
}
    
