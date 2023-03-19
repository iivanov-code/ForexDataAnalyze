using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ForexTest
{
    [Serializable]
    public class SerializationTest : ISerializable
    {
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected SerializationTest(SerializationInfo info, StreamingContext context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}
