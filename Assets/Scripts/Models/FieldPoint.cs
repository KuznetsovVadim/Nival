using System;
using UnityEngine;

namespace Assets.Scripts.Models
{
    /// <summary>
    /// Модель ячейки для юнита
    /// </summary>
    public class FieldPoint: IEquatable<FieldPoint>
    {
        public Vector3 Position { get; private set; }

        public bool Blocked { get; private set; }
        public bool Marked { get; private set; }

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

        private int Side;
        
        /// <summary>
        /// Создает модель ячейки для юнита
        /// </summary>
        /// <param name="X">Позиция Х в матрице</param>
        /// <param name="Z">Позиция Y в матрице</param>
        /// <param name="Position">Позиция на игровом поле</param>
        /// <param name="Side">Сторона игрового поля</param>
        /// <param name="Blocked">Блокировка</param>
        /// <param name="Marked">Отмечен</param>
        public FieldPoint(int X, int Z, Vector3 Position, int Side, bool Blocked = false, bool Marked = false)
        {
            this.Position = Position;
            this.Blocked = Blocked;
            this.Marked = Marked;
            this.Side = Side;
        }

        /// <summary>
        /// Изменяет состояние модели ячейки и возвращает ее
        /// </summary>
        /// <param name="Blocked">Блокировка</param>
        /// <param name="Marked">Отмечен</param>
        /// <returns></returns>
        public FieldPoint Point(bool Blocked, bool Marked)
        {
            this.Blocked = Blocked;
            this.Marked = Marked;
            return this;
        }

        public bool Equals(FieldPoint other)
        {
            if (other == null) return false;
            if (other.Position == Position) return true;
            return false;
        }

        /// <summary>
        /// Создает модель матрицы волнового алгоритма и возвращает ее
        /// </summary>
        /// <returns></returns>
        public PointModel CreatePoint()
        {
            return new PointModel((int)Position.x, (int)Position.z, Side, Blocked, Marked);
        }
    }
}
