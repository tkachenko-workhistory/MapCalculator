using Core;
using Kriging;

namespace MapCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Здесь необходимо заполнить названия файлов используя параметры командной строки, либо диалоги
            var filePoints = args[0];
            var fileGrid = args[1];
            var fileMap = args[2];
            // Там надо реализовать методы загрузки параметров в каком либо виде, например точки X,Y,Z, парамкетры сетки как минимум/максимум по X|Y и количество ущлов по X|Y
            var points = Helpers.LoadPoints(filePoints);
            var map = Helpers.LoadGrid(fileGrid);
            // В каких узлах считаем
            var mask = Helpers.AllNodes(map);
            // Метод интерполяции
            var interpolator = new KrigingInterpolator();
            // Сама интерполяция, сейчас параметры специфичные не определены
            if (interpolator.Interpolate(map, points, mask, new KrigingInterpolationOptions()))
            {
                // Необходимо реалиозовать сохранение
                Helpers.SaveMap(fileMap, map);
            }
        }
    }
}