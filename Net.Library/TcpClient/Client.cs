using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library.Client
{
    /// <summary>
    /// Класс для работы клиента. 
    /// </summary>
   
    public class Client
    {
        /// <summary>
        /// Объект класса TcpClient, который предоставляет клиентские подключения для сетевых служб протокола TCP.
        /// </summary>
        TcpClient tcpClient;
        /// <summary>
        /// Метод для получения сообщения от клиента.
        /// </summary>
        /// <returns>Результат операции.</returns>
        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8081);

                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }
        /// <summary>
        /// Метод для добавления нового клиента.
        /// </summary>
        public void AddClient()
        {
            tcpClient = new TcpClient("127.0.0.1", 8081);
            NetworkStream stream = tcpClient.GetStream();
            // Сериализация объекта класса SendInfo с id=0, что сообщает серверу о необходимости передать клиенту новый доступный ID.
            BinaryFormatter bf = new BinaryFormatter();
            SendInfo si = new SendInfo(null, 0, null, null);
            bf.Serialize(stream, si);

            stream.Close();
            tcpClient.Close();
        }
        /// <summary>
        /// Метод для удаления клиента с данным ID.
        /// </summary>
        /// <param name="id">ID удаляемого клиента.</param>
        public void RemoveClient(int id)
        {
            tcpClient = new TcpClient("127.0.0.1", 8081);
            NetworkStream stream = tcpClient.GetStream();
            // Сериализация объекта класса SendInfo с id!=0, что сообщает серверу о необходимости сделать данный ID доступным новым клиентам.
            BinaryFormatter bf = new BinaryFormatter();
            SendInfo si = new SendInfo(null, id, null, null);
            bf.Serialize(stream, si);

            stream.Close();
            tcpClient.Close();
        }
        /// <summary>
        /// Метод для отправления сообщения серверу.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// /// <param name="id">ID клиента.</param>
        /// <returns>Результат операции.</returns>
        public OperationResult SendMessageToServer(string message,int id)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8081);
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                BinaryFormatter bf = new BinaryFormatter();
                SendInfo si = new SendInfo("message",id,null, null);
                bf.Serialize(stream, si);

                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Метод для отправления сообщения серверу.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// /// <param name="id">ID клиента.</param>
        /// <returns>Результат операции.</returns>
        public OperationResult SendFileToServer(string filename,int id)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8081);
                
                NetworkStream ns = tcpClient.GetStream();
                BinaryFormatter bf = new BinaryFormatter();
                FileInfo fi = new FileInfo(filename);
                SendInfo si = new SendInfo("file",id,fi.Extension,fi.Name);
                bf.Serialize(ns, si);
                var bytes = File.ReadAllBytes(filename);
                bf.Serialize(ns, bytes);
                ns.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
    }
}
