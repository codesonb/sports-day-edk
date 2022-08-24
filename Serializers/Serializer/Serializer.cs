using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Data.Serializers
{
    abstract partial class Serializer<TReader, TWriter> : ISerializer
    {
        public event EventHandler<SerializerEventArgs> SerializingField;
        public event EventHandler<SerializerEventArgs> DeserializingField;

        public event EventHandler Serialized;
        public event EventHandler Deserialized;

        //share cache [ (?) singleton pattern for easier usage ] 
        protected static Dictionary<Type, FieldInfo[]> _cache = new Dictionary<Type, FieldInfo[]>();
        protected const string C_NILSIG = "$NILVAL";
        protected const string C_RPTSIG = "$RPTOBJ";

        public bool Serialize(Stream stream, object graph)
        {
            try
            {
                TWriter bw = getWriter(stream);
                return this._serialize(bw, null, graph, new List<dynamic>());
            }
            finally
            {
                if (null != Serialized) Serialized(this, EventArgs.Empty);
            }
        }
        public dynamic Deserialize(Stream stream)
        {
            try
            {
                TReader br = getReader(stream);
                return this._deserialize(br, null, typeof(object), new List<dynamic>());
            }
            finally
            {
                if (null != Deserialized) Deserialized(this, EventArgs.Empty);
            }
        }

        protected abstract TWriter getWriter(Stream stream);
        protected abstract TReader getReader(Stream stream);
        protected abstract long getPosition(TReader reader);
        protected abstract long getPosition(TWriter writer);

        protected abstract void Write(TWriter writer, bool value);
        protected abstract void Write(TWriter writer, char value);
        protected abstract void Write(TWriter writer, byte value);
        protected abstract void Write(TWriter writer, sbyte value);
        protected abstract void Write(TWriter writer, short value);
        protected abstract void Write(TWriter writer, ushort value);
        protected abstract void Write(TWriter writer, int value);
        protected abstract void Write(TWriter writer, uint value);
        protected abstract void Write(TWriter writer, long value);
        protected abstract void Write(TWriter writer, ulong value);
        protected abstract void Write(TWriter writer, float value);
        protected abstract void Write(TWriter writer, double value);
        protected abstract void Write(TWriter writer, decimal value);
        protected abstract void Write(TWriter writer, string value);
        protected abstract void Write(TWriter writer, byte[] blob);
        protected abstract void WriteNull(TWriter writer);
        protected abstract void WriteCachedObject(TWriter writer, int index);

        protected abstract bool ReadBoolean(TReader reader);
        protected abstract char ReadChar(TReader reader);
        protected abstract byte ReadByte(TReader reader);
        protected abstract sbyte ReadSByte(TReader reader);
        protected abstract short ReadInt16(TReader reader);
        protected abstract ushort ReadUInt16(TReader reader);
        protected abstract int  ReadInt32(TReader reader);
        protected abstract uint ReadUInt32(TReader reader);
        protected abstract long ReadInt64(TReader reader);
        protected abstract ulong ReadUInt64(TReader reader);
        protected abstract float ReadSingle(TReader reader);
        protected abstract double ReadDouble(TReader reader);
        protected abstract decimal ReadDecimal(TReader reader);
        protected abstract string ReadString(TReader reader);
        protected abstract byte[] ReadBytes(TReader reader, int length);

        protected virtual bool _write(TWriter writer, FieldInfo fieldInfo, object graph, List<dynamic> objectCache)
        {
            return _serialize(writer, fieldInfo, graph, objectCache);
        }
        protected virtual object _read(TReader reader, FieldInfo fieldInfo, Type fieldType, List<dynamic> objectCache)
        {
            return _deserialize(reader, fieldInfo, fieldType, objectCache);
        }

        private bool _serialize(TWriter writer, FieldInfo fInfo, object graph, List<dynamic> objectCache)
        {
            onSerializing(fInfo, graph);

            //null reference handle
            if (null == graph)
            {
                //handle null array
                if (fInfo.FieldType.IsArray || null != fInfo.FieldType.GetInterface("ICollection") || null != fInfo.FieldType.GetInterface("ICollection`1"))
                {
                    graph = new object[0]; // escape from null reference, force write 0 length such that no need to care about actual type
                }
                else
                {
                    WriteNull(writer);
                    return true;
                }
            }

            // get object type
            Type typ = graph.GetType();
            TypeCode tc = Type.GetTypeCode(typ);

            switch (tc)
            {
                case TypeCode.Object:
                    // handle special cases
                    // 1) Collections, incl. Array  - reduce size for 
                    // 2) Color - save int value of the coloec
                    // 3) Image - save image as | img_length + img_png_data |

                    // Note -- <string> is also ICollection of <char>, here is filtered out by TypeCode.Object
                    // Type.GetTypeCode(type) != TypeCode.String &&

                    // Note
                    //               | Array | Array[,] | List | Dictionary | Sorted-XXX | Stack | Queue | LinkedList | HashSet
                    //-----------------------------------------------------------------------------------------------------------
                    // ICollection   | yes   | yes      | yes  | yes        | yes        | yes   | yes   | yes        | no
                    // ICollection`1 | yes   | no       | yes  | yes        | yes        | no    | no    | yes        | yes

                    if (null != typ.GetInterface("ICollection") || null != typ.GetInterface("ICollection`1"))
                    {
                        //catch generic type
                        Type[] genTyps = typ.GetGenericArguments();

                        //convert
                        ICollection collection = graph as ICollection;
                        int arrCount;
                        if (null == collection) //HashSet<> : ICollection`1
                        {
                            Type ic1 = typeof(ICollection<>).MakeGenericType(genTyps[0]);

                            //get collection count
                            PropertyInfo p_count = ic1.GetProperty("Count");
                            arrCount = (int)p_count.GetValue(graph, new object[0]);

                            //copy array
                            collection = Array.CreateInstance(genTyps[0], arrCount);
                            if (arrCount > 0)
                            {
                                MethodInfo m_toarray = ic1.GetMethod("CopyTo");
                                m_toarray.Invoke(graph, new object[] { collection, 0 });    //void function
                            }
                        }
                        else
                        {
                            arrCount = collection.Count;
                        }

                        //write length
                        this.Write(writer, arrCount);

                        if (genTyps.Length > 1)
                        {
                            //#dangerous code peice, assume only key-value-pairs appears
                            foreach (dynamic obj in graph as ICollection)
                            {
                                Type kvtyp = obj.GetType();
                                PropertyInfo keyinfo = kvtyp.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                                PropertyInfo valinfo = kvtyp.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                                dynamic key = keyinfo.GetValue(obj, new object[0]);
                                dynamic val = valinfo.GetValue(obj, new object[0]);

                                this._write(writer, null, key, objectCache);
                                this._write(writer, null, val, objectCache);
                            }
                        }
                        else
                        {
                            //write items
                            foreach (object obj in collection as ICollection)
                                this._write(writer, null, obj, objectCache);
                        }
                    }
                    else if (graph is Color)
                    {
                        //convert
                        Color color = (Color)graph;
                        int colorBin = color.ToArgb();
                        //write integer expression
                        this.Write(writer, colorBin);
                    }
                    else if (graph is Image)
                    {
                        //convert
                        Image img = (Image)graph;

                        //prepare buffer stream
                        using (MemoryStream ms = new MemoryStream())
                        {
                            //save to memory stream
                            img.Save(ms, ImageFormat.Png);

                            //get & write length of file
                            this.Write(writer, ms.Position);

                            //concatanate file
                            byte[] buffer = ms.ToArray();
                            this.Write(writer, buffer);
                        }
                    }
                    else
                    {
                        int idx = objectCache.IndexOf(graph);
                        if (idx < 0)
                        {
                            //cache
                            objectCache.Add(graph);

                            // get type members
                            FieldInfo[] fields;
                            if (!_cache.TryGetValue(typ, out fields))
                            {
                                fields = typ.GetHierarchicalFields();
                                _cache.Add(typ, fields);
                            }

                            this.Write(writer, graph.GetType().FullName);
                            // get member values
                            foreach (FieldInfo f in fields)
                                this._write(writer, f, f.GetValue(graph), objectCache);
                        }
                        else
                        {
                            this.WriteCachedObject(writer, idx);
                        }
                    }
                    break;
                case TypeCode.DateTime:
                    //write long (64 bits int) to represent a DateTime structure
                    long dateBin = ((DateTime)graph).ToBinary();
                    this.Write(writer, dateBin);
                    break;
                case TypeCode.Boolean: this.Write(writer, (bool)graph); break;
                case TypeCode.Byte: this.Write(writer, (byte)graph); break;
                case TypeCode.Char: this.Write(writer, (char)graph); break;
                case TypeCode.Decimal: this.Write(writer, (decimal)graph); break;
                case TypeCode.Double: this.Write(writer, (double)graph); break;
                case TypeCode.Int16: this.Write(writer, (short)graph); break;
                case TypeCode.Int32: this.Write(writer, (int)graph); break;
                case TypeCode.Int64: this.Write(writer, (long)graph); break;
                case TypeCode.SByte: this.Write(writer, (sbyte)graph); break;
                case TypeCode.Single: this.Write(writer, (float)graph); break;
                case TypeCode.String: this.Write(writer, (string)graph); break;
                case TypeCode.UInt16: this.Write(writer, (ushort)graph); break;
                case TypeCode.UInt32: this.Write(writer, (uint)graph); break;
                case TypeCode.UInt64: this.Write(writer, (ulong)graph); break;
            }

            return true;
        }

        private dynamic _deserialize(TReader reader, FieldInfo fInfo, Type fieldType, List<dynamic> objectCache)
        {
            onDeserializing(fInfo);

            TypeCode tc = Type.GetTypeCode(fieldType);
            switch (tc)
            {
                case TypeCode.Object:
                    if (null != fieldType.GetInterface("ICollection") || null != fieldType.GetInterface("ICollection`1"))
                    {   //collection detected
                        //get collection size
                        int arrLength = this.ReadInt32(reader);

                        //check actual type :
                        // (1) Array            1-dimension array
                        // (2) ICollection      non generic collection
                        // (3) ICollection<>    single generic type
                        // (4) ICollection<,>   double generic type
                        if (fieldType.IsArray)
                        {
                            //get array element type
                            Type elmType = fieldType.GetElementType();

                            //create objects
                            dynamic x = Array.CreateInstance(elmType, arrLength);
                            for (int i = 0; i < arrLength; i++)
                            {
                                dynamic dyn = this._read(reader, null, elmType, objectCache);
                                if (elmType.IsEnum)
                                    dyn = Convert.ChangeType(Enum.Parse(elmType, dyn.ToString()), elmType);
                                x[i] = dyn;
                            }

                            //return array
                            return x;
                        }
                        else
                        {
                            //convert to type designated by declaration
                            var collection = Activator.CreateInstance(fieldType);

                            //get generic types
                            Type[] genTyps = fieldType.GetGenericArguments();           //check list type
                            MethodInfo mInfo = fieldType.GetMethod("Add", genTyps);     //get insertion method

                            //#DEBUG REQUIRED
                            // dangerous code peice...

                            //The following generalized both types of:
                            // type (1) - single-typed value list
                            // type (2) - key-value-pair construction

                            for (int i = 0; i < arrLength; i++)
                            {
                                object[] pars = new object[genTyps.Length];
                                for (int j = 0; j < genTyps.Length; j++)
                                    pars[j] = this._read(reader, null, genTyps[j], objectCache);
                                mInfo.Invoke(collection, pars);
                            }
                            return collection;

                            // *Note: mInfo assumes the generic-collections has method Add(...) and;
                            //        the parameters of the method correspond to the generic definition
                        }
                    }
                    else if (typeof(Color) == fieldType)
                    {
                        //read integer value of color
                        int colorBin = this.ReadInt32(reader);
                        //convert
                        Color color = Color.FromArgb(colorBin);
                        //return
                        return color;
                    }
                    else if (fieldType.IsSubclassOf(typeof(Image)) || typeof(Image) == fieldType)
                    {
                        //read image length
                        long imgLength = this.ReadInt64(reader);

                        //pass to memory stream
                        using (MemoryStream ms = new MemoryStream())
                        {
                            //read image data to temporary memory stream by buffer
                            while (imgLength > 0)
                            {
                                //capture image division by buffer
                                byte[] buffer;
                                if (imgLength > int.MaxValue)
                                    buffer = this.ReadBytes(reader, int.MaxValue);
                                else
                                    buffer = this.ReadBytes(reader, (int)imgLength);

                                //move to temporary memory stream
                                ms.Write(buffer, 0, buffer.Length);
                                imgLength -= buffer.Length;
                            }

                            //back to begin
                            ms.Seek(0, SeekOrigin.Begin);

                            //read image
                            Image img = Image.FromStream(ms);
                            return img;
                        }
                    }
                    else //single instance object
                    {
                        // read actual type name (due to inheritance)
                        // - also for cached-detection
                        string readTypeName = this.ReadString(reader);
                        switch (readTypeName)
                        {
                            case C_NILSIG: return null; //no read, no need to set
                            case C_RPTSIG:
                                int idx = this.ReadInt32(reader);
                                return objectCache[idx];
                            default:
                                //read the type to be read
                                Type readType = Extensions.GetType(readTypeName);

                                //create object
                                dynamic x = FormatterServices.GetUninitializedObject(readType);
                                objectCache.Add(x);     //cache

                                //get field information
                                FieldInfo[] fields;
                                if (!_cache.TryGetValue(readType, out fields))
                                {
                                    fields = readType.GetHierarchicalFields();
                                    _cache.Add(readType, fields);
                                }

                                //deserialization
                                foreach (FieldInfo f in fields)
                                {
                                    //read stream
                                    dynamic dyn = this._read(reader, f, f.FieldType, objectCache);

                                    //convert enum
                                    if (f.FieldType.IsEnum)
                                        dyn = Convert.ChangeType(Enum.Parse(f.FieldType, dyn.ToString()), f.FieldType);

                                    //set value
                                    f.SetValue(x, dyn);
                                }
                                return x;
                        }
                    }
                //end of TypeCode.Object
                case TypeCode.DateTime:
                    //write long (64 bits int) representing struc<DateTime> - standard numeric representation time format
                    long binval = this.ReadInt64(reader);
                    return DateTime.FromBinary(binval);
                case TypeCode.Boolean: return this.ReadBoolean(reader);
                case TypeCode.Byte: return this.ReadByte(reader);
                case TypeCode.Char: return this.ReadChar(reader);
                case TypeCode.Decimal: return this.ReadDecimal(reader);
                case TypeCode.Double: return this.ReadDouble(reader);
                case TypeCode.Int16: return this.ReadInt16(reader);
                case TypeCode.Int32: return this.ReadInt32(reader);
                case TypeCode.Int64: return this.ReadInt64(reader);
                case TypeCode.SByte: return this.ReadSByte(reader);
                case TypeCode.Single: return this.ReadSingle(reader);
                case TypeCode.String: return this.ReadString(reader);
                case TypeCode.UInt16: return this.ReadUInt16(reader);
                case TypeCode.UInt32: return this.ReadUInt32(reader);
                case TypeCode.UInt64: return this.ReadUInt64(reader);
            }
            throw new InvalidOperationException();
        }

        protected void onSerializing(FieldInfo fieldInfo, object graph)
        {
            if (null != this.SerializingField)
                this.SerializingField(this, new SerializerEventArgs(fieldInfo, graph));
        }
        protected void onDeserializing(FieldInfo fieldInfo)
        {
            if (null != this.DeserializingField)
                this.DeserializingField(this, new SerializerEventArgs(fieldInfo, null));
        }

    }
}
