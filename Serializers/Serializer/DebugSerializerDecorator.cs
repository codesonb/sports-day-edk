using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Data.Serializers
{
    public partial class Serializer<TReader, TWriter>
    {
        public static ISerializer DebugSerializer(Serializer<TReader, TWriter> serializer)
        {
            return new DebugSerializerDecorator(serializer);
        }

        //----------------------------------------------------
        private sealed class DebugSerializerDecorator : Serializer<TReader, TWriter>
        {
            public event EventHandler<SerializerEventArgs> SerializedField;
            public event EventHandler<SerializerEventArgs> DeserializedField;

            public readonly Serializer<TReader, TWriter> BaseSerializer;

            public DebugSerializerDecorator(Serializer<TReader, TWriter> baseSerializer)
            {
                if (null == baseSerializer) throw new ArgumentNullException();
                this.BaseSerializer = baseSerializer;

                this.Serialized += DebugSerializerDecorator_Serialized;
                this.Serialized += DebugSerializerDecorator_Finished;
                this.Deserialized += (s, e) => { Console.WriteLine("deserialized"); };
                this.Deserialized += DebugSerializerDecorator_Finished;
            }

            private void DebugSerializerDecorator_Finished(object sender, EventArgs e)
            {
                string str = sb.ToString();
                string path = @"R:\serailizer_" + DateTime.Now.ToString("hhmmss") + "_" + DateTime.Now.ToBinary().ToString("x16") + ".log";
                using (StreamWriter log = new StreamWriter(path))
                    log.Write(str);
                sb.Clear();
            }
            private void DebugSerializerDecorator_Serialized(object sender, EventArgs e)
            {
                byte[] bys = buffer_writer.ToArray();
                Console.WriteLine("serialized :" + bys.Length);
                ori_stream.Write(bys, 0, bys.Length);
            }
            
            int stack;
            StringBuilder sb = new StringBuilder();

            Stream ori_stream;
            MemoryStream buffer_writer;

            /* --------- pure adaption --------- */
            protected override TWriter getWriter(Stream stream)
            {
                ori_stream = stream;
                buffer_writer = new MemoryStream();
                return BaseSerializer.getWriter(buffer_writer);
            }
            protected override TReader getReader(Stream stream)
            {
                const int C_BufferSize = 1024;
                byte[] buffer = new byte[C_BufferSize];

                MemoryStream ms = new MemoryStream();
                ori_stream = stream;

                int read;
                do
                {
                    read = ori_stream.Read(buffer, 0, C_BufferSize);
                    ms.Write(buffer, 0, read);
                } while (read == C_BufferSize);
                ms.Seek(0, SeekOrigin.Begin);
                return BaseSerializer.getReader(ms);
            }
            protected override long getPosition(TReader reader) { return BaseSerializer.getPosition(reader); }
            protected override long getPosition(TWriter writer) { return BaseSerializer.getPosition(writer); }

            protected override bool ReadBoolean(TReader reader) { return BaseSerializer.ReadBoolean(reader); }
            protected override char ReadChar(TReader reader) { return BaseSerializer.ReadChar(reader); }
            protected override byte ReadByte(TReader reader) { return BaseSerializer.ReadByte(reader); }
            protected override decimal ReadDecimal(TReader reader) { return BaseSerializer.ReadDecimal(reader); }
            protected override double ReadDouble(TReader reader) { return BaseSerializer.ReadDouble(reader); }
            protected override short ReadInt16(TReader reader) { return BaseSerializer.ReadInt16(reader); }
            protected override int ReadInt32(TReader reader) { return BaseSerializer.ReadInt32(reader); }
            protected override long ReadInt64(TReader reader) { return BaseSerializer.ReadInt64(reader); }
            protected override sbyte ReadSByte(TReader reader) { return BaseSerializer.ReadSByte(reader); }
            protected override float ReadSingle(TReader reader) { return BaseSerializer.ReadSingle(reader); }
            protected override string ReadString(TReader reader) { return BaseSerializer.ReadString(reader); }
            protected override ushort ReadUInt16(TReader reader) { return BaseSerializer.ReadUInt16(reader); }
            protected override uint ReadUInt32(TReader reader) { return BaseSerializer.ReadUInt32(reader); }
            protected override ulong ReadUInt64(TReader reader) { return BaseSerializer.ReadUInt64(reader); }
            protected override byte[] ReadBytes(TReader reader, int length) { return BaseSerializer.ReadBytes(reader, length); }

            protected override void Write(TWriter writer, bool value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, char value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, byte value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, sbyte value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, short value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, ushort value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, int value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, uint value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, long value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, ulong value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, float value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, double value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, decimal value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, string value) { BaseSerializer.Write(writer, value); }
            protected override void Write(TWriter writer, byte[] blob) { BaseSerializer.Write(writer, blob); }
            protected override void WriteNull(TWriter writer) { BaseSerializer.WriteNull(writer); }
            protected override void WriteCachedObject(TWriter writer, int index) { BaseSerializer.WriteCachedObject(writer, index); }
            /* --------- pure adaption --------- */

            protected override object _read(TReader reader, FieldInfo fieldInfo, Type fieldType, List<dynamic> objectCache)
            {
                if (stack > 0) sb.Append(new string(' ', stack));
                if (null != fieldInfo)
                    sb.Append("<").Append(fieldInfo.Name).Append("> @ ");
                else
                    sb.Append("[").Append(fieldType.Name).Append("] @ ");
                sb.Append(getPosition(reader)).AppendLine();

                stack++;
                object rtn = base._read(reader, fieldInfo, fieldType, objectCache);
                stack--;

                onDeserializedField(fieldInfo, rtn);
                return rtn;
            }
            protected override bool _write(TWriter writer, FieldInfo fieldInfo, object graph, List<dynamic> objectCache)
            {
                if (stack > 0) sb.Append(new string(' ', stack));
                if (null != fieldInfo)
                    sb.Append("<").Append(fieldInfo.Name).Append("> @ ");
                else
                    sb.Append("[").Append(graph.GetType().Name).Append("] @ ");
                sb.Append(getPosition(writer)).AppendLine();

                stack++;
                bool suc = base._write(writer, fieldInfo, graph, objectCache);
                stack--;

                onSerializedField(fieldInfo, graph);
                return suc;
            }

            // raise events
            private void onSerializedField(FieldInfo fieldInfo, object graph)
            {
                if (null != this.SerializedField)
                    this.SerializedField(this, new SerializerEventArgs(fieldInfo, graph));
            }
            private void onDeserializedField(FieldInfo fieldInfo, object graph)
            {
                if (null != this.DeserializedField)
                    this.DeserializedField(this, new SerializerEventArgs(fieldInfo, graph));
            }

        } // end inner class
    } // end outter class
}
