using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Data.Piers;
using EDKv5.SchedulerService;

namespace EDKv5.MonitorServices
{
    public class MonitorStation
    {
        public event EventHandler ConnectionRequestReceived;
        public event EventHandler ConnectionEstabilished;
        public event EventHandler<MonitorEventArgs> TimeoutPassed;
        public event Action<string> CommandReceived;

        // const
        const int C_DELAY = 500;

        // constructor
        public MonitorStation(Project project)
        {
            this.project = project;
            // get today
            var days = project.Schedule.Days;
            var now = DateTime.Now;
            int i = -1;

            if (false)
            {
                now = new DateTime(2017, 2, 25);
            }


            while (++i < days.Length - 1)
            {
                if (days[i].Start > now)
                { break; }
            }
            this.Today = days[i];
        }

        // fields
        Project project;

        // - connections
        Semaphore semaphore = new Semaphore(0, 1);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        UdpClient udpListener;
        List<Pier> ls_clients = new List<Pier>();

        // properties
        public IScheduleDay Today { get; }

        // functions
        public void Start()
        {
            udpListener = new UdpClient(AppSettings.D_PORT);

            Task task = new Task(do_acceptClient, tokenSource.Token);
            task.Start();
        }

        #region Socket Operations
        private void do_acceptClient()
        {
            while(true)
            {
                if (udpListener.Available > 0)
                {
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, AppSettings.D_PORT);
                    byte[] bys = udpListener.Receive(ref endpoint);

                    if (process_ConnectionRequest(endpoint, bys))
                        onConnectionRequestReceived();
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Process the connection request get from UDP datagram
        /// </summary>
        /// <param name="bys">Datagram</param>
        /// <param name="token">Cancellation token of the monitor, for termination of client reader.</param>
        /// <returns></returns>
        private bool process_ConnectionRequest(IPEndPoint endpoint, byte[] bys)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bys))
                using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
                {
                    //read data
                    int nonce = br.ReadInt32();
                    endpoint.Port = br.ReadInt32();     //update TCP port, it may not be the same with UDP

                    //estabilish connection
                    TcpClient client = new TcpClient();
                    client.Connect(endpoint);

                    //hash nonce
                    BinaryWriter bw = new BinaryWriter(client.GetStream());
                    SHA256 sha = SHA256.Create();
                    byte[] hashedNonce = sha.ComputeHash(BitConverter.GetBytes(nonce));
                    bw.Write(hashedNonce.Length);
                    bw.Write(hashedNonce);

                    //push to client management
                    Pier monitorPier = new Pier(client);
                    ls_clients.Add(monitorPier);
                    monitorPier.CommandReceived += onCommandReceived;

                    //start / restart client processing thread
                    if (ls_clients.Count == 1)
                    {
                        Task task = new Task(do_receiveData, tokenSource.Token);
                        task.Start();
                    }
                    onConnectionEstabilished(monitorPier);
                }
                return true;
            } catch (Exception ex) {
                //throw ex;
            }
            return false;
        }

        private void do_receiveData()
        {
            do
            {
                //clone array reference for thread safe iteration
                Pier[] arr = ls_clients.ToArray();
                foreach (Pier client in arr)
                    client.Receive();

                Thread.Sleep(C_DELAY);
            } while (true);
        }
        #endregion

        #region Call Event Handlers
        protected void onConnectionRequestReceived()
        {
            if (null != this.ConnectionRequestReceived)
                ConnectionRequestReceived(this, EventArgs.Empty);
        }
        protected void onConnectionEstabilished(Pier dataEntryClient)
        {
            if (null != this.ConnectionEstabilished)
                ConnectionEstabilished(this, EventArgs.Empty);
        }
        protected void onTimeoutPassed(MonitorEventArgs e)
        {
            if (null != this.TimeoutPassed)
                TimeoutPassed(this, e);
        }
        protected void onCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            if (null != this.CommandReceived)
                this.CommandReceived(e.Command.GetType().Name);
        }
        #endregion

    }

}

