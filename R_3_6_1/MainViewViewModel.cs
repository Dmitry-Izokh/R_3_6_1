using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_3_6_1
{
    public class MainViewViewModel
    {   // Свойства
        // Свойства с приставкой Selected берут информацию с MainView из ComboBox-SelectedValue.
        private ExternalCommandData _commandData;
        public List<MechanicalSystemType> SystemTypes { get; } = new List<MechanicalSystemType>();
        public MechanicalSystemType SelectedDuctSystemType { get; set; }
        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public DuctType SelectedDuctType { get; set; }
        public List<Level> Levels { get; } = new List<Level>();
        public Level SelectedLevel { get; set; }
        public double Hight { get; set; }
        public DelegateCommand SaveCommand { get; }
        public List<XYZ> Points { get; } = new List<XYZ>();

        // Конструктор
        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            SystemTypes = DuctUtils.GetSystemTipes(commandData);
            DuctTypes = DuctUtils.GetDuctTipes(commandData);
            Levels = LevelUtils.GetLevels(commandData);
            Hight = 1200;
            SaveCommand = new DelegateCommand(OnSaveCommand);
            Points = SelectionUtils.GetPoint(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }

        //событие создается для скрытие окна на время выбора.
        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }
        //событие создается для повторного открытия окна после отработки программы.
        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
        //событие создается для закрытия программы.
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        // Методы
        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Делаем проверку на количество выбранных точек.
            // Если точек меньше двух или не выбран тип воздуховодов
            // или не выбран уровень, тогда ничего не возвращаем и заканчиваем данный метод.
            if(Points.Count<2|| DuctTypes==null|| Levels==null)
            {
                return;
            }
            // Создаем переменную-лист points без определенного типа данных куда будем заносить точки.
            var points = new List<Point>();

            // Создаем переменные без определенного типа данных куда будем заносить координаты начальной и конечной точки.
            // К координате Z добавляем предустановленную высоту из конструктора.
            var startPoint = new XYZ(Points[0].X, Points[0].Y, Points[0].Z + Hight);
            var endPoint = new XYZ(Points[1].X, Points[1].Y, Points[1].Z + Hight);

            // Проводим цикл в котором проверяем индекс индекс точки
            // и продолжаем эту процедуру пока индекс равен 0.
            // Цикл проходит столько раз сколько у нас количество точек.
            // Заполняю ранее созданный пустой список точек.
            for(int i=0; i<Points.Count; i++)
            {
                if (i == 0)
                    continue;                
            }

            // создаем транзакцию, которая позволяет внести изменеия (создать воздуховод) в файле Revit.
            using (var ts = new Transaction(doc, "Создание воздуховода"))
            {
                ts.Start();
                Duct.CreatePlaceholder(doc,
                    SelectedDuctSystemType.Id,
                    SelectedDuctType.Id, SelectedLevel.Id,
                    startPoint,
                    endPoint);
                ts.Commit();
            }    
        }
    }

}
