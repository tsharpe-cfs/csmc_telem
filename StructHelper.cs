using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using PacketDotNet.Utils.Converters;

namespace csmc
{
	public static class StructHelper
	{
		public static T ToStruct<T>(this byte[] data, Endianness source) where T : struct
		{
			UpdateEndianness(typeof(T), data, source);
			var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
#pragma warning disable 8605
			var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
#pragma warning restore 8605
			pData.Free();
			return result;
		}

		public static byte[] ToArray<T>(this T data, Endianness source) where T : struct
		{
			var result = new byte[Marshal.SizeOf(typeof(T))];
			var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
			Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
			pResult.Free();
			UpdateEndianness(typeof(T), result, source);
			return result;
		}

		private static void UpdateEndianness(Type type, byte[] data, Endianness source)
		{
			if (BitConverter.IsLittleEndian && (source == Endianness.LittleEndian))
				return;
			
			foreach (var field in type.GetFields())
			{
				if (!field.FieldType.IsPrimitive)
					continue;

				int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
				Array.Reverse(data, offset, Marshal.SizeOf(field.FieldType));
			}
		}
	}
}