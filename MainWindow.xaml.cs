using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BeehiveManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 




    interface IDefend
    {
        void Defend();
    }


    class NectarDefender : NectarCollector, IDefend
    {
        public void Defend()
        {

        }

    }


    static class HoneyVault
    {
        public const float NECTAR_CONVERSION_RATIO = 19f;
        public const float LOW_LEVEL_WARNING = 10f;
        private static float honey = 25f;
        private static float nectar = 100f;
        
        public static void ConvertNectarToHoney(float amount)
        {
            
            float nectarToConvert = amount;
            if (nectarToConvert > nectar) nectarToConvert = nectar;
            nectar -= nectarToConvert;
            nectar += nectarToConvert * NECTAR_CONVERSION_RATIO;
            
        }

        public static bool ConsumeHoney(float amount)
        {
            if(honey >= amount)
            {
                honey -= amount;
                return true;
            }
            return false;
        }

        public static void CollectNectar(float amount)
        {
            if (amount > 0f) nectar += amount;
        }

        public static string StatusReport
        {
            get
            {
                string status = $"{honey:0.0} units of honey\n" + $"{nectar:0.0} units of nectar";
                string warnings = "";
                if (honey < LOW_LEVEL_WARNING) warnings += "\nLOW HONEY - ADD A HONEY MANUFACTUER";
                if (nectar < LOW_LEVEL_WARNING)
                {
                    warnings += "\nLOW HONEY - ADD A HONEY MANUFACTUER";
                }
                return status + warnings;
            }
        }
    }


    interface IWorker
    {
        string Job { get; }
        void WorkTheNextShift();
    }

    class Bee : IWorker
    {
        public virtual float CostPerShift { get; }
        public string Job
        {
            get;
            private set;
        }

        public Bee(string job)
        {
            Job = job;
        }
        
        public void WorkTheNextShift() 
        {
            if(HoneyVault.ConsumeHoney(CostPerShift))
            {
                DoJob();
            }
        }

       protected virtual void DoJob() { }
       
    }

    class NectarCollector : Bee
    {
        public const float NECTAR_COLLECTED_PER_SHIFT = 33.25f;
        public override float CostPerShift { get { return 1.95f; } }
        public NectarCollector() : base("Nectar Collector") { }


        protected override void DoJob()
        {
            HoneyVault.CollectNectar(NECTAR_COLLECTED_PER_SHIFT);
        }
    }

    class HoneyManufacture : Bee
    {
        public const float NECTAR_COLLECTED_PER_SHIFT = 33.15f;
        public override float CostPerShift { get { return 1.7f; } }
        public HoneyManufacture() : base("Honey Manufacture") { }

        protected override void DoJob()
        {
            HoneyVault.ConvertNectarToHoney(NECTAR_COLLECTED_PER_SHIFT);
        }

    }


    class Queen : Bee, INotifyPropertyChanged
    {
        public const float EGGS_PER_SHIFT = 0.45f;
        public const float HONET_PER_ASSIGNED_WORKER = 0.5f;
        
        private IWorker[] workers = new IWorker[0];
        private float eggs = 0;
        private float unassignedWorkers = 5;

        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusReport { get; private set; }
        public override float CostPerShift { get { return 2.15f; } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Queen() : base("Queen")
        {
            AssignBee("Honey Manufacture");
            AssignBee("Nectar Collector");
            AssignBee("Egg Care");

        }

        private void AddWorker(Bee worker)
        {
            if(unassignedWorkers >= 1)
            {
                unassignedWorkers--;
                Array.Resize(ref workers, workers.Length + 1);
                workers[workers.Length - 1] = worker;
            }
        }


        private void UpdateStatusReport()
        {
            StatusReport = $"Vault report:\n{HoneyVault.StatusReport}\n" +
            $"\nEgg count: {eggs:0.0}\nUnassigned workers: {unassignedWorkers:0.0}\n" +
            $"{WorkerStatus("Nectar Collector")}\n{WorkerStatus("Honey Manufacturer")}" +
            $"\n{WorkerStatus("Egg Care")}\nTOTAL WORKERS: {workers.Length}";
            OnPropertyChanged("StatusReport");
        }


        public void CareForEggs(float eggsToConvert)
        {
            if (eggs >= eggsToConvert)
            {
                eggs -= eggsToConvert;
                unassignedWorkers += eggsToConvert;
            }


        }

        private string WorkerStatus(string job)
        {
            int count = 0;
            foreach (IWorker worker in workers)
                if (worker.Job == job) count++;
            string s = "s";
            if (count == 1) s = " ";
            return $"{count} {job} bee{s}";

        }

        public void AssignBee(string job)
        {
            switch(job)
            {
                case "Honey Manufacture":
                    AddWorker(new HoneyManufacture());
                    break;
                case "Nectar Collector":
                    AddWorker(new NectarCollector());
                    break;
                case "Egg Care":
                    AddWorker(new EggCare(this));
                    break;
            }
            UpdateStatusReport();
        }
     


        protected override void DoJob()
        {
            eggs += EGGS_PER_SHIFT;
            foreach(IWorker worker in workers)
            {
                worker.WorkTheNextShift();
               
                
            }
            HoneyVault.ConsumeHoney(unassignedWorkers * HONET_PER_ASSIGNED_WORKER);
            UpdateStatusReport();

        }

    }






    class EggCare : Bee
    {
        public const float CARE_PROGRESS_PER_SHIFT = 0.15f;
        public override float CostPerShift { get { return 1.35f; } }
        private Queen queen;
        public EggCare(Queen queen) : base("EggCare") 
        {
            this.queen = queen;
        }

        protected override void DoJob()
        {
            queen.CareForEggs(CARE_PROGRESS_PER_SHIFT);
        }
       
    }
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private readonly Queen queen;
        public MainWindow()
        {
            InitializeComponent();
            queen = Resources["queen"] as Queen;
            //StatusReport.Text = queen.StatusReport;
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1.5);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            WorkShift_Click(this, new RoutedEventArgs());
        }

        private void Assignjob_Click(object sender, RoutedEventArgs e)
        {
            queen.AssignBee(jobSelector.Text);
            //StatusReport.Text = queen.StatusReport;

        }

        private void WorkShift_Click(object sender, RoutedEventArgs e)
        {
            
            queen.WorkTheNextShift();
            //StatusReport.Text = queen.StatusReport;
        }
    }
}
