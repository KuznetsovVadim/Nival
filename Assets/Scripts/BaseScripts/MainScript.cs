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
        [SerializeField] private GameObject cell;
        [SerializeField] private GameObject unit;
        [SerializeField] private LayerMask mask;
        [SerializeField] private Text text;

        public Placement PlacementScript;
        public Field FieldScript;

        public InputController InputController;
        public UnitController UnitController;

        public static MainScript GetMainScript { get; private set; }

        private void Awake()
        {
            GetMainScript = this;

            PlacementScript = new Placement(cell, unit);
            FieldScript = new Field(PlacementScript.Field);

            UnitController = new UnitController(PlacementScript.Units, FieldScript.FieldMatrix, FieldScript.MarkedCells);
            InputController = new InputController(mask);
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
            if (text.text == "Follow") text.text = "Roam";
            else text.text = "Follow";
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

