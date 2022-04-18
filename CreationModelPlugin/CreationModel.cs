using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //доступ к документу Revit
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = GetLevels(doc, "Уровень 1");
            Level level2 = GetLevels(doc, "Уровень 2");


            //ширина
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            //глубина
            double deepth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            //получение набора точек
            double dx = width / 2;
            double dy = deepth / 2;

            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            {
                CreateWalls(dx, dy, level1, level2, doc);
            }
            transaction.Commit();

            return Result.Succeeded;
        }
        //метод выбора уровня
        public Level GetLevels(Document doc, string levelName)
        {
            //фильтр по уровням
            List<Level> listlevel = new FilteredElementCollector(doc)
                  .OfClass(typeof(Level))
                  .OfType<Level>()
                  .ToList();
            Level level = listlevel
                  .Where(x => x.Name.Equals(levelName))     //фильтр по имени уровня
                 .FirstOrDefault();
            return level;
        }
        //метод создания стен
        public List<Wall> CreateWalls(double dx, double dy, Level level1, Level level2, Document doc)
        {
            //массив, в который добавляем созданные стены
            List<Wall> walls = new List<Wall>();
            //коллекция с точками
            List<XYZ> points = new List<XYZ>();

            //цикл создания стен
            for (int i = 0; i < 4; i++)
            {
                points.Add(new XYZ(-dx, -dy, 0));
                points.Add(new XYZ(dx, -dy, 0));
                points.Add(new XYZ(dx, dy, 0));
                points.Add(new XYZ(-dx, dy, 0));
                points.Add(new XYZ(-dx, -dy, 0));
                //создание отрезка
                Line line = Line.CreateBound(points[i], points[i + 1]);
                //построение стены по отрезку
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                //находим высоту стены, привязывая ее к уровню
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
                //добавляем в список созданную стену
                walls.Add(wall);
            }
            return walls;
        }
    }
}



