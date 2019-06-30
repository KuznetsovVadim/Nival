using Assets.Scripts.Controllers;
using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Ячейка игрового поля
    /// </summary>
    public class Cell:MonoBehaviour
    {
        private GameObject Block;

        private MeshRenderer Renderer;

        public Vector3 Position { get; private set; }

        public string Name { get; private set; }

        public bool Blocked { get; private set; }

        public bool Marked { get; private set; }

        public bool Locked { get; private set; }

        public int X
        {
            get
            {
                return (int)Position.x;
            }
        }

        public int Z
        {
            get
            {
                return (int)Position.z;
            }
        }

        private CellChangies Changies;

        public event CellClicked OnClickEvent;

        private void Awake()
        {
            Position = new Vector3(gameObject.transform.position.x, 0.3f, gameObject.transform.position.z);
            Name = gameObject.name;
            Block = gameObject.transform.Find("Block").gameObject;

            Renderer = gameObject.GetComponent<MeshRenderer>();

            Renderer.material.color = Color.yellow;

            Blocked = false;
            Marked = false;
            Locked = false;

        }

        /// <summary>
        /// Возвращает ячейку матрицы соответсвующую ячейке игрового поля
        /// </summary>
        /// <param name="Blocked">Заблокирована</param>
        /// <param name="Marked">Отмечена</param>
        /// <returns></returns>
        public FieldPoint CreatePoint(int Side, bool Blocked = false, bool Marked = false)
        {
            return new FieldPoint(X, Z, Position, Side, Blocked, Marked);
        }

        /// <summary>
        /// Получает данные о щелчке мыши и обрабатывает их
        /// </summary>
        /// <param name="Click">Параметры щелчка мыши</param>
        internal void OnClick(MouseClick Click)
        {
            switch(Click)
            {
                case MouseClick.LeftButton:

                    if(Locked) return;

                    Changies = ChangiesStatus(Click);

                    Blocked = !Blocked;

                    BlockSwitch();

                    break;

                case MouseClick.RightButton:

                    Changies = ChangiesStatus(Click);

                    Marked = !Marked;

                    MarkSwitch();

                    break;
            }

            OnClickEvent?.Invoke(this, Changies);
        }

        /// <summary>
        /// Проверяет изменения в ячейке и возвращает тип изменений (одиночное \ двойное)
        /// </summary>
        /// <param name="Click">Параметры щелчка мыши</param>
        /// <returns></returns>
        private CellChangies ChangiesStatus(MouseClick Click)
        {
            if(!Blocked & !Marked)
            {
                return CellChangies.OneWay;
            }
            else
            {
                if(Blocked & Click == MouseClick.RightButton)
                {
                    return CellChangies.TwoWays;
                }
                if(Marked & Click == MouseClick.LeftButton)
                {
                    return CellChangies.TwoWays;
                }
                return CellChangies.OneWay;
            }
        }

        /// <summary>
        /// Переключает блокировку ячейки
        /// </summary>
        private void BlockSwitch()
        {
            if(Blocked)
            {
                Renderer.material.color = Color.red;
                Block.SetActive(true);
                Marked = false;
            }
            else
            {
                Renderer.material.color = Color.yellow;
                Block.SetActive(false);
            }
        }

        /// <summary>
        /// Переключает отметку ячейки
        /// </summary>
        private void MarkSwitch()
        {
            if (Marked)
            {
                Renderer.material.color = Color.green;
                Blocked = false;
                Block.SetActive(false);
            }
            else
            {
                Renderer.material.color = Color.yellow;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            Locked = true;
        }

        private void OnTriggerExit(Collider other)
        {
            Locked = false;
        }
    }
}

