using System;

namespace Logging.Models
{
    public class ExceptionData
    {
        public ExceptionData(Exception ex)
        {
            Message = ex.Message;
            StackTrace = ex.ToString();
        }

        private string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
