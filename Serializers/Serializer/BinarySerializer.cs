using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Data.Serializers
{
    public sealed class BinarySerializer : Serializer<BinaryReader, BinaryWriter>
    {

        /*******************************************/
        /* -------------- Singleton -------------- */
        private static ISerializer _instance;
        public static ISerializer GetInstance()
        {
#if DEBUG && false
            if (null == _instance) _instance = DebugSerializer(new BinarySerializer());
#else
            if (null == _instance) _instance = new BinarySerializer();
#endif
            return _instance;
        }
        private BinarySerializer() { }
        /*******************************************/

        protected override BinaryReader getReader(Stream stream)
        {
            return new BinaryReader(stream);
        }
        protected override BinaryWriter getWriter(Stream stream)
        {
            return typeof(BinaryWriter).IsInstanceOfType(stream) ?
                (BinaryWriter)(dynamic)stream :     // (???) do not make new binary writer
                new BinaryWriter(stream);
        }
        protected override long getPosition(BinaryReader reader)
        {
            return reader.BaseStream.Position;
        }
        protected override long getPosition(BinaryWriter writer)
        {
            return writer.BaseStream.Position;
        }

        protected override bool ReadBoolean(BinaryReader reader) { return reader.ReadBoolean(); }
        protected override char ReadChar(BinaryReader reader) { return reader.ReadChar(); }
        protected override byte ReadByte(BinaryReader reader) { return reader.ReadByte(); }
        protected override decimal ReadDecimal(BinaryReader reader) { return reader.ReadDecimal(); }
        protected override double ReadDouble(BinaryReader reader) { return reader.ReadDouble(); }
        protected override short ReadInt16(BinaryReader reader) { return reader.ReadInt16(); }
        protected override int ReadInt32(BinaryReader reader) { return reader.ReadInt32(); }
        protected override long ReadInt64(BinaryReader reader) { return reader.ReadInt64(); }
        protected override sbyte ReadSByte(BinaryReader reader) { return reader.ReadSByte(); }
        protected override float ReadSingle(BinaryReader reader) { return reader.ReadSingle(); }
        protected override string ReadString(BinaryReader reader) { return reader.ReadString(); }
        protected override ushort ReadUInt16(BinaryReader reader) { return reader.ReadUInt16(); }
        protected override uint ReadUInt32(BinaryReader reader) { return reader.ReadUInt32(); }
        protected override ulong ReadUInt64(BinaryReader reader) { return reader.ReadUInt64(); }
        protected override byte[] ReadBytes(BinaryReader reader, int length) { return reader.ReadBytes(length); }

        protected override void Write(BinaryWriter writer, bool value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, char value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, byte value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, sbyte value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, short value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, ushort value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, int value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, uint value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, long value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, ulong value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, float value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, double value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, decimal value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, string value) { writer.Write(value); }
        protected override void Write(BinaryWriter writer, byte[] blob) { writer.Write(blob); }
        protected override void WriteNull(BinaryWriter writer) { writer.Write(C_NILSIG); }
        protected override void WriteCachedObject(BinaryWriter writer, int index)
        {
            writer.Write(C_RPTSIG);
            writer.Write(index);
        }

    }
}