using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library.Server
{
    /// <summary>
    /// Класс, отвечающий за работу сервера: прием сообщений от клиентов, передачу сообщений обратно.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Объект класса TcpListener, который прослушивает подключения от TCP-клиентов сети.
        /// </summary>
        TcpListener serverListener;
        /// <summary>
        /// Очередь доступных ID клиентов.
        /// </summary>
        public Queue<int> ClientsId = new Queue<int>();
        /// <summary>
        /// Число для индексации файла, присылаемого от клиента серверу.
        /// </summary>
        public int FileIndex = 0;
        /// <summary>
        /// Текущее количество клиентов.
        /// </summary>
        public int ClientsCnt=0;
        /// <summary>
        /// Максимальное количество клиентов.
        /// </summary>
        public int MaxConnectionsCnt = 3;
        /// <summary>
        /// Результат работы сервера в виде строки.
        /// </summary>
        public string res = "";
        /// <summary>
        /// Конструктор класса Server.
        /// </summary>
        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8081);
        }
        /// <summary>
        /// Метод для выключения прослушивания порта.
        /// </summary>
        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null ) 
                {
                    serverListener.Stop();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }
        /// <summary>
        /// Метод для включения прослушивания порта.
        /// </summary>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                {   
                    serverListener.Start();
                    for (int i = 1; i <= MaxConnectionsCnt; i++)
                    {
                        ClientsId.Enqueue(i);
                    }
                }
                
                while (true)
                {
                    OperationResult result = await ReceiveSomethingFromClient();
                    if (result.Result == Result.Fail)
                    {
                        res = "Unexpected error: " + result.Message;
                        Console.WriteLine(res);
                        SendMessageToClient(res);
                    }
                    else if (result.Result == Result.LimitFail || result.Result == Result.AddClient||result.Result == Result.OK)
                    {
                        res = result.Message;
                        Console.WriteLine(res);
                        SendMessageToClient(res);
                    }
                    else
                    {
                        res = result.Message;
                        Console.WriteLine(res);
                    } 
                }
            }
            catch (Exception e)
            {
                res = "Cannot turn on listener: " + e.Message;
                Console.WriteLine(res);
            }
        }
        /// <summary>
        /// Метод для присваивания новому клиенту следующего доступного ID из очереди ClientsId.
        /// </summary>
        /// <param name="stream">Текущий поток для передачи данных между клиентом и сервером.</param>
        /// <returns>Результат операции.</returns>
        public OperationResult SetClient(NetworkStream stream)
        {
            try
            {
                // Проверка доступности добавления нового клиента.
                if (ClientsCnt >= MaxConnectionsCnt)
                {
                    stream.WriteByte(0);
                    stream.Close();
                    return new OperationResult(Result.LimitFail, "Connection limit reached!");
                }
                Interlocked.Increment(ref ClientsCnt);
                int newid = ClientsId.Dequeue();
                // Новое значение ID записывается в поток для передачи клиенту.
                stream.WriteByte((byte)newid);
                stream.Close();
                return new OperationResult(Result.AddClient, "New client was added. ID=" + newid);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }

        }
        /// <summary>
        /// Метод для удаления клиента по его ID, т.е. добавления этого ID в очередь ClientsId.
        /// </summary>
        /// <param name="id">Идентификатор удаляемого клиента.</param>
        /// <returns>Результат операции.</returns>
        public OperationResult DeleteClient(int id)
        {
            try
            {
                ClientsId.Enqueue(id);
                Interlocked.Decrement(ref ClientsCnt);
                return new OperationResult(Result.DeleteClient, "Client was removed. ID=" + id);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Ожидание соединения с клиентом и выполнение действий на основе полученной информации: добавление, удаление клиента, получение сообщения или файла. 
        /// </summary>
        /// <returns>Результат операции.</returns>
        public async Task<OperationResult> ReceiveSomethingFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                StringBuilder recievedMessage = new StringBuilder();
                TcpClient client = serverListener.AcceptTcpClient();
                
                NetworkStream stream = client.GetStream();

                BinaryFormatter bf = new BinaryFormatter();
                // Преобразуем поток байтов в объект класса SendInfo для определения последующих действий.
                SendInfo si = (SendInfo)bf.Deserialize(stream);
                // Добавление нового клиента.
                if (si.id == 0)
                {
                    return SetClient(stream);
                }
                // Удаление клиента.
                if (si.id!=0 && si.type == null)
                {
                    stream.Close();
                    return DeleteClient(si.id);
                }
                // Клиент отправил файл.
                if (si.type=="file")
                {
                    ReceiveFileFromClient(client,si.fileextension);
                    recievedMessage.Append(si.filename);
                }
                // Клиент отправил сообщение.
                else
                {
                    byte[] data = new byte[256];
                    do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                }
                stream.Close();
                client.Close();
                return new OperationResult(Result.OK, "Client "+si.id+">"+recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Отправление сообщения клиенту. 
        /// </summary>
        /// <param name="message">Сообщение клиенту.</param>
        /// <returns>Результат операции.</returns>
        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
        /// <summary>
        /// Получение файла от клиента. 
        /// </summary>
        /// <param name="client">Текущий клиент-соединение.</param>
        /// <param name="file">Расширение файла, полученного от клиента.</param>
        public void ReceiveFileFromClient(TcpClient client,string file)
        {
            NetworkStream ns = client.GetStream();
            BinaryFormatter bf = new BinaryFormatter();
            var bytes = (byte[])bf.Deserialize(ns);
            string dir = DateTime.Today.ToString("yyyy-MM-dd");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            // Создаем файл, который содержит номер последнего добавленного файла для синхронизации
            // нумерации файлов между сессиями.
            string s = "";
            if (!File.Exists(dir + @"\logfile.txt"))
            {
                while (FileIndex != 0)
                {
                    Interlocked.Decrement(ref FileIndex);
                }
            }
            else
            {
                File.SetAttributes(dir + @"\logfile.txt", FileAttributes.Normal);
                using (StreamReader sr = File.OpenText(dir + @"\logfile.txt"))
                {
                    s = sr.ReadLine();
                }
                while (FileIndex != Convert.ToInt32(s))
                {
                    Interlocked.Increment(ref FileIndex);
                }
            }
            Interlocked.Increment(ref FileIndex);

            File.WriteAllBytes(dir + @"\" + "File" + FileIndex + file, bytes);

            using (FileStream fs = File.Create(dir + @"\logfile.txt"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(FileIndex.ToString());
                fs.Write(info, 0, info.Length);
            }
            File.SetAttributes(dir + @"\logfile.txt", FileAttributes.Hidden);
             ns.Close();
            client.Close();
            
        }
    }
}