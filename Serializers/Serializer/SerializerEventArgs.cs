using System;
using System.Reflection;

namespace Data.Serializers
{
    public class SerializerEventArgs : EventArgs
    {
        public SerializerEventArgs(FieldInfo fieldInfo, object graph)
        {
            FieldInfo = fieldInfo;
            Graph = graph;
        }
        public readonly FieldInfo FieldInfo;
        public readonly object Graph;
    }
}
