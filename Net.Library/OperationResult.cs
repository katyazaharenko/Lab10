using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeProject.Library
{
    /// <summary>
    /// Перечисление возможных результатов обработки сервером полученной информации.
    /// </summary>
    public enum Result { OK, Fail ,LimitFail,AddClient,DeleteClient};
    /// <summary>
    /// Класс, объект которого представляет результат работы сервера.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Результат операции.
        /// </summary>
        public Result Result;
        /// <summary>
        /// Сообщение, сопровождающее результат.
        /// </summary>
        public string Message;
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public OperationResult(Result result, string message)
        {
            Result = result;
            Message = message;
        }
    }
}
