using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pure_csharp
{
    public class Worker
    {
        Mutex mutexObj = new Mutex();
        Mutex mutexObj2 = new Mutex();
        public Worker(List<JobID> jobIDs, int JId)
        {
            JobIDs = jobIDs;
            num = JId;
        }
        public void Start()
        {
            while (true)
            {
                if ((JobIDs.Any()) && (num > 0))
                {              
                    found = JobIDs.Where(m => m.Id == num).SingleOrDefault();
                    if (found == null)
                        CreateJson(0);
                    var webClient = new WebClient();
                    // Выполняем запрос по адресу и получаем ответ в виде строки
                    var response = webClient.DownloadString(found.Par2);
                    File.WriteAllText("temp" + num.ToString() + ".txt", response);
                    file = new System.IO.StreamReader(@"temp" + num.ToString() + ".txt");
                    Thread.Sleep(1000);
                    Thread readfile_thread = new Thread(ReadFile);
                    mutexObj.WaitOne();
                    readfile_thread.Start();
                    mutexObj.ReleaseMutex();
                }
            }
        }
        private void CreateJson(int p)
        {
            string t = "";
            if (p == 0)
            {
                t = "{\r\n\"state\": \"eexists\",\r\n\"data\": []\r\n}";
                File.WriteAllText("response" + num.ToString() + ".eexists" + ".json", t);
            }
            if (p == 1)
            {
                t = "{\r\n\"state\": \"progress\",\r\n\"data\": []\r\n}";
                File.WriteAllText("response" + num.ToString() + ".progress" + ".json", t);
            }
            if (p == 2)
            {
                t = "{\r\n\"state\": \"queued\",\r\n\"data\": []\r\n}";
                File.WriteAllText("response" + num.ToString() + ".queued" + ".json", t);
            }
            if (p == 3)
            {
                t = "{\r\n\"state\": \"ready\",\r\n\"data\": [" + io + "]\r\n}";
                File.WriteAllText("response" + num.ToString() + ".ready" + ".json", t);
                arrs.Clear();
                sortarrs.Clear();
                io = "";
            }
        }
        private void ReadFile()
        {
            while (true)
            {
                while ((line = file.ReadLine()) != null)
                {
                    numbers += line;
                }
                string[] symbols = numbers.Split(',');
                int[] arr = symbols.Select(ch => int.Parse(ch.ToString())).ToArray();
                int MN = arr.GetLength(0) / found.Par1;
                int[] temp_arr;
                Thread[] threads = new Thread[found.Par1];
                arrs = new List<int[]>();
                long f = 0;
                    while (f < arr.GetLength(0))
                    {
                        if (f + MN <= arr.GetLength(0))
                        {
                            temp_arr = new int[MN];
                            for (long i = f; i < f + MN; i++)
                            {
                                temp_arr[i - f] = arr[i];
                            }
                            f += MN;
                        }
                        else
                        {
                            temp_arr = new int[arr.GetLength(0) - f];
                            for (long i = f; i < arr.GetLength(0); i++)
                            {
                                temp_arr[i - f] = arr[i];
                            }
                            f += MN;
                        }
                    arrs.Add(temp_arr);
                }
                for (long i = 0; i < found.Par1; i++)
                {
                    mutexObj2.WaitOne();
                    int[] tempmas = arrs.ElementAt(Convert.ToInt32(i));
                    threads[i] = new Thread(new ParameterizedThreadStart(new ParameterizedThreadStart((x) =>
                    {
                        SortPart(tempmas);
                    })));
                    threads[i].Start();
                    mutexObj2.ReleaseMutex();
                }
                CreateJson(2);
                for (long i = 0; i < found.Par1; i++)
                    arr = Merge(arr, arrs.ElementAt(Convert.ToInt32(i)));
                CreateJson(1);
                io = string.Join(", ", arr);
                File.WriteAllText("sort" + num.ToString() + ".txt", io);
                CreateJson(3);
                Thread.Sleep(1000);
            }
        }
        private void SortPart(int[] part)
        {
            while (true)
            {
                quickSort(part, 0, part.GetLength(0) - 1);
                Thread.Sleep(1000);
                sortarrs.Add(part);
            }
        }
        private int[] Merge(int[] A, int[] B)
        {
            int[] a = A;
            int[] b = B;
            int[] result = new int[A.GetLength(0) + B.GetLength(0)];
            int i = 0, j = 0;
            int index = 0;
            while (i < A.GetLength(0) && j < B.GetLength(0))
            {
                if (a[i] < b[j])
                {
                    result[index] = a[i];
                    i++;
                }
                else
                {
                    result[index] = b[j];
                    j++;
                }

                index++;
            }
            while (i < A.GetLength(0))
            {
                result[index] = a[i];
                index++;
                i++;
            }

            while (j < B.GetLength(0))
            {
                result[index] = b[j];
                index++;
                j++;
            }
            return result;
        }
        private void quickSort(int[] a, long l, long r)
        {
            int temp;
            int x = a[l + (r - l) / 2];
            long i = l;
            long j = r;
                while (i <= j)
                {
                    while (a[i] < x) i++;
                    while (a[j] > x) j--;
                    if (i <= j)
                    {
                        temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                        i++;
                        j--;
                    }
                }
                if (i < r)
                    quickSort(a, i, r);

                if (l < j)
                    quickSort(a, l, j);       
        }

        private List<JobID> JobIDs;
        private JobID found;
        private int num;
        private int[] arr = new int[0];
        string io;
        private List<int[]> arrs;
        private List<int[]> sortarrs = new List<int[]>();
        private string line, numbers = "";
        private System.IO.StreamReader file;
    }
}
