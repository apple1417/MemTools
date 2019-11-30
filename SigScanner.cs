using System;
using System.Linq;

namespace MemTools {
  public static class SigScanner {
    public static IntPtr SigScan(this MemManager manager, IntPtr start, int len, int offset, params string[] pattern) {
      // Join all strings and remove whitespace
      string joinedPattern = new string(string.Join("", pattern)
                                              .Where(c => !char.IsWhiteSpace(c))
                                              .ToArray());

      if ((joinedPattern.Length % 2) != 0) {
        throw new ArgumentException("Recived a hex string with an odd amount of characters!");
      }

      byte[] outputPattern = new byte[joinedPattern.Length / 2];
      bool[] outputMask = new bool[joinedPattern.Length / 2];

      for (int i = 0; i < outputPattern.Length; i++) {
        try {
          outputPattern[i] = byte.Parse(joinedPattern.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
          outputMask[i] = true;
        } catch (FormatException) {
          outputPattern[i] = 0;
          outputMask[i] = false;
        }
      }

      return manager.SigScan(start, len, offset, outputPattern, outputMask);
    }

    public static IntPtr SigScan(this MemManager manager, IntPtr start, int len, int offset, byte[] pattern, bool[] mask) {
      if (pattern.Length != mask.Length) {
        throw new ArgumentException("Pattern and mask must be the same length!");
      }

      byte[] memory = manager.Read(start, len);

      // Just using a naive O(nm) search, it's fast enough
      for (int i = 0; i < memory.Length - pattern.Length; i++) {
        bool found = true;
        for (int j = 0; j < pattern.Length; j++) {
          if (!mask[j]) {
            continue;
          }
          if (memory[i + j] != pattern[j]) {
            found = false;
            break;
          }
        }
        if (found) {
          return IntPtr.Add(start, i + offset);
        }
      }

      return IntPtr.Zero;
    }
  }
}
