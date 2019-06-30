using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Волновой алгоритм поиска пути
    /// </summary>
    public static class WaveAlgorithm
    {
        /// <summary>
        /// Создает матрицу
        /// </summary>
        /// <param name="SourceMatrix"></param>
        /// <returns></returns>
        private static PointModel[,] SetMatrix(FieldPoint[,] SourceMatrix)
        {
            PointModel[,] Matrix = new PointModel[SourceMatrix.GetLength(0), SourceMatrix.GetLength(1)];

            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(0); j++)
                {
                    Matrix[i, j] = SourceMatrix[i, j].CreatePoint();
                }
            }

            return Matrix;
        }

        /// <summary>
        /// Возвращает массив точек маршрута, если цель достижима
        /// </summary>
        /// <param name="SourceMatrix"></param>
        /// <param name="StartPoint"></param>
        /// <param name="TargetPoint"></param>
        /// <returns></returns>
        public static FieldPoint[] ShortWay(FieldPoint[,] SourceMatrix, FieldPoint StartPoint, FieldPoint TargetPoint)
        {
            Queue<PointModel> CellsQueue = new Queue<PointModel>();
            
            PointModel[,] Matrix = SetMatrix(SourceMatrix);

            ref PointModel Start = ref Matrix[StartPoint.X, StartPoint.Z];
            ref PointModel End = ref Matrix[TargetPoint.X, TargetPoint.Z];

            int Step = 2;

            Start.SetStep(1);

            CellsQueue.Enqueue(Start);

            PointModel Last = CellsQueue.Last();

            while (End.StepNumber == 0 & CellsQueue.Count > 0)
            {
                AllDirections(Matrix, CellsQueue, Step, WaveMode.Steps);

                if (CellsQueue.Peek().Equals(Last))
                {
                    Step++;
                    Last = CellsQueue.Last();
                }

                CellsQueue.Dequeue();
            }

            if (End.StepNumber == 0) return null;

            FieldPoint[] WayPoints = new FieldPoint[End.StepNumber];

            Step = End.StepNumber - 1;

            CellsQueue.Clear();

            CellsQueue.Enqueue(End);

            for (int i = Step; i >= 0; i--)
            {
                AllDirections(Matrix, CellsQueue, Step, WaveMode.Route);

                WayPoints[i] = SourceMatrix[CellsQueue.Peek().X, CellsQueue.Peek().Z];

                CellsQueue.Dequeue();

                Step = Step > 1 ? (Step - 1) : 1;
            }

            return WayPoints;
        }

        /// <summary>
        /// Проверяет есть ли выход из текущего положения юнита
        /// </summary>
        /// <param name="SourceMatrix"></param>
        /// <param name="CurrentPoint"></param>
        /// <returns></returns>
        public static bool NoWay(FieldPoint[,] SourceMatrix, FieldPoint CurrentPoint)
        {
            PointModel CurrentCell = CurrentPoint.CreatePoint();

            PointModel[] Directions = SetDirections(CurrentCell);
            
            for (int i = 0; i < Directions.Length; i++)
            {
                if(!SourceMatrix[Directions[i].X, Directions[i].Z].Blocked)
                {
                    return false;
                }
            }
            return true;
        }

        #region StepsAndRoutes

        /// <summary>
        /// Находит путь к цели юнита
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Matrix"></param>
        /// <param name="CellsQueue"></param>
        /// <param name="Step"></param>
        private static void SetStep(PointModel Model, PointModel[,] Matrix, Queue<PointModel> CellsQueue, int Step)
        {
            int i = Model.X;
            int j = Model.Z;

            if ((i < 0 || i > Matrix.GetLength(0) - 1) || (j < 0 || j > Matrix.GetLength(1) - 1)) return;
            if (Matrix[i, j].StepNumber != 0 | Matrix[i, j].Blocked) return;

            Matrix[i, j].SetStep(Step);

            CellsQueue.Enqueue(Matrix[i, j]);
        }

        /// <summary>
        /// Создает маршрут к цели юнита
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Matrix"></param>
        /// <param name="CellsQueue"></param>
        /// <param name="Step"></param>
        /// <param name="EndMethod"></param>
        private static void SetRoute(PointModel Model, PointModel[,] Matrix, Queue<PointModel> CellsQueue, int Step, ref bool EndMethod)
        {
            int i = Model.X;
            int j = Model.Z;

            if ((i < 0 || i > Matrix.GetLength(0) - 1) || (j < 0 || j > Matrix.GetLength(1) - 1)) return;
            if (Matrix[i, j].StepNumber == 0 | Matrix[i, j].Blocked) return;

            if (Matrix[i, j].StepNumber == Step)
            {
                CellsQueue.Enqueue(Matrix[i, j]);
                EndMethod = true;
            }
        }

        /// <summary>
        /// Просчет возможных направлений движения
        /// </summary>
        /// <param name="CurrentCell"></param>
        /// <returns></returns>
        private static PointModel[] SetDirections(PointModel CurrentCell)
        {
            PointModel[] Temp;

            switch (CurrentCell.Orientation)
            {
                case Orientation.Center:

                    return Temp = new PointModel[8]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                case Orientation.BottonSide:

                    return Temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                    };

                case Orientation.LeftSide:

                    return Temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                    };

                case Orientation.RightSide:
                    
                    return Temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                case Orientation.TopSide:

                    return Temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                case Orientation.DownLeftCorner:

                    return Temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                    };

                case Orientation.DownRightCorner:

                    return Temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                    };

                case Orientation.TopLeftCorner:

                    return Temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        
                    };

                case Orientation.TopRightCorner:

                    return Temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                default:

                    return Temp = new PointModel[8]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };
            }
        }

        /// <summary>
        /// Проверяет все доступные направления движения
        /// </summary>
        /// <param name="Matrix"></param>
        /// <param name="CellsQueue"></param>
        /// <param name="Step"></param>
        /// <param name="mode"></param>
        private static void AllDirections(PointModel[,] Matrix, Queue<PointModel> CellsQueue, int Step, WaveMode mode)
        {
            PointModel CurrentCell = CellsQueue.Peek();

            bool EndMethod = false;

            PointModel[] Directions = SetDirections(CurrentCell);

            switch (mode)
            {
                case WaveMode.Steps:

                    for (int i = 0; i < Directions.Length; i++)
                    {
                        SetStep(Directions[i], Matrix, CellsQueue, Step);
                    }
                    break;

                case WaveMode.Route:

                    for (int i = 0; i < Directions.Length; i++)
                    {
                        if (EndMethod) return;
                        SetRoute(Directions[i], Matrix, CellsQueue, Step, ref EndMethod);
                    }

                    break;
            }
        }

        #endregion
    }
}
