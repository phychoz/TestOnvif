using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /*        
           t=  (time the session is active)     
           r=* (zero or more repeat times)
         */
    public class TimeDescription
    {
        public string Time { get; private set; }
        public List<string> Repetitions = null;

        public TimeDescription()
        {
            Repetitions = new List<string>();
        }

        public TimeDescription(string time)
        {
            Time = time;
            Repetitions = new List<string>();
        }
    }
}
