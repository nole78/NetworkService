using NetworkService.Persistance;
using NetworkService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        public NetworkEntitiesViewModel networkEntitiesViewModel;
        public NetworkDisplayViewModel networkDisplayViewModel;
        public MeasurementGraphViewModel measurementGraphViewModel;
        private BindableBase _currentViewModel;
        private readonly LoggerService _logger;
        private readonly MeasurementProcessingService _processor;

        #region Properties
        public BindableBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand UndoCommand { get; private set; }
        #endregion
        public MainWindowViewModel()
        {
            NavCommand = new MyICommand<string>(OnNavCommand);
            UndoCommand = new MyICommand(OnUndoCommand, CanUndo);

            AppDatabase.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppDatabase.Instance.LastAction))
                {
                    UndoCommand.RaiseCanExecuteChanged();
                }
            };

            networkEntitiesViewModel = new NetworkEntitiesViewModel();
            networkDisplayViewModel = new NetworkDisplayViewModel();
            measurementGraphViewModel = new MeasurementGraphViewModel();
            CurrentViewModel = networkDisplayViewModel;

            _logger = new LoggerService("log.txt");
            _processor = new MeasurementProcessingService(_logger);

            CreateListener(); //Povezivanje sa serverskom aplikacijom
        }

        private void CreateListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = Encoding.ASCII.GetBytes(AppDatabase.Instance.Resources.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji
                            _processor.ProcessMeasurement(incomming);
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        private void OnNavCommand(string destination)
        {
            switch (destination) 
            {
                case "entity":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "graph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;
                case "display":
                    CurrentViewModel = networkDisplayViewModel;
                    break;
            }
        }

        private void OnUndoCommand()
        {
            AppDatabase.Instance.Undo();
        }

        private bool CanUndo() => AppDatabase.Instance.LastAction != null;

    }
}
