using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Serializers
{
    public class JsonSerializer : Serializer<StreamReader, StreamWriter>
    {

        /*******************************************/
        /* -------------- Singleton -------------- */
        private static JsonSerializer _instance;
        private static JsonSerializer GetInstance()
        {
            if (null == _instance) _instance = new JsonSerializer();
            return _instance;
        }

        protected override StreamWriter getWriter(Stream stream) { return new StreamWriter(stream, Encoding.UTF8); }
        protected override StreamReader getReader(Stream stream) { return new StreamReader(stream, Encoding.UTF8); }

        protected override long getPosition(StreamReader reader) { return reader.BaseStream.Position; }
        protected override long getPosition(StreamWriter writer) { return writer.BaseStream.Position; }


        private void WriteValue(StreamWriter writer, string value)
        {
            writer.Write(value);
        }

        protected override void Write(StreamWriter writer, bool value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, char value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, byte value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, sbyte value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, short value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, ushort value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, int value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, uint value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, long value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, ulong value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, float value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, double value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, decimal value) { writer.Write(value); }
        protected override void Write(StreamWriter writer, string value)
        {
            writer.Write("\"");
            writer.Write(value);
            writer.Write("\"");
        }
        protected override void Write(StreamWriter writer, byte[] blob)
        {
            writer.Write("\"");
            writer.Write(Convert.ToBase64String(blob));
            writer.Write("\"");
        }
        protected override void WriteNull(StreamWriter writer) { writer.Write("null"); }
        protected override void WriteCachedObject(StreamWriter writer, int index)
        {
            writer.Write("{\"__type\": \"");
            writer.Write(C_RPTSIG);
            writer.Write("\", \"index\":");
            writer.Write(index);
            writer.Write("},");
        }

        protected override bool ReadBoolean(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override char ReadChar(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override byte ReadByte(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override sbyte ReadSByte(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override short ReadInt16(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override ushort ReadUInt16(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override int ReadInt32(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override uint ReadUInt32(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override long ReadInt64(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override ulong ReadUInt64(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override float ReadSingle(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override double ReadDouble(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override decimal ReadDecimal(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override string ReadString(StreamReader reader)
        {
            throw new NotImplementedException();
        }

        protected override byte[] ReadBytes(StreamReader reader, int length)
        {
            throw new NotImplementedException();
        }

        private JsonSerializer() { }
        /*******************************************/


    }
}
