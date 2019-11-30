using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemTools {
  public static class MemIOExtensions {
    public static IntPtr FindPointerEnd(this MemManager manager, Pointer fullPtr) {
      IntPtr ptr = fullPtr.BaseAddress;
      for (int i = 0; i < fullPtr.Offsets.Count - 1; i++) {
        ptr = IntPtr.Add(ptr, fullPtr.Offsets[i]);
        ptr = manager.ReadPtr(ptr);
      }
      return IntPtr.Add(ptr, fullPtr.Offsets.Last());
    }

    public static byte[] Read(this MemManager manager, Pointer ptr, long len) => manager.Read(ptr, len);

    public static Byte ReadByte(this MemManager manager, Pointer ptr) => manager.ReadByte(manager.FindPointerEnd(ptr));
    public static Byte ReadByte(this MemManager manager, IntPtr addr) {
      return manager.Read(addr, 1)[0];
    }

    public static Int16 ReadInt16(this MemManager manager, Pointer ptr) => manager.ReadInt16(manager.FindPointerEnd(ptr));
    public static Int16 ReadInt16(this MemManager manager, IntPtr addr) {
      return BitConverter.ToInt16(manager.Read(addr, 2), 0);
    }

    public static Int32 ReadInt32(this MemManager manager, Pointer ptr) => manager.ReadInt32(manager.FindPointerEnd(ptr));
    public static Int32 ReadInt32(this MemManager manager, IntPtr addr) {
      return BitConverter.ToInt32(manager.Read(addr, 4), 0);
    }

    public static Int64 ReadInt64(this MemManager manager, Pointer ptr) => manager.ReadInt64(manager.FindPointerEnd(ptr));
    public static Int64 ReadInt64(this MemManager manager, IntPtr addr) {
      return BitConverter.ToInt64(manager.Read(addr, 8), 0);
    }

    public static Single ReadFloat(this MemManager manager, Pointer ptr) => manager.ReadFloat(manager.FindPointerEnd(ptr));
    public static Single ReadFloat(this MemManager manager, IntPtr addr) {
      return BitConverter.ToSingle(manager.Read(addr, 4), 0);
    }

    public static Double ReadDouble(this MemManager manager, Pointer ptr) => manager.ReadDouble(manager.FindPointerEnd(ptr));
    public static Double ReadDouble(this MemManager manager, IntPtr addr) {
      return BitConverter.ToDouble(manager.Read(addr, 8), 0);
    }

    public static string ReadAscii(this MemManager manager, Pointer ptr) => manager.ReadAscii(manager.FindPointerEnd(ptr), 256);
    public static string ReadAscii(this MemManager manager, IntPtr addr) => manager.ReadAscii(addr, 256);
    public static string ReadAscii(this MemManager manager, Pointer ptr, int bufferSize) => manager.ReadAscii(manager.FindPointerEnd(ptr), bufferSize);
    public static string ReadAscii(this MemManager manager, IntPtr addr, int bufferSize) {
      return Encoding.ASCII.GetString(ReadStrBytes(manager, addr, bufferSize));
    }

    public static string ReadUtf8(this MemManager manager, Pointer ptr) => manager.ReadUtf8(manager.FindPointerEnd(ptr), 256);
    public static string ReadUtf8(this MemManager manager, IntPtr addr) => manager.ReadUtf8(addr, 256);
    public static string ReadUtf8(this MemManager manager, Pointer ptr, int bufferSize) => manager.ReadUtf8(manager.FindPointerEnd(ptr), bufferSize);
    public static string ReadUtf8(this MemManager manager, IntPtr addr, int bufferSize) {
      return Encoding.UTF8.GetString(ReadStrBytes(manager, addr, bufferSize));
    }
    
    private static byte[] ReadStrBytes(MemManager manager, IntPtr addr, int bufferSize) {
      List<byte> fullStr = new List<byte>();
      byte[] buf = manager.Read(addr, bufferSize);

      int i = 0;
      while (buf[i] != 0) {
        fullStr.Add(buf[i]);

        i++;
        if (i == buf.Length) {
          addr = IntPtr.Add(addr, bufferSize);
          buf = manager.Read(addr, bufferSize);

          i = 0;
        }
      }
      return fullStr.ToArray();
    }

    // Reads an address directly out of memory - used for things like move instructions
    public static IntPtr ReadPtr(this MemManager manager, Pointer ptr) => manager.ReadPtr(manager.FindPointerEnd(ptr));
    public static IntPtr ReadPtr(this MemManager manager, IntPtr addr) {
      if (manager.Is64Bit) {
        return new IntPtr(manager.ReadInt64(addr));
      } else {
        return new IntPtr(manager.ReadInt32(addr));
      }
    }

    // Reads a relative address out of memory - used for things like calls or jump instructions
    public static IntPtr ReadOffset(this MemManager manager, Pointer ptr) => manager.ReadOffset(manager.FindPointerEnd(ptr));
    public static IntPtr ReadOffset(this MemManager manager, IntPtr addr) {
      if (manager.Is64Bit) {
        throw new NotImplementedException();
      } else {
        return new IntPtr(manager.ReadInt32(addr) + addr.ToInt32() + 4);
      }
    }

    public static void Write(this MemManager manager, Pointer ptr, byte[] buffer) => manager.Write(manager.FindPointerEnd(ptr), buffer);

    public static void WriteByte(this MemManager manager, Pointer ptr, Byte val) => manager.WriteByte(manager.FindPointerEnd(ptr), val);
    public static void WriteByte(this MemManager manager, IntPtr addr, Byte val) {
      manager.Write(addr, new byte[] { val });
    }

    public static void WriteInt16(this MemManager manager, Pointer ptr, Int16 val) => manager.WriteInt16(manager.FindPointerEnd(ptr), val);
    public static void WriteInt16(this MemManager manager, IntPtr addr, Int16 val) {
      manager.Write(addr, BitConverter.GetBytes(val));
    }

    public static void WriteInt32(this MemManager manager, Pointer ptr, Int32 val) => manager.WriteInt32(manager.FindPointerEnd(ptr), val);
    public static void WriteInt32(this MemManager manager, IntPtr addr, Int32 val) {
      manager.Write(addr, BitConverter.GetBytes(val));
    }

    public static void WriteInt64(this MemManager manager, Pointer ptr, Int64 val) => manager.WriteInt64(manager.FindPointerEnd(ptr), val);
    public static void WriteInt64(this MemManager manager, IntPtr addr, Int64 val) {
      manager.Write(addr, BitConverter.GetBytes(val));
    }

    public static void WriteFloat(this MemManager manager, Pointer ptr, Single val) => manager.WriteFloat(manager.FindPointerEnd(ptr), val);
    public static void WriteFloat(this MemManager manager, IntPtr addr, Single val) {
      manager.Write(addr, BitConverter.GetBytes(val));
    }

    public static void WriteDouble(this MemManager manager, Pointer ptr, Double val) => manager.WriteDouble(manager.FindPointerEnd(ptr), val);
    public static void WriteDouble(this MemManager manager, IntPtr addr, Double val) {
      manager.Write(addr, BitConverter.GetBytes(val));
    }

    public static void WriteAscii(this MemManager manager, Pointer ptr, string val) => manager.WriteAscii(manager.FindPointerEnd(ptr), val);
    public static void WriteAscii(this MemManager manager, IntPtr addr, string val) {
      manager.Write(addr, Encoding.ASCII.GetBytes(val + "\x00"));
    }

    public static void WriteUtf8(this MemManager manager, Pointer ptr, string val) => manager.WriteUtf8(manager.FindPointerEnd(ptr), val);
    public static void WriteUtf8(this MemManager manager, IntPtr addr, string val) {
      manager.Write(addr, Encoding.UTF8.GetBytes(val + "\x00"));
    }

    public static void WritePtr(this MemManager manager, Pointer ptr, IntPtr val) => manager.WritePtr(manager.FindPointerEnd(ptr), val);
    public static void WritePtr(this MemManager manager, IntPtr addr, IntPtr val) {
      if (manager.Is64Bit) {
        manager.WriteInt64(addr, val.ToInt64());
      } else {
        manager.WriteInt32(addr, val.ToInt32());
      }
    }

    public static void WriteOffset(this MemManager manager, Pointer ptr, IntPtr val) => manager.WriteOffset(manager.FindPointerEnd(ptr), val);
    public static void WriteOffset(this MemManager manager, IntPtr addr, IntPtr val) {
      if (manager.Is64Bit) {
        throw new NotImplementedException();
      } else {
        manager.WriteInt32(addr, val.ToInt32() - addr.ToInt32() - 4);
      }
    }
  }
}
