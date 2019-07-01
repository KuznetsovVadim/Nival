using Assets.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public static FieldPoint[] ShortWay(FieldPoint[,] SourceMatrix, FieldPoint StartPoint, UnitState UnitSate)
        {
            Queue<PointModel> cellsQueue = new Queue<PointModel>();
            
            PointModel[,] matrix = SetMatrix(SourceMatrix);

            ref PointModel start = ref matrix[StartPoint.X, StartPoint.Z];

            PointModel end = new PointModel();

            PointModel first = start;
            
            int step = 2;

            start.SetStep(1);

            cellsQueue.Enqueue(start);
            
            while (cellsQueue.Count > 0 & end.StepNumber != 0)
            {
                AllDirections(matrix, cellsQueue, step, WaveMode.Steps, UnitSate, ref end);

                if(cellsQueue.Peek().Equals(first))
                {
                    step++;
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
                AllDirections(matrix, cellsQueue, step, WaveMode.Route, UnitSate, ref end);

                WayPoints[i] = SourceMatrix[cellsQueue.Peek().X, cellsQueue.Peek().Z];

                cellsQueue.Dequeue();

                step = step > 1 ? (step - 1) : 1;
            }

            return WayPoints;
        }

        /// <summary>
        /// Проверяет возможность достичь точки назначения
        /// </summary>
        /// <param name="SourceMatrix"></param>
        /// <param name="StartPoint"></param>
        /// <param name="TargetPoint"></param>
        /// <returns></returns>
        public static bool CanReach(FieldPoint[,] SourceMatrix, FieldPoint StartPoint, FieldPoint TargetPoint)
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

            if (end.StepNumber == 0) return false;
            return true;
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
        private static void SetStep(PointModel Model, PointModel[,] Matrix, Queue<PointModel> CellsQueue, int Step, UnitState UnitState, ref bool TargetFound)
        {
            int i = Model.X;
            int j = Model.Z;
            
            if (Matrix[i, j].StepNumber != 0 | Matrix[i, j].Blocked) return;

            Matrix[i, j].SetStep(Step);

            switch (UnitState)
            {
                case UnitState.Roam:
                    
                    TargetFound = true;

                    break;

                case UnitState.Follow:

                    if(Matrix[i, j].Marked) TargetFound = true;

                    break;
            }

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
        private static void AllDirections(PointModel[,] Matrix, Queue<PointModel> CellsQueue, int Step, WaveMode mode, UnitState UnitSate, ref PointModel EndPoint)
        {
            PointModel currentCell = CellsQueue.Peek();

            bool EndMethod = false;

            PointModel[] Directions = SetDirections(currentCell);

            switch (mode)
            {
                case WaveMode.Steps:

                    int RandomMove = Random.Range(0, Directions.Length - 1);

                    SetStep(Directions[RandomMove], Matrix, CellsQueue, Step, UnitSate, ref EndMethod);

                    if(EndMethod)
                    {
                        EndPoint = Directions[RandomMove];
                        return;
                    }

                    for (int i = 0; i < Directions.Length; i++)
                    {
                        if (EndMethod)
                        {
                            EndPoint = Directions[i];
                            return;
                        }

                        SetStep(Directions[i], Matrix, CellsQueue, Step, UnitSate, ref EndMethod);
                        
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
