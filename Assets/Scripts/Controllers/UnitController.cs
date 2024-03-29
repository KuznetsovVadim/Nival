﻿using Assets.Scripts.BaseScripts;
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
            MapChanged.Invoke(FieldMatrix);
            if(MarkedCells != null)
            {
                SendMarkedPointsToUnits();
            }
        }

        /// <summary>
        /// Раздает юнитам "отмеченные" ячейки. Если остались лишние ячейки - отправляет в резерв
        /// </summary>
        private void SendMarkedPointsToUnits()
        {
            reservePoints.Clear();

            foreach (var Cell in markedCells)
            {
                reservePoints.Add(Cell);
            }

            for (int i = 0; i < units.Length; i++)
            {
                units[i].GetMarkedPointsUpdate(reservePoints);
            }
        }

        /// <summary>
        /// Переключение поведения юнитов
        /// </summary>
        public void SwitchUnits()
        {
            switchBehavior.Invoke();
            SendMarkedPointsToUnits();
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


