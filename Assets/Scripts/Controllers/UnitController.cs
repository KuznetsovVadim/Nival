using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Контроллер юнитов
    /// </summary>
    public class UnitController:BaseController
    {
        private Unit[] Units;

        private FieldPoint[,] FieldMatrix;

        private List<FieldPoint> MarkedCells;

        private List<FieldPoint> ReservePoints;

        public event MapChanged MapChanged;

        private event Action SwitchBehavior;

        /// <summary>
        /// Создает контроллер управляющий юнитами
        /// </summary>
        /// <param name="Units">Все доступные юниты</param>
        /// <param name="FieldMatrix">Матрица игрового поля</param>
        /// <param name="MarkedCells">Список "отмеченных" ячеек игрового поля</param>
        public UnitController(Unit[] Units, FieldPoint[,] FieldMatrix, List<FieldPoint> MarkedCells)
        {
            this.Units = Units;
            this.FieldMatrix = FieldMatrix;
            this.MarkedCells = MarkedCells;
            ReservePoints = new List<FieldPoint>();

            MainScript.GetMainScript.FieldScript.FieldChanged += SetChanges;

            foreach (var Unit in Units)
            {
                MapChanged += Unit.GetMatrixUpdate;
                SwitchBehavior += Unit.SwitchBehavior;
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
                this.FieldMatrix = FieldMatrix;

                MapChanged.Invoke(FieldMatrix);
                SendMarkedPointsToUnits();
            }
            else
            {
                if(FieldMatrix != null)
                {
                    this.FieldMatrix = FieldMatrix;
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

            for(int i = 0; i < Units.Length; i++)
            {
                if(counter < MarkedCells.Count)
                {
                    Units[i].GetMarkedPoint(MarkedCells[counter]);
                    counter++;
                }
                else
                {
                    Units[i].GetMarkedPoint(null);
                }
            }

            if(counter < MarkedCells.Count)
            {
                for (int i = counter; i < MarkedCells.Count; i++)
                {
                    ReservePoints.Add(MarkedCells[counter]);
                }
            }
            else
            {
                ReservePoints.Clear();
            }
        }

        /// <summary>
        /// Получает ячейки из резерва
        /// </summary>
        /// <returns></returns>
        private FieldPoint SendReservedMarkedPoint()
        {
            FieldPoint Temp = null;

            if (ReservePoints.Count > 0)
            {
                Temp = ReservePoints[0];
                ReservePoints.Remove(ReservePoints[0]);
            }

            return Temp;
        }

        /// <summary>
        /// Переключение поведения юнитов
        /// </summary>
        public void SwitchUnits()
        {
            SwitchBehavior.Invoke();
        }

        public override void ControllerUpdate()
        {
            foreach (var Unit in Units)
            {
                Unit.Update();
            }
        }

        public override void ControllerLateUpdate(float Time)
        {
            foreach (var Unit in Units)
            {
                Unit.LateUpdate(Time);
            }
        }
    }
}


