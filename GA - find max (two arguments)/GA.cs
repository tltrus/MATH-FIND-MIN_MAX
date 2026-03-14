using System;
using System.Collections;


namespace _3D_Chart_WPF_v1
{
    // Code from book "Numerical Methods, Algorithms and Tools in C#", Waldemar Dos Passos, CRC Press 2010.
    internal class GeneticAlgorithm
    {
        private static Random rand = new Random();

        /// <summary>
        /// Класс Генетического Алгоритма
        /// </summary>
        public class GA
        {
            private double minX;
            private double maxX;

            public double MutationRate;     // уровень мутации
            public double CrossoverRate;    // уровень скрещивания
            public int ChromosomeLength;    // длина хромосомы
            public int PopulationSize;      // размер популяции
            public int GenerationSize;      // размер поколений
            private bool SelectionMethod; // Метод отбора. 0 - рулетка, 1 - турнир

            public double TotalFitness = 0;
            public bool Elitism;
            public ArrayList CurrentGenerationList;
            private ArrayList NextGenerationList;
            private ArrayList FitnessList;

            public delegate void StopHandler();
            public event StopHandler TimerNotify;
            public delegate void bestPositionHandler(double[] bestPosition);
            public event bestPositionHandler BestPositionNotify;

            Func<double, double, double> F;
            public int gen;

            // Мои поля
            public ArrayList FitnessChartList;

            //Конструктор с параметрами: уровень скрещивания, уровень мутации,
            //размер популяции, размер поколений, длина хромосомы, метод отбора
            public GA(double XoverRate, double mutRate, int popSize, int genSize, int ChromLength, bool SelMethod, double minX, double maxX, Func<double, double, double> func)
            {
                Elitism = false;
                MutationRate = mutRate;
                CrossoverRate = XoverRate;
                PopulationSize = popSize;
                GenerationSize = genSize;
                ChromosomeLength = ChromLength;
                SelectionMethod = SelMethod; // Метод отбора. 0 - рулетка, 1 - турнир

                this.minX = minX;
                this.maxX = maxX;
                F = func;

                Init();
            }

            private void Init()
            {
                gen = 0;
                
                //Создаются списки для хранения фитнеса, текущего и следующего поколений 
                FitnessList = new ArrayList();
                CurrentGenerationList = new ArrayList(GenerationSize);
                NextGenerationList = new ArrayList(GenerationSize);
                //инициализация уровня мутации
                Chromosome.ChromosomeMutationRate = MutationRate;
                FitnessChartList = new ArrayList();

                //Создание популяции
                for (int i = 0; i < PopulationSize; i++)
                {
                    Chromosome g = new Chromosome(ChromosomeLength, true, minX, maxX); // Создается новая хромосома с нужной длиной
                    CurrentGenerationList.Add(g);   // добавляем хромосомы в текущий список
                }

                //Ранжирование созданной популяции
                RankPopulation();
            }

            public void Calculation()
            {
                if (gen < GenerationSize)
                {
                    CreateNextGeneration();
                    RankPopulation();

                    ++gen;
                }
                else
                {
                    var bestPosition = GetBestValues();
                    
                    BestPositionNotify?.Invoke(bestPosition);
                    TimerNotify?.Invoke();
                }
            }

            //Метод Рулетки.
            private int RouletteSelection()
            {
                double randomFitness = rand.NextDouble() * TotalFitness;
                int idx = -1;
                int mid;
                int first = 0;
                int last = PopulationSize - 1;
                mid = (last - first) / 2;
                while (idx == -1 && first <= last)
                {
                    if (randomFitness < (double)FitnessList[mid])
                    { last = mid; }
                    else if (randomFitness > (double)FitnessList[mid])
                    { first = mid; }
                    mid = (first + last) / 2;
                    if ((last - first) == 1) idx = last;
                }
                return idx;
            }

            //Метод Турнира
            private int TournamentSelection()
            {
                int idx1 = 0;
                int idx2 = 0;
                int idx = 0;
                var g = CurrentGenerationList;

                int rnd11 = rand.Next(0, (g.Count - 1) / 2);
                int rnd12 = rand.Next(0, (g.Count - 1) / 2);

                int rnd21 = rand.Next(g.Count - g.Count / 2, g.Count - 1);
                int rnd22 = rand.Next(g.Count - g.Count / 2, g.Count - 1);

                // Первый полуфинал
                double a = ((Chromosome)g[rnd11]).ChromosomeFitness;
                double b = ((Chromosome)g[rnd12]).ChromosomeFitness;
                if (a > b) idx1 = rnd11; else idx1 = rnd12;

                // Второй полуфинал
                a = ((Chromosome)g[rnd21]).ChromosomeFitness;
                b = ((Chromosome)g[rnd22]).ChromosomeFitness;
                if (a > b) idx2 = rnd21; else idx2 = rnd22;

                // Финал
                a = ((Chromosome)g[idx1]).ChromosomeFitness;
                b = ((Chromosome)g[idx2]).ChromosomeFitness;
                if (a > b) idx = idx1; else idx = idx2;

                return idx;
            }

            // Ранжирование популяции хромосом
            // Рассчитывается фитнес функция и делается сортировка
            private void RankPopulation()
            {
                TotalFitness = 0;

                for (int i = 0; i < PopulationSize; i++)
                {
                    Chromosome g = (Chromosome)CurrentGenerationList[i];
                    g.ChromosomeFitness = F(g.ChromosomeGenes[0], g.ChromosomeGenes[1]); // Вызов ранее привязанной функции-делегата с параметром double
                    TotalFitness += g.ChromosomeFitness; // Общее значение всех фитнесов
                }

                // Сортировка =====================================
                CurrentGenerationList.Sort(new ChromosomeComparer()); // Используется интерфейс IComparer

                // Заполнение списка FitnessList
                // На каждой итерации идет складывание предыдущего значения фитнеса.
                // Это нужно для того, чтобы сформировать отрезки для Метода рулетки.
                // Для справки: TotalFitness и последний элемент FitnessList равны.
                double fitness = 0.0;
                FitnessList.Clear();
                for (int i = 0; i < PopulationSize; i++)
                {
                    fitness += ((Chromosome)CurrentGenerationList[i]).ChromosomeFitness;
                    FitnessList.Add((double)fitness);
                }

                // Для вывода графика на экран
                FitnessChartList.Add(((Chromosome)CurrentGenerationList[PopulationSize - 1]).ChromosomeFitness);
            }

            //Создание нового поколения хромосом. 
            private void CreateNextGeneration()
            {
                NextGenerationList.Clear();
                Chromosome g = null;

                // Если флаг Элитизма = 1, то копируем последнюю максимальную хромосому из ранее отсортированного списка.
                // Затем выполняем скрещивание и мутацию.
                // В конце, если флаг = 1, копируем лучшую хромосому в новую популяцию
                if (Elitism)
                    g = (Chromosome)CurrentGenerationList[PopulationSize - 1];

                int pidx1; int pidx2;

                for (int i = 0; i < PopulationSize; i += 2)
                {
                    if (!SelectionMethod)
                    {
                        pidx1 = RouletteSelection(); // Метод Рулетка
                        pidx2 = RouletteSelection(); // Метод Рулетка
                    }
                    else
                    {
                        pidx1 = TournamentSelection(); // Метод Турнира
                        pidx2 = TournamentSelection(); // Метод Турнира
                    }

                    Chromosome parent1, parent2, child1, child2;
                    parent1 = ((Chromosome)CurrentGenerationList[pidx1]);
                    parent2 = ((Chromosome)CurrentGenerationList[pidx2]);

                    if (rand.NextDouble() < CrossoverRate) // Сравниваем с уровнем скрещивания
                    { parent1.Crossover(ref parent2, out child1, out child2); } // если скрещивать разрешено
                    else
                    {
                        child1 = parent1;
                        child2 = parent2;
                    }
                    child1.Mutate(); // Мутация
                    child2.Mutate(); // Мутация
                    NextGenerationList.Add(child1);
                    NextGenerationList.Add(child2);
                }

                if (Elitism && g != null) NextGenerationList[0] = g;
                CurrentGenerationList.Clear();
                for (int i = 0; i < PopulationSize; i++)
                    CurrentGenerationList.Add(NextGenerationList[i]);
            }

            //Извлечение лучшего значения, основанного на фитнесе
            //Список уже отсортирован, поэтому достаточно взять нужный (последний) элемент
            public double[] GetBestValues()
            {
                Chromosome g = ((Chromosome)CurrentGenerationList[PopulationSize - 1]);
                double[] values = new double[g.ChromosomeLength + 1];   // подготавливаем пустую переменную [x, y, fitness:Z]
                values = g.ExtractChromosomeValues();                   // извлекаем значение х и у

                var fitness = g.ChromosomeFitness;
                values[values.Length - 1] = fitness;                    // последний элемент - fitness

                return values;
            }
        }

        /// <summary>
        /// Класс Хромосома
        /// </summary>
        public class Chromosome
        {
            public double[] ChromosomeGenes;
            public int ChromosomeLength;
            public double ChromosomeFitness;
            public static double ChromosomeMutationRate;
            double minX, maxX;

            //Конструктор Хромосомы
            //На входе: длина хромосомы и флаг.
            //Флаг 0 - Создается пустая хромосома. Используется при скрещивании
            //Флаг 1 - Хросомосома заполняется случайными генами. Используется при инициализации хромосом
            public Chromosome(int length, bool createGenes, double minX, double maxX)
            {
                this.minX = minX;
                this.maxX = maxX;

                ChromosomeLength = length;
                ChromosomeGenes = new double[length];
                if (createGenes)    // Флаг 0 или 1
                {
                    for (int i = 0; i < ChromosomeLength; i++)
                        ChromosomeGenes[i] = (maxX - minX) * rand.NextDouble() + minX; // создаем случайные переменные x и y
                }
            }

            //Скрещивание
            //Сначала случайно определяется позиция (индекс) генома в массиве (0 или 1).
            //Создаются два потомка.
            //Взависимости от индекса позиции выбирается вариант скрещивания.
            public void Crossover(ref Chromosome Chromosome2, out Chromosome child1, out Chromosome child2)
            {
                //int position = (int)(rand.NextDouble() * (double)ChromosomeLength); 
                int position = (int)(((maxX - minX) * rand.NextDouble() + minX) * (double)ChromosomeLength); // (maxX - minX) * rand.NextDouble() + minX; // создаем случайные переменные x и y
                child1 = new Chromosome(ChromosomeLength, false, minX, maxX);
                child2 = new Chromosome(ChromosomeLength, false, minX, maxX);
                for (int i = 0; i < ChromosomeLength; i++)
                {
                    if (i < position)
                    {
                        child1.ChromosomeGenes[i] = ChromosomeGenes[i];
                        child2.ChromosomeGenes[i] = Chromosome2.ChromosomeGenes[i];
                    }
                    else
                    {   // крест на крест
                        child1.ChromosomeGenes[i] = Chromosome2.ChromosomeGenes[i];
                        child2.ChromosomeGenes[i] = ChromosomeGenes[i];
                    }
                }
            }

            //Мутация генов хромосомы
            public void Mutate()
            {
                for (int position = 0; position < ChromosomeLength; position++)
                {
                    if (rand.NextDouble() < ChromosomeMutationRate) // мутация 0.05
                    {
                        double gendo = ChromosomeGenes[position];
                        double rnd = (maxX - minX) * rand.NextDouble() + minX; // создаем случайные переменные x и y  rand.NextDouble();
                        ChromosomeGenes[position] = (ChromosomeGenes[position] + rnd) / 2.0;
                        double gen = ChromosomeGenes[position];
                    }

                }
            }

            //Извлечение значения хромосомы
            public double[] ExtractChromosomeValues()
            {
                double[] values = new double[ChromosomeLength + 1];

                for (int i = 0; i < ChromosomeLength; i++)
                    values[i] = ChromosomeGenes[i];

                return values;
            }
        }


        /// <summary>
        /// Сравниваются фитнесы двух хромосом
        /// </summary>
        private sealed class ChromosomeComparer : IComparer
        {
            public int Compare(object x, object y) // x - первый по порядку член популяции; y - следующий за ним член
            {
                if (!(x is Chromosome) || !(y is Chromosome))
                    throw new ArgumentException("Not of type Chromosome");
                if (((Chromosome)x).ChromosomeFitness > ((Chromosome)y).ChromosomeFitness)
                    return 1;
                else if (((Chromosome)x).ChromosomeFitness == ((Chromosome)y).ChromosomeFitness)
                    return 0;
                else
                    return -1;
            }
        }
    }
}
