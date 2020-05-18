using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library
{
    /// <summary>
    /// Сериализуемый класс для передачи от клиента серверу информации о совершаемой операции.
    /// </summary>
    [Serializable]
    class SendInfo
    {
        /// <summary>
        /// Тип передаваемой информации - сообщение, файл.
        /// </summary>
        public string type;
        /// <summary>
        /// ID клиента, передающего информацию.
        /// </summary>
        public int id;
        /// <summary>
        /// Расширение передаваемого файла.
        /// </summary>
        public string fileextension;
        /// <summary>
        /// Имя передаваемого файла.
        /// </summary>
        public string filename;
        /// <summary>
        /// Конструктор класса SendInfo.
        /// </summary>
        public SendInfo(string type,int id,string fileextension,string filename)
        {
            this.type = type;
            this.id = id;
            this.fileextension = fileextension;
            this.filename = filename;
        }
    }
}
