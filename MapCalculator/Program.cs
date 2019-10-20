using Core;
using Kriging;

namespace MapCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Здесь необходимо заполнить названия файлов используя параметры командной строки, либо диалоги
            var filePoints = string.Empty;
            var fileGrid = string.Empty;
            var fileMap = string.Empty;
            /// Там надо реализовать методы загрузки параметров в каком либо виде, например точки X,Y,Z, парамкетры сетки как минимум/максимум по X|Y и количество ущлов по X|Y
            var points = Helpers.LoadPoints(filePoints);
            var map = Helpers.LoadGrid(fileGrid);
            /// В каких узлах считаем
            var mask = Helpers.AllNodes(map);
            /// Метод интерполяции
            var interpolator = new KrigingInterpolator();
            /// Сама интерполяция, сейчас параметры специфичные не определены
            if (interpolator.Interpolate(map, points, mask, new KrigingInterpolationOptions()))
            {
                /// Необходимо реалиозовать осхранение
                Helpers.SaveMap(fileMap, map);
            }
        }
    }
}