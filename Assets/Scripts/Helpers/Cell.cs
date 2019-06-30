using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Ячейка игрового поля
    /// </summary>
    public class Cell:MonoBehaviour
    {
        private GameObject block;

        private MeshRenderer cellRenderer;

        public Vector3 Position { get; private set; }

        public string Name { get; private set; }

        public bool Blocked { get; private set; }

        public bool Marked { get; private set; }

        private bool locked;

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

        private CellChangies changies;

        public event CellClicked OnClickEvent;

        private void Awake()
        {
            Position = new Vector3(gameObject.transform.position.x, 0.3f, gameObject.transform.position.z);
            Name = gameObject.name;
            block = gameObject.transform.Find("Block").gameObject;

            cellRenderer = gameObject.GetComponent<MeshRenderer>();

            cellRenderer.material.color = Color.yellow;

            Blocked = false;
            Marked = false;
            locked = false;

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

                    if(locked) return;

                    changies = ChangiesStatus(Click);

                    Blocked = !Blocked;

                    BlockSwitch();

                    break;

                case MouseClick.RightButton:

                    changies = ChangiesStatus(Click);

                    Marked = !Marked;

                    MarkSwitch();

                    break;
            }

            OnClickEvent?.Invoke(this, changies);
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
                cellRenderer.material.color = Color.red;
                block.SetActive(true);
                Marked = false;
            }
            else
            {
                cellRenderer.material.color = Color.yellow;
                block.SetActive(false);
            }
        }

        /// <summary>
        /// Переключает отметку ячейки
        /// </summary>
        private void MarkSwitch()
        {
            if (Marked)
            {
                cellRenderer.material.color = Color.green;
                Blocked = false;
                block.SetActive(false);
            }
            else
            {
                cellRenderer.material.color = Color.yellow;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            locked = true;
        }

        private void OnTriggerExit(Collider other)
        {
            locked = false;
        }
    }
}

