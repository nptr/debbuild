using System;
using System.Collections.Generic;
using System.Text;

namespace DebianPackage
{
    public class Result
    {
        private bool m_success;
        
        private string m_message;
        public string Message
        {  
            get { return m_message; } 
        }

        private Result()
        {
            m_success = true;
            m_message = null;
        }

        private Result(string message)
        {
            m_success = false;
            m_message = message;
        }

        public static Result Ok()
        {
            return new Result();
        }

        public static Result Fail()
        {
            return new Result(null);
        }

        public static Result Fail(string message)
        {
            return new Result(message);
        }

        public bool IsOk()
        {
            return m_success;
        }
    }
}
