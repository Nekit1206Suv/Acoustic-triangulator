using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Triangulator
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _sideLength = 20;
        private double _el1 = 30, _az1 = 0;
        private double _el2 = 30, _az2 = 0;
        private double _el3 = 30, _az3 = 0;
        private TriangulationResult _result;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainViewModel() => Recalculate();

        public void Recalculate()
        {
            Result = TriangulationMath.CalculatePosition(SideLength, El1, Az1, El2, Az2, El3, Az3);
        }

        public TriangulationResult Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
        }

        public double SideLength { get => _sideLength; set { _sideLength = value; OnPropertyChanged(); Recalculate(); } }

        public double El1 { get => _el1; set { _el1 = value; OnPropertyChanged(); Recalculate(); } }
        public double Az1 { get => _az1; set { _az1 = value; OnPropertyChanged(); Recalculate(); } }

        public double El2 { get => _el2; set { _el2 = value; OnPropertyChanged(); Recalculate(); } }
        public double Az2 { get => _az2; set { _az2 = value; OnPropertyChanged(); Recalculate(); } }

        public double El3 { get => _el3; set { _el3 = value; OnPropertyChanged(); Recalculate(); } }
        public double Az3 { get => _az3; set { _az3 = value; OnPropertyChanged(); Recalculate(); } }
    }
}