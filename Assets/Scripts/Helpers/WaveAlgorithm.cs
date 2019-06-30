using Assets.Scripts.Models;
using System.Collections.Generic;
using System.Linq;

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
            PointModel[,] matrix = new PointModel[SourceMatrix.GetLength(0), SourceMatrix.GetLength(1)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    matrix[i, j] = SourceMatrix[i, j].CreatePoint();
                }
            }

            return matrix;
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
            Queue<PointModel> cellsQueue = new Queue<PointModel>();
            
            PointModel[,] matrix = SetMatrix(SourceMatrix);

            ref PointModel start = ref matrix[StartPoint.X, StartPoint.Z];
            ref PointModel end = ref matrix[TargetPoint.X, TargetPoint.Z];

            int step = 2;

            start.SetStep(1);

            cellsQueue.Enqueue(start);

            PointModel last = cellsQueue.Last();

            while (end.StepNumber == 0 & cellsQueue.Count > 0)
            {
                AllDirections(matrix, cellsQueue, step, WaveMode.Steps);

                if (cellsQueue.Peek().Equals(last))
                {
                    step++;
                    last = cellsQueue.Last();
                }

                cellsQueue.Dequeue();
            }

            if (end.StepNumber == 0) return null;

            FieldPoint[] WayPoints = new FieldPoint[end.StepNumber];

            step = end.StepNumber - 1;

            cellsQueue.Clear();

            cellsQueue.Enqueue(end);

            for (int i = step; i >= 0; i--)
            {
                AllDirections(matrix, cellsQueue, step, WaveMode.Route);

                WayPoints[i] = SourceMatrix[cellsQueue.Peek().X, cellsQueue.Peek().Z];

                cellsQueue.Dequeue();

                step = step > 1 ? (step - 1) : 1;
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
            PointModel currentCell = CurrentPoint.CreatePoint();

            PointModel[] directions = SetDirections(currentCell);
            
            for (int i = 0; i < directions.Length; i++)
            {
                if(!SourceMatrix[directions[i].X, directions[i].Z].Blocked)
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
            PointModel[] temp;

            switch (CurrentCell.Orientation)
            {
                case Orientation.Center:

                    return temp = new PointModel[8]
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

                    return temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                    };

                case Orientation.LeftSide:

                    return temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                    };

                case Orientation.RightSide:
                    
                    return temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                case Orientation.TopSide:

                    return temp = new PointModel[5]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                case Orientation.DownLeftCorner:

                    return temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                    };

                case Orientation.DownRightCorner:

                    return temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z+1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z+1),
                    };

                case Orientation.TopLeftCorner:

                    return temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X+1, CurrentCell.Z),
                        new PointModel(CurrentCell.X+1, CurrentCell.Z-1),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        
                    };

                case Orientation.TopRightCorner:

                    return temp = new PointModel[3]
                    {
                        new PointModel(CurrentCell.X-1, CurrentCell.Z),
                        new PointModel(CurrentCell.X,   CurrentCell.Z-1),
                        new PointModel(CurrentCell.X-1, CurrentCell.Z-1),
                    };

                default:

                    return temp = new PointModel[8]
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
            PointModel currentCell = CellsQueue.Peek();

            bool EndMethod = false;

            PointModel[] Directions = SetDirections(currentCell);

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
