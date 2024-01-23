using System.Diagnostics;
using System.Runtime.InteropServices;
using PacketDotNet;
using PacketDotNet.Utils.Converters;
using SharpPcap;
using SharpPcap.LibPcap;

namespace csmc
{
	class Program
	{
		#pragma warning disable 0649
		struct TelemHeader
		{
			public UInt32 msg_id;
			public UInt32 flags;
			public UInt32 num_schema_frags;
			public UInt32 num_atomic_frags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] schema_hash;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public char[] schema_frag;
			public UInt32 schema_idx;
			public UInt32 atomic_idx;
			public Int64 timestamp;

			public override string ToString()
			{
				return String.Format("MsgId: {0}, Schema: {1} / {2}, SchemaHash: {3}", msg_id, schema_idx, num_schema_frags, schema_hash);
			}
		}
		#pragma warning restore 0649

		private static Dictionary<string, int> schema_frags = new Dictionary<string, int>();

		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: dotnet run <telem file>");
				return;
			}
			
			//string file = "/home/tsharpe/investigations/csmc/data_trimmed_lowspeed.pcap";
			string file = args[0];

			using var device = new CaptureFileReaderDevice(file);
			device.Open();
			device.OnPacketArrival += Device_OnPacketArrival;
			device.Capture();
		}

		static int packetNumber = 0;
		private static void Device_OnPacketArrival(object s, PacketCapture e)
		{
			packetNumber++;

			RawCapture rawPacket = e.GetPacket();
			Packet packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
			UdpPacket udpPacket = packet.Extract<UdpPacket>();

			byte[] data = udpPacket.PayloadData;
			
			TelemHeader header = data.ToStruct<TelemHeader>(Endianness.BigEndian);
			string hash = Convert.ToHexString(header.schema_hash);
			if (!schema_frags.ContainsKey(hash))
				schema_frags.Add(hash, (int)header.schema_idx);
			else
			{
				int expected = schema_frags[hash] + 1;
				if (expected != header.schema_idx)
				{
					Console.WriteLine("{0} - Missing idx for schema: {1}, expected {2} at {3:X}", packetNumber, hash, expected, header.timestamp);
					Console.WriteLine("    Last seen: {0}, Current:{1}", schema_frags[hash], header.schema_idx);
				}
				schema_frags[hash] = (int)header.schema_idx;
			}

		}
	}
}
