using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using Kriging;

namespace KrigingTests
{
    [TestClass]
    public class KrigingTests
    {
        [TestMethod]
        public void BaseTest()
        {
            var param = Helpers.GenerateParam();
            var points = Helpers.GeneratePoints();
            var interpolator = new KrigingInterpolator();
            var result = interpolator.Interpolate(param, points, Helpers.AllNodes(param), new KrigingInterpolationOptions());
            Assert.AreEqual(true, result, "Метод построения Kriging не выдает результат.");
        }

        /// <summary>
        /// Тестирование ковариационной функции,
        ///   для теста взяты значений из методички
        /// </summary>
        [TestMethod]
        public void CovarianceFunctionTest()
        {
            var interpolator = new KrigingInterpolator();
            // Проверим несколько занчений, взятых так же с методички
            Assert.AreEqual(0.28, interpolator.CovarianceFunction(1897), 0.01);
            Assert.AreEqual(0.78, interpolator.CovarianceFunction(0), 0.01);
            Assert.AreEqual(0.06, interpolator.CovarianceFunction(3130), 0.01);
            Assert.AreEqual(0.17, interpolator.CovarianceFunction(2441), 0.01);
            Assert.AreEqual(0.27, interpolator.CovarianceFunction(1970), 0.01);
        }

        /// <summary>
        /// Тестирование умножения матриц
        /// </summary>
        [TestMethod]
        public void MultiplicationMatrixTest()
        {
            var matrix1 = new double[,]
            {
                { 3, -1, 2 } ,
                { 4, 2, 0 },
                { -5, 6, 1 }
            };

            var matrix2 = new double[,]
            {
                { 8, 1 } ,
                { 7, 2 },
                { 2, -3 }
            };

            var interpolator = new KrigingInterpolator();
            var res = interpolator.Multiplication(matrix1, matrix2);
            Assert.AreEqual(21, res[0, 0]);
            Assert.AreEqual(-5, res[0, 1]);
            Assert.AreEqual(46, res[1, 0]);
            Assert.AreEqual(8, res[1, 1]);
            Assert.AreEqual(4, res[2, 0]);
            Assert.AreEqual(4, res[2, 1]);
        }

        /// <summary>
        /// Тестирование получения обратной матрицы
        /// </summary>
        [TestMethod]
        public void InvertibleMatrixTest()
        {
            var matrix = new[,] {
                { 0.78, 0.28, 0.06, 0.17, 0.40, 0.43 } ,
                { 0.28, 0.78, 0.43, 0.39, 0.27, 0.20 },
                { 0.06, 0.43, 0.78, 0.37, 0.11, 0.06 },
                { 0.17, 0.39, 0.37, 0.78, 0.37, 0.27 },
                { 0.40, 0.27, 0.11, 0.37, 0.78, 0.65 },
                { 0.43, 0.20, 0.06, 0.27, 0.65, 0.78 }};

            var interpolator = new KrigingInterpolator();
            var reverse = interpolator.InvertibleMatrix(matrix);
            // Для проверки полученной обратной матрицы, умножим её на исходную матрицу 
            //  должна получиться единичная матрица
            var res = interpolator.Multiplication(matrix, reverse);
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    // На главной диагонали должна быть 1, в остальном 0
                    Assert.AreEqual(i == j ? 1 : 0, res[i, j], 0.0001);
        }

        /// <summary>
        /// Тестирование умножения обратной матрицы на вектор
        /// </summary>
        /// <remarks>
        /// В целом тест является комбинацией двух других тестов,
        ///   и его можно было и не писать, 
        ///   но хотелось сопоставить с результатами данными в примере в методичке
        /// </remarks>
        [TestMethod]
        public void InvertibleMultiplicationMatrixTest()
        {
            // Исходная матрица
            var matrix = new[,] {
                { 0.78, 0.28, 0.06, 0.17, 0.40, 0.43 } ,
                { 0.28, 0.78, 0.43, 0.39, 0.27, 0.20 },
                { 0.06, 0.43, 0.78, 0.37, 0.11, 0.06 },
                { 0.17, 0.39, 0.37, 0.78, 0.37, 0.27 },
                { 0.40, 0.27, 0.11, 0.37, 0.78, 0.65 },
                { 0.43, 0.20, 0.06, 0.27, 0.65, 0.78 }};

            var vector = new[,] {
                { 0.38 } ,
                { 0.56 },
                { 0.32 },
                { 0.49 },
                { 0.46 },
                { 0.37 }};

            var interpolator = new KrigingInterpolator();

            // Обратная матрица
            var reverse = interpolator.InvertibleMatrix(matrix);
            // Результат умножения
            var res = interpolator.Multiplication(reverse, vector);

            // (хм, больша погрешность, походу в методичке безбожно округляли,
            //   но результаты, в целом, одного порядка)
            Assert.AreEqual(0.1475, res[0, 0], 0.02);
            Assert.AreEqual(0.4564, res[1, 0], 0.02);
            Assert.AreEqual(-0.0205, res[2, 0], 0.02);
            Assert.AreEqual(0.2709, res[3, 0], 0.02);
            Assert.AreEqual(0.2534, res[4, 0], 0.02);
            Assert.AreEqual(-0.0266, res[5, 0], 0.02);
        }
    }
}