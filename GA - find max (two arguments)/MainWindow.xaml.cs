using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using WPFSurfacePlot3D;
using static _3D_Chart_WPF_v1.GeneticAlgorithm;


namespace _3D_Chart_WPF_v1
{

    public partial class MainWindow : Window
    {
        SurfacePlotModel mySurfacePlotModel;
        double i = -1;

        System.Windows.Threading.DispatcherTimer timer;
        bool showBestPosition;
        GA GA;
        double minX, maxX;
        int generation;
        double bestX, bestY, fitness;

        public MainWindow()
        {
            InitializeComponent();

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerMainTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            Init();
        }

        void Init()
        {
            showBestPosition = false;

            mySurfacePlotModel = new SurfacePlotModel();
            mySurfacePlotView.DataContext = mySurfacePlotModel;
            mySurfacePlotModel.Title = "";

            Func<double, double, double> sampleFunction = (x, y) => x * y;
            //sampleFunction = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
            //sampleFunction = (x, y) => Math.Sqrt(x * x) + Math.Sqrt(y * y);
            //sampleFunction = (x, y) => (x * x  - 5 * Math.Cos(2 * Math.PI * x)) + (y * y - 5 * Math.Cos(2 * Math.PI * y));  //(x ^ 2 - 10 * cos(2 * PI * x)) + (y ^ 2 - 10 * cos(2 * PI * y)) + 20
            //sampleFunction = (x, y) => (1 - x) * (1 - x) + 100 * (y - x * x) * (y - x * x);
            sampleFunction = (x, y) => -1 * ((Math.Sin(x) * Math.Pow(Math.Sin((1 * x * x) / Math.PI), 20) + (Math.Sin(y) * Math.Pow(Math.Sin((2 * y * y) / Math.PI), 20))));
            //sampleFunction = (x, y) => 2 * x * y * (1 - x) * (1 - y) * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y);
            mySurfacePlotModel.PlotFunction(sampleFunction, -4, 4, -4, 4);

            minX = -4;
            maxX = 4;

            //
            generation = 100;
            GA = new GeneticAlgorithm.GA(0.8, 0.05, 50, generation, 2, false, minX, maxX, sampleFunction);
            GA.Elitism = true;
            GA.TimerNotify += () => { timer.Stop(); btnStart.Content = "Start timer"; };
            GA.BestPositionNotify += (array) => {
                if (array.Length != 3) return;

                bestX = array[0];
                bestY = array[1];
                fitness = array[2];

                showBestPosition = true;

                rtbConsole.AppendText("\r\rBest solution found, x = " + bestX.ToString("F4") + ", y = " + bestY.ToString("F4"));
                rtbConsole.AppendText("\rFitness, z = " + fitness.ToString("F4"));
                rtbConsole.AppendText("\r\r******* END *******");
            };

            rtbConsole.Clear();

            rtbConsole.AppendText("\rBegin GENETIC ALGORITHM demo.");
            rtbConsole.AppendText("\r\rGoal is to find MAXIMUM of function for 2 variables.");
            rtbConsole.AppendText("\r(x, y) => -1 * ((Math.Sin(x) * Math.Pow(Math.Sin((1 * x * x) / Math.PI), 20) + (Math.Sin(y) * Math.Pow(Math.Sin((2 * y * y) / Math.PI), 20))))");

            rtbConsole.AppendText("\r\rSetting Crossover rate to " + GA.CrossoverRate);
            rtbConsole.AppendText("\rSetting Mutation rate to " + GA.MutationRate);
            rtbConsole.AppendText("\rSetting Population size to " + GA.PopulationSize);
            rtbConsole.AppendText("\rSetting Generation size to " + GA.GenerationSize);
            rtbConsole.AppendText("\rSetting Chromosome length to " + GA.ChromosomeLength);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Обновление положения всех точек
            var points = new CustomPoint3DCollection();

            // Создаем точки, вращающиеся вокруг оси Z
            points.Add(new Point3D(2 * Math.Cos(i), 2 * Math.Sin(i), 1));
            points.Add(new Point3D(2 * Math.Cos(i + Math.PI), 2 * Math.Sin(i + Math.PI), -1));
            points.Add(new Point3D(1 * Math.Cos(i / 2), 1 * Math.Sin(i / 2), 0.5));
            points.Add(new Point3D(1.5 * Math.Cos(i + Math.PI / 2), 1.5 * Math.Sin(i + Math.PI / 2), -0.5));

            mySurfacePlotModel.Points = points;
            i += 0.2;
        }

        private void Drawing()
        {
            // Обновление положения всех точек
            var points = new CustomPoint3DCollection();

            // Draw points
            for (int i = 0; i < GA.CurrentGenerationList.Count; ++i)
            {
                var chromosome = (Chromosome)GA.CurrentGenerationList[i];
                var X = chromosome.ChromosomeGenes[0]; // X
                var Y = chromosome.ChromosomeGenes[1]; // Y
                var Z = chromosome.ChromosomeFitness;  // Z = fitness value

                points.Add(new Point3D(X, Y, Z));
            }

            mySurfacePlotModel.Points = points;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                Init();
                timer.Start();
                btnStart.Content = "Stop timer";
            }
            else
            {
                timer.Stop();
                btnStart.Content = "Start timer";
            }
        }

        private void timerMainTick(object sender, EventArgs e)
        {
            GA.Calculation();
            lbEpoch.Content = GA.gen + " / " + generation;

            Drawing();
        }
    }
}