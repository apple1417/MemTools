using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MemTools {
  public static class MemIOExtensions {
    public static IntPtr FindPointerEnd(this MemManager manager, Pointer fullPtr) {
      IntPtr ptr = fullPtr.BaseAddress;
      for (int i = 0; i < fullPtr.Offsets.Count - 1; i++) {
        ptr = IntPtr.Add(ptr, fullPtr.Offsets[i]);
        ptr = manager.Read<IntPtr>(ptr);
      }
      return IntPtr.Add(ptr, fullPtr.Offsets.Last());
    }

    public static byte[] Read(this MemManager manager, Pointer ptr, long len) => manager.Read(manager.FindPointerEnd(ptr), len);

    public static T Read<T>(this MemManager manager, Pointer ptr) where T : struct => manager.Read<T>(manager.FindPointerEnd(ptr));
    public static T Read<T>(this MemManager manager, IntPtr addr) where T : struct {
      Type readType = typeof(T);
      readType = readType.IsEnum ? Enum.GetUnderlyingType(readType) : readType;
      int size = Marshal.SizeOf(readType);

      // Pre read conversions
      if (typeof(T) == typeof(IntPtr)) {
        readType = manager.Is64Bit ? typeof(Int64) : typeof(Int32);
        size = manager.Is64Bit ? 8 : 4;
      } else if (typeof(T) == typeof(UIntPtr)) {
        readType = manager.Is64Bit ? typeof(UInt64) : typeof(UInt32);
        size = manager.Is64Bit ? 8 : 4;
      }

      // Read the data and convert it to a <readType> object
      byte[] data = manager.Read(addr, size);
      GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
      var val = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), readType);
      handle.Free();

      // Post read conversions
      if (typeof(T) == typeof(IntPtr) || typeof(T) == typeof(UIntPtr)) {
        ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { val.GetType() });
        return (T) constructor.Invoke(new object[] { val });
      }

      return (T) val;
    }

    public static string ReadString(this MemManager manager, Pointer ptr, Encoding strEncoding, int bufferSize = 256) => manager.ReadString(manager.FindPointerEnd(ptr), strEncoding, bufferSize);
    public static string ReadString(this MemManager manager, IntPtr addr, Encoding strEncoding, int bufferSize = 256) {
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
      return strEncoding.GetString(fullStr.ToArray());
    }

    public static IntPtr ReadRelativePtr(this MemManager manager, Pointer ptr) => manager.ReadRelativePtr(manager.FindPointerEnd(ptr));
    public static IntPtr ReadRelativePtr(this MemManager manager, IntPtr addr) {
      if (manager.Is64Bit) {
        throw new NotImplementedException();
      } else {
        return new IntPtr(manager.Read<Int32>(addr) + addr.ToInt32() + 4);
      }
    }

    public static void Write(this MemManager manager, Pointer ptr, byte[] buffer) => manager.Write(manager.FindPointerEnd(ptr), buffer);

    public static void Write<T>(this MemManager manager, Pointer ptr, T val) where T : struct => manager.Write<T>(manager.FindPointerEnd(ptr), val);
    public static void Write<T>(this MemManager manager, IntPtr addr, T val) where T : struct {
      object oVal = val;
      int size = Marshal.SizeOf(oVal);

      if (typeof(T) == typeof(IntPtr)) {
        if (manager.Is64Bit) {
          oVal = ((IntPtr) oVal).ToInt64();
          size = 8;
        } else {
          oVal = ((IntPtr) oVal).ToInt32();
          size = 4;
        }
      } else if (typeof(T) == typeof(UIntPtr)) {
        if (manager.Is64Bit) {
          oVal = ((UIntPtr) oVal).ToUInt64();
          size = 8;
        } else {
          oVal = ((UIntPtr) oVal).ToUInt32();
          size = 4;
        }
      }

      // Convert the object to a byte[]
      byte[] data = new byte[size];
      GCHandle handle = GCHandle.Alloc(oVal, GCHandleType.Pinned);
      Marshal.Copy(handle.AddrOfPinnedObject(), data, 0, size);
      handle.Free();

      manager.Write(addr, data);
    }

    public static void WriteString(this MemManager manager, Pointer ptr, string val, Encoding strEncoding) => manager.WriteString(manager.FindPointerEnd(ptr), val, strEncoding);
    public static void WriteString(this MemManager manager, IntPtr addr, string val, Encoding strEncoding) {
      manager.Write(addr, strEncoding.GetBytes(val + "\x00"));
    }

    public static void WriteRelativeAddress(this MemManager manager, Pointer ptr, IntPtr val) => manager.WriteRelativeAddress(manager.FindPointerEnd(ptr), val);
    public static void WriteRelativeAddress(this MemManager manager, IntPtr addr, IntPtr val) {
      if (manager.Is64Bit) {
        throw new NotImplementedException();
      } else {
        manager.Write(addr, val.ToInt32() - addr.ToInt32() - 4);
      }
    }
  }
}
