using System;
using System.Collections.Generic;
using System.Linq;

namespace MemTools {
  public class Pointer {
    public IntPtr BaseAddress { get; private set; }
    public List<int> Offsets { get; private set; }

    public Pointer(IntPtr baseAddr, params int[] offsets) {
      BaseAddress = baseAddr;
      Offsets = new List<int>(offsets);
    }

    public Pointer Clone() {
      return new Pointer(BaseAddress, Offsets.ToArray());
    }

    public Pointer Adjust(int offset) {
      int[] newOffsets = Offsets.ToArray();
      newOffsets[newOffsets.Length - 1] += offset;
      return new Pointer(BaseAddress, newOffsets);
    }

    public Pointer AddOffsets(params int[] newOffsets) {
      int[] newPtrOffsets = new int[Offsets.Count + newOffsets.Length];
      Offsets.CopyTo(newPtrOffsets);
      newOffsets.CopyTo(newPtrOffsets, Offsets.Count);
      return new Pointer(BaseAddress, newPtrOffsets);
    }

    public Pointer RemoveOffsets(int amount) {
      if (amount >= Offsets.Count) {
        return new Pointer(BaseAddress);
      }

      return new Pointer(BaseAddress, Offsets.Take(Offsets.Count - amount).ToArray());
    }
  }
}
