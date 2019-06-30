using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;


namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Контроллер юнитов
    /// </summary>
    public class UnitController:BaseController
    {
        private Unit[] units;

        private FieldPoint[,] fieldMatrix;

        private List<FieldPoint> markedCells;

        private List<FieldPoint> reservePoints;

        public event MapChanged MapChanged;

        private event Action switchBehavior;

        /// <summary>
        /// Создает контроллер управляющий юнитами
        /// </summary>
        /// <param name="Units">Все доступные юниты</param>
        /// <param name="FieldMatrix">Матрица игрового поля</param>
        /// <param name="MarkedCells">Список "отмеченных" ячеек игрового поля</param>
        public UnitController(Unit[] Units, FieldPoint[,] FieldMatrix, List<FieldPoint> MarkedCells)
        {
            units = Units;
            fieldMatrix = FieldMatrix;
            markedCells = MarkedCells;
            reservePoints = new List<FieldPoint>();

            MainScript.GetMainScript.FieldScript.FieldChanged += SetChanges;

            foreach (var Unit in units)
            {
                MapChanged += Unit.GetMatrixUpdate;
                switchBehavior += Unit.SwitchBehavior;
                Unit.GetReserveMarkedPoint += SendReservedMarkedPoint;
                Unit.GetStartData(FieldMatrix);
            }
        }

        /// <summary>
        /// Передает изменения игрового поля юниту
        /// </summary>
        /// <param name="FieldMatrix">Матрица игрового поля</param>
        /// <param name="MarkedCells">Список "отмеченных" ячеек игрового поля</param>
        public void SetChanges(FieldPoint[,] FieldMatrix, List<FieldPoint> MarkedCells)
        {
            if(FieldMatrix != null & MarkedCells != null)
            {
                MapChanged.Invoke(FieldMatrix);
                SendMarkedPointsToUnits();
            }
            else
            {
                if(FieldMatrix != null)
                {
                    MapChanged.Invoke(FieldMatrix);
                }
                else
                {
                    SendMarkedPointsToUnits();
                }
            }
        }

        /// <summary>
        /// Раздает юнитам "отмеченные" ячейки. Если остались лишние ячейки - отправляет в резерв
        /// </summary>
        private void SendMarkedPointsToUnits()
        {
            int counter = 0;

            for(int i = 0; i < units.Length; i++)
            {
                if(counter < markedCells.Count)
                {
                    units[i].GetMarkedPoint(markedCells[counter]);
                    counter++;
                }
                else
                {
                    units[i].GetMarkedPoint(null);
                }
            }

            if(counter < markedCells.Count)
            {
                for (int i = counter; i < markedCells.Count; i++)
                {
                    reservePoints.Add(markedCells[counter]);
                }
            }
            else
            {
                reservePoints.Clear();
            }
        }

        /// <summary>
        /// Получает ячейки из резерва
        /// </summary>
        /// <returns></returns>
        private FieldPoint SendReservedMarkedPoint()
        {
            FieldPoint Temp = null;

            if (reservePoints.Count > 0)
            {
                Temp = reservePoints[0];
                reservePoints.Remove(reservePoints[0]);
            }

            return Temp;
        }

        /// <summary>
        /// Переключение поведения юнитов
        /// </summary>
        public void SwitchUnits()
        {
            switchBehavior.Invoke();
        }

        public override void ControllerUpdate()
        {
            foreach (var Unit in units)
            {
                Unit.Update();
            }
        }

        public override void ControllerLateUpdate(float Time)
        {
            foreach (var Unit in units)
            {
                Unit.LateUpdate(Time);
            }
        }
    }
}


