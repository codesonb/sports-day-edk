using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Data.Piers;
using EDKv5.Protocols;

namespace EDKv5.MonitorServices
{
    public class DataEntryStation
    {
        const int C_DELAY = 500;

        //constructor
        // --- public no-argument constructor

        //field
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Pier pier;
        int _d_timeout = 20000;

        //properties
        public int Port { get; set; } = AppSettings.D_PORT;
        public int Timeout
        {
            get { return _d_timeout; }
            set { _d_timeout = value < 100 ? 100 : value; }
        }
        public CancellationToken CancellationToken { get { return tokenSource.Token; } }

        //function
        #region Cancellation
        public void Cancel()
        {
            if (null == this.pier)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
            } else {
                throw new InvalidOperationException("Cannot cancel after connection established, please use ");
            }
        }
        #endregion


        #region Commands
        public void RequestOutline()
        {
            RequestTodayEventOutlinesCommand cmd = new RequestTodayEventOutlinesCommand();
            pier.Send(cmd);
        }
        public void RequestCompetition(int id)
        {
            //create command
            RequestCompetitionCommand cmd = new RequestCompetitionCommand(id);
            pier.Send(cmd);
        }
        public void RequestUpdateResults(int id, CompetitionResult[] results)
        {
            UpdateResultCommand cmd = new UpdateResultCommand(id, results);
            pier.Send(cmd);
        }

        #endregion


        #region Server Discovery
        public bool SeekServer()
        {
            // if connected
            if (null != pier)
                throw new Exception("Already connected to monitor station.");

            // copy reference, fix ref for loop
            CancellationTokenSource _t_tokenSource = this.tokenSource;

            //create temporary TCP listener for income connection from server
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            try
            {
                listener.Start();

                //generate nonce
                Random r = new Random();
                int nonce = r.Next(int.MinValue, int.MaxValue);

                //send request
                Task.Factory.StartNew(() => yield(nonce), _t_tokenSource.Token);

                //hash for check
                SHA256 sha = SHA256.Create();
                byte[] hashedNonce = sha.ComputeHash(BitConverter.GetBytes(nonce));

                //wait for response
                int waitCount = _d_timeout; //copy config
                while (!(waitCount < 0 || _t_tokenSource.IsCancellationRequested))
                {
                    if (listener.Pending())
                    {
                        TcpClient client = listener.AcceptTcpClient();

                        //check 
                        BinaryReader br = new BinaryReader(client.GetStream());
                        int len = br.ReadInt32();
                        byte[] bys = br.ReadBytes(len);

                        //check hash
                        if (!hashedNonce.SequenceEqual(bys))
                        {   //failed checking
                            client.Close();
                            continue;
                        }

                        //create pier
                        pier = new Pier(client);
                        Task.Factory.StartNew(do_receiveData, tokenSource.Token);
                        return true;

                    }
                    else
                    {
                        Thread.Sleep(C_DELAY);
                        waitCount -= C_DELAY;
                    }
                }

                //timeout handle
                if (waitCount < 0)
                    throw new TimeoutException();

                //cancel handle
                // - to finally ensure listener is disposed
                return false;
            }
            catch (Exception ex)
            {
                //re-throw
                throw ex;
            }
            finally
            {
                // make sure listener is disposed
                listener.Stop();
                GC.Collect(GC.GetGeneration(listener), GCCollectionMode.Forced);
                listener = null;
            }
        }

        //----
        private void yield(int nonce)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.Write(nonce);
                bw.Write(Port);

                //create temporary UDP client
                using (UdpClient udp = new UdpClient())
                {
                    byte[] buffer = ms.GetBuffer();
                    udp.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, AppSettings.D_PORT));
                }
            }
        }

        //----
        private void do_receiveData()
        {
            do
            {
                while (pier.Receive()) ;
                Thread.Sleep(C_DELAY);
            } while (!tokenSource.IsCancellationRequested);
        }
        #endregion

        #region Destruction
        ~DataEntryStation()
        {
            if (null != tokenSource)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = null;
            }
        }
        #endregion

    }
}
