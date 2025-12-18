using Be.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlatbufferToolkit.UI.HexView
{
    public static class HexBoxExtensions
    {
        public static byte[] GetSelectedBytes(this HexBox hexView)
        {
            long start = hexView.SelectionStart;
            long length = hexView.SelectionLength;

            if (length == 0) return Array.Empty<byte>();

            byte[] buffer = new byte[8];

            for (long i = 0; i < Math.Min(length, 8); i++)
            {
                buffer[i] = hexView.ByteProvider.ReadByte(start + i);
            }

            return buffer;
        }
    }
}
