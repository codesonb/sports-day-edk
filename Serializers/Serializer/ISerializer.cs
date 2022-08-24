using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Data.Serializers
{ 
    public interface ISerializer
    {
        bool Serialize(Stream stream, object graph);
        dynamic Deserialize(Stream stream);
    }

}
