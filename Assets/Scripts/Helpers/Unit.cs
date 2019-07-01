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

        public bool Follow { get; private set; } = true;

        private bool readyToMove = false;

        private bool cellToMove = false;

        private bool deadEnd = false;

        private bool noMarkedPointToReach = false;

        public bool CanMove { get; private set; } = false;

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
        }

        /// <summary>
        /// Получает обновленную матрицу игрового поля и ячейку, в которой произошло последнее изменение
        /// </summary>
        /// <param name="ChangedCell">Ячейка с изменениями</param>
        /// <param name="FieldMatrix">Матрица поля</param>
        public void GetMatrixUpdate(FieldPoint[,] FieldMatrix)
        {
            fieldMatrix = FieldMatrix;
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
            if(CanMove)
            {
                UpdateCurrentPosition();
                GetDirection();
            }
            else
            {
                Wave();
            }
        }

        public void LateUpdate(float Time)
        {

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
            if (cellToMove) return;
            if (counter < wayPoints.Length)
            {
                if(wayPoints[counter].Blocked)
                {
                    CanMove = false;
                    Wave();
                }
                else
                {

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
        

        private void Wave()
        {
            wayPoints = WaveAlgorithm.ShortWay(fieldMatrix, currentCell, Roam ? UnitState.Roam : UnitState.Follow);

            if(wayPoints == null)
            {
                CanMove = false;
            }
            else
            {
                CanMove = true;
            }
        }
    }
}
    
