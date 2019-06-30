using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using UnityEngine;


namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Скрипт расстановки объектов на сцене
    /// </summary>
    public class Placement
    {
        private GameObject Cell;
        private GameObject Unit;

        private GameObject CellTemp;
        private GameObject UnitTemp;

        private int FieldSide;
        private int UnitsCount;

        public Cell[,] Field { get; }

        public Unit[] Units { get; }
        
        /// <summary>
        /// Создает скрипт расстановки объектов на сцене
        /// </summary>
        /// <param name="Cell">Ячейка игрового поля</param>
        /// <param name="Unit">Юнит</param>
        public Placement(GameObject Cell, GameObject Unit)
        {
            this.Cell = Cell;
            this.Unit = Unit;
            FieldSide = Random.Range(5, 10);
            UnitsCount = Random.Range(5, 5);
            
            Field = new Cell[FieldSide, FieldSide];
            Units = new Unit[UnitsCount];

            PlaceCells(FieldSide);
            PlaceUnits(UnitsCount, FieldSide);
            PlaceCamera(FieldSide-1);
        }

        /// <summary>
        /// Расставляет ячейки
        /// </summary>
        /// <param name="Side">Сторона игрового поля</param>
        private void PlaceCells(int Side)
        {
            Vector3 Position = Vector3.zero;
            Quaternion Rotation = new Quaternion();
            int Offset = 1;

            for (int i = 0; i < Side; i++)
            {
                for (int j = 0; j < Side; j++)
                {
                    CellTemp = GameObject.Instantiate(Cell, Position, Rotation);
                    CellTemp.name = "Cell"+i + "" + j;
                    Field[i, j] = CellTemp.GetComponent<Cell>();

                    Position.z += Offset;
                }
                Position.z = 0;
                Position.x += Offset;
            }
        }

        /// <summary>
        /// Расставляет юнитов
        /// </summary>
        /// <param name="Count">Количество юнитов</param>
        /// <param name="Side">сторона игрового поля</param>
        private void PlaceUnits(int Count, int Side)
        {
            Vector3 Position = new Vector3(0, 0.3f, 0);
            Quaternion Rotation = new Quaternion();

            for (int i = 0; i < Count; i++)
            {
                Position.x = Random.Range(0, Side - 1);
                Position.z = Random.Range(0, Side - 1);

                UnitTemp = GameObject.Instantiate(Unit, Position, Rotation);
                UnitTemp.name = "Unit" + i;
                Units[i] = new Unit(UnitTemp.name, UnitTemp);
            }
        }

        /// <summary>
        /// Расположение камеры
        /// </summary>
        /// <param name="Side">Сторона игрового поля</param>
        private void PlaceCamera(int Side)
        {
            Camera.main.transform.position = new Vector3((float)Side / 2, 10, (float)Side / 2);
        }
    }
}


