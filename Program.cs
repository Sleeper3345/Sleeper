using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pure_csharp
{
    class Server
    {
        TcpListener Listener; // Объект, принимающий TCP-клиентов
        int port;
        int jid;
        List<JobID> jobId = new List<JobID>();
        int jobIdCount = 1;
        static object LockObject = new object();
        static volatile int starterCount = 0;
        static ManualResetEvent startEvent = new ManualResetEvent(false);

        // Запуск сервера
        public Server(int Port)
        {
            port = Port;
        }

        public void AddJobID()
        {
            
        }

        public void Start()
        {
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start(); // Запускаем его
            //Client client = new Client(Listener.AcceptTcpClient());

            // В бесконечном цикле
            while (true)
            {
                Client client = new Client(Listener.AcceptTcpClient());
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
                if (client.GetSecondParam() != null)
                {
                    JobID jId = new JobID(jobIdCount, client.GetFirstParam(), client.GetSecondParam());
                    jobIdCount++;
                    jobId.Add(jId);
                }
                if (client.GetSecondParam() == null)
                {
                    jid = client.GetFirstParam();
                }
                Thread.Sleep(1000);
                Worker worker = new Worker(jobId, jid);
                Thread worker_thread = new Thread(worker.Start);
                worker_thread.Start();
            }
        }

        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }

        static void Main(string[] args)
        {
            Server server = new Server(8888);
            Thread thread = new Thread(server.Start);
            thread.Start();
            int MaxThreadsCount = Environment.ProcessorCount * 4;
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            ThreadPool.SetMinThreads(2, 2);
        }

        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }
        static void Starting(object paramThread)
        {
            lock (LockObject)
            {
                starterCount++;
            }
            startEvent.WaitOne();
            (paramThread as Thread).Start();
        }
    }
}
