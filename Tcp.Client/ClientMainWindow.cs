using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;
using SomeProject.Library.Server;
using System.Threading;

namespace SomeProject.TcpClient
{
    /// <summary>
    /// Класс, отвечающий за работу формы ClientMainWindow.
    /// </summary>
    public partial class ClientMainWindow : Form
    {
        /// <summary>
        /// ID клиента.
        /// </summary>
        public int id = 0;
        /// <summary>
        /// Путь к файлу, передаваемому серверу.
        /// </summary>
        public string SendFileName = null;
        /// <summary>
        /// Объект класса Client.
        /// </summary>
        public Client client;
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public ClientMainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Обработчик события загрузки формы.
        /// Инициализирует нового клиента, если это возможно.
        /// </summary>
        private void ClientMainWindow_Load(object sender, EventArgs e)
        {
            try
            {
                client = new Client();
                client.AddClient();
                string res = client.ReceiveMessageFromServer().Message;
                int resLength = res.Length;
                id = Convert.ToInt32(res.Substring(resLength - 1, 1));
                textBox1.Text +=res + Environment.NewLine;
            }
            catch
            {
                MessageBox.Show("Новый клиент недоступен!", "Error");
                this.Close();
            }
        }
        /// <summary>
        /// Обработчик события нажатия на кнопку Send Message.
        /// </summary>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            if (textBox.Text!="")
            {
                OperationResult res = client.SendMessageToServer(textBox.Text,id);
                if (res.Result == Result.OK)
                {
                    textBox.Text = "";
                    labelRes.Text = "Message was sent succefully!";
                    
                }
                else
                {
                    labelRes.Text = "Cannot send the message to the server.";
                }
                textBox1.Text += client.ReceiveMessageFromServer().Message + Environment.NewLine;
                timer.Interval = 2000;
                timer.Start();
            }
            
        }
        /// <summary>
        /// Обработчик таймера.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }
        /// <summary>
        /// Обработчик события нажатия на кнопку Send File.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            //Client client = new Client();
            if (SendFileName != null)
            {
                OperationResult res = client.SendFileToServer(SendFileName,id);
                if (res.Result == Result.OK)
                {
                    label1.Text = "";
                    SendFileName = null;
                    labelRes.Text = "File was sent succefully!";
                }
                else
                {
                    labelRes.Text = "Cannot send the file to the server.";
                }
                textBox1.Text += client.ReceiveMessageFromServer().Message + Environment.NewLine;
                timer.Interval = 2000;
                timer.Start();
            }
        }
        /// <summary>
        /// Обработчик события нажатия на кнопку Choose File.
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SendFileName = dlg.FileName;
                label1.Text = dlg.SafeFileName;
            }
        }
        /// <summary>
        /// Обработчик события закрытия формы.
        /// </summary>
        private void ClientMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (id != 0)
            {
                client.RemoveClient(id);
            }
        }
    }
}
