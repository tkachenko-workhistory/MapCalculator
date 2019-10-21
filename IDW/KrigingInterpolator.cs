using Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kriging
{
    public class KrigingInterpolator : IInterpolator
    {
        public bool Interpolate(Point3D[][] map, List<Point3D> points, bool[][] calculatingMask,
            IInterpolationOptions options)
        {
            // Первое о чём нам пишут в методичке по Simple Kriging, это то что за m надо взять фиксированнео значение равное мат ожиданию
            //  найдём среднюю высоту
            var m = points.Average(p => p.Z);

            double[,] distanceMatrix = CalcDistanceMatrix(points);
            // Параметр K из методички
            double[,] covarianceMatrix = ConvertToCovariance(distanceMatrix);
            // Найдём обратную матрицу K^-1
            var reverse = InvertibleMatrix(covarianceMatrix);

            // Изначально я хотел перебрать все points и найти какой ячейке они принадлежат,
            //  но сранив файлы output.xyz и input.txt, я понял что так ни кто не делал
            //  к тому же понадобился бы шаг сетки, а его из этого метода не получить
            //  ... если только в options передать. 

            // Надо заполнить map высотами
            // Теперь будем перебирать все ячейки
            for (var i = 0; i < map.Length; i++)
                for (var j = 0; j < map[i].Length; j++)
                {
                    // Значение ковариаци от расстояния до каждой из точек points
                    var vector = points.Select(p => CovarianceFunction(p.Distance(map[i][j]))).ToArray();
                    // Транспонируем вектор так чтобы у нас получилась матрица с одной колонкой
                    var k = new double[points.Count, 1];
                    for (var l = 0; l < points.Count; l++)
                        k[l, 0] = vector[l];

                    // Умножим обратную матрицу на этот вектор
                    //  в методичке он right-hand vector, 
                    //  очевидно не просто так и должен идти вторым
                    // K^-1 * k
                    var krigingWeights = Multiplication(reverse, k);

                    // Расчитаем непосредственно высоту - то для чего всё это и затевалось
                    double summ = points.Select((t, l) => krigingWeights[l, 0] * (t.Z - m)).Sum();
                    map[i][j].Z = m + summ;
                }
            return true;
        }

        /// <summary>
        /// Надём обратную матрицу методом Гаусса — Жордана
        /// </summary>
        /// <param name="matrix">Исходная матрица</param>
        public double[,] InvertibleMatrix(double[,] matrix)
        {
            var size = matrix.GetLength(0);
            // Для того чтобы исключить внесение изменений в переданную матрицу - сделаем её копию
            double[,] matrixClone = new double[size, size];
            Array.Copy(matrix, matrixClone, matrix.Length);

            // Создадим единичную матрицу
            var resultMatrix = new double[size, size];
            for (int i = 0; i < size; i++)
                resultMatrix[i, i] = 1;

            // Прямой ход - образование нулей ПОД главной диагональю
            for (int i = 0; i < size; i++)
            {
                // Для получения 1 на главной диагонали
                var divider = matrixClone[i, i];
                // Разделим всю строку на значение на главной диагонали
                for (int j = 0; j < size; j++)
                {
                    matrixClone[i, j] /= divider;
                    resultMatrix[i, j] /= divider;
                }

                // Теперь получим 0 под главной диагональю
                //   для всех нижележащих строк...
                for (int k = i + 1; k < size; k++)
                {
                    var multiplier = matrixClone[k, i];
                    // Вычтем из каждого столбца
                    for (int j = 0; j < size; j++)
                    {
                        matrixClone[k, j] -= matrixClone[i, j] * multiplier;
                        resultMatrix[k, j] -= resultMatrix[i, j] * multiplier;
                    }
                }

            }
            // Обратный ход - образование нулей НАД главной диагональю
            for (int i = size - 1; i > 0; i--)
            {
                // Теперь получим 0 над главной диагональю
                // для всех строк выше...
                for (int k = i - 1; k >= 0; k--)
                {
                    var multiplier = matrixClone[k, i];
                    // Вычтем из каждого столбца
                    for (int j = 0; j < size; j++)
                    {
                        matrixClone[k, j] -= matrixClone[i, j] * multiplier;
                        resultMatrix[k, j] -= resultMatrix[i, j] * multiplier;
                    }
                }
            }

            return resultMatrix;
        }

        /// <summary>
        /// Умножение матриц
        /// </summary>
        /// <param name="a">В ответе количество строк будет равно количеству строк в этой матрице</param>
        /// <param name="b">В ответе количество столбцов будет равно количеству столбцов в этой матрице</param>
        /// <returns>Результат умножения</returns>
        public double[,] Multiplication(double[,] a, double[,] b)
        {
            double[,] r = new double[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    for (int k = 0; k < b.GetLength(0); k++)
                        r[i, j] += a[i, k] * b[k, j];
                }
            }
            return r;
        }

        /// <summary>
        /// Матрица расстояний между парами координат
        /// </summary>
        public double[,] CalcDistanceMatrix(List<Point3D> points)
        {
            double[,] distanceMatrix = new double[points.Count, points.Count];
            for (int i = 0; i < points.Count - 1; i++)
            {
                distanceMatrix[i, i] = 0;
                for (int j = i + 1; j < points.Count; j++)
                {
                    var distance = points[i].Distance(points[j]);
                    // Матрица симметрична
                    distanceMatrix[i, j] = distance;
                    distanceMatrix[j, i] = distance;
                }
            }
            return distanceMatrix;
        }

        /// <summary>
        /// Получить ковариационную матрицу
        /// </summary>
        /// <param name="distanceMatrix">Матрица расстояний</param>
        public double[,] ConvertToCovariance(double[,] distanceMatrix)
        {
            var size = distanceMatrix.GetLength(0);
            var covarianceMatrix = new double[size, size];

            // На главной диагонали всегда нули - не будет считать всё время одно и то же
            var covariansInZero = CovarianceFunction(0);
            for (int i = 0; i < size - 1; i++)
            {
                covarianceMatrix[i, i] = covariansInZero;
                for (int j = i + 1; j < size; j++)
                {
                    var covarians = CovarianceFunction(distanceMatrix[i, j]);
                    // Матрица так же симметрична
                    covarianceMatrix[i, j] = covarians;
                    covarianceMatrix[j, i] = covarians;
                }
            }
            return covarianceMatrix;
        }

        /// <summary>
        /// Некая коварианционная функция взятая из методички
        /// </summary>
        /// <param name="h">Расстояние</param>
        /// <returns>Оценка ковариаионной функции</returns>
        /// <remarks>C(h) = C(0)-g(h) = 0.78 * (1-1.5 * (h/4141)+ 0.5 * (h/4141)3)</remarks>
        public double CovarianceFunction(double h)
        {
            return 0.78 * (1 - 1.5 * (h / 4141) + 0.5 * Math.Pow(h / 4141, 3));
        }
    }
}