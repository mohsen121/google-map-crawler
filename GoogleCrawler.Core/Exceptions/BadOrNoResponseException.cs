using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCrawler.Core.Exceptions
{
    [System.Serializable]
    public class BadOrNoResponseException : Exception
    {
        public BadOrNoResponseException() { }
        public BadOrNoResponseException(string message) : base(message) { }
        public BadOrNoResponseException(string message, Exception inner) : base(message, inner) { }
        protected BadOrNoResponseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
