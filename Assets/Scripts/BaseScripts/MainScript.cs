using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Controllers;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.BaseScripts
{
    /// <summary>
    /// Основной скрипт
    /// </summary>
    public class MainScript : MonoBehaviour
    {
        [SerializeField] private GameObject Cell;
        [SerializeField] private GameObject Unit;
        [SerializeField] private LayerMask Mask;
        [SerializeField] private Text Text;

        public Placement PlacementScript;
        public Field FieldScript;

        public InputController InputController;
        public UnitController UnitController;

        public static MainScript GetMainScript { get; private set; }

        private void Awake()
        {
            GetMainScript = this;

            PlacementScript = new Placement(Cell, Unit);
            FieldScript = new Field(PlacementScript.Field);

            UnitController = new UnitController(PlacementScript.Units, FieldScript.FieldMatrix, FieldScript.MarkedCells);
            InputController = new InputController(Mask);
        }

        private void Update()
        {
            InputController.ControllerUpdate();
            UnitController.ControllerUpdate();
        }

        private void LateUpdate()
        {
            UnitController.ControllerLateUpdate(Time.deltaTime);
        }

        /// <summary>
        /// Переключение поведения юнитов
        /// </summary>
        public void SwitchUnits()
        {
            UnitController.SwitchUnits();
            if (Text.text == "Follow") Text.text = "Roam";
            else Text.text = "Follow";
        }

        /// <summary>
        /// Перезапуск сцены
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene("Units");
        }

        /// <summary>
        /// Выход
        /// </summary>
        public void Exit()
        {
            Application.Quit();
        }


    }
}

