using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdmiExtenderLib
{
	/// <summary>
	/// Accepts data blocks from Lenkeng HDMI Extender Sender devices, and returns completed images when available.
	/// </summary>
	public class JpegAssembler
	{
		private FragmentedJpeg currentFrame;
		/// <summary>
		/// This temporarily holds an unfinished frame in the event of UDP packets getting mixed up.
		/// </summary>
		private FragmentedJpeg previousFrame;

		public byte[] AddChunkAndGetNextImage(byte[] data)
		{
			if(data.Length <= 4)
				return null;

			ushort frameNumber = BitConverter.ToUInt16(data.Take(2).Reverse().ToArray<byte>(), 0);
			if (currentFrame == null)
			{
				// We have no current frame, so either we just started or we just finished a current frame and need to create a new one.
				currentFrame = new FragmentedJpeg(frameNumber);
				currentFrame.AddFragment(data);
			}
			else if (currentFrame.frameNumber == frameNumber)
			{
				// This is the current frame, so simply add the chunk to it.
				currentFrame.AddFragment(data);
			}
			else
			{
				// This frame number is not our current frame.
				if (previousFrame != null && previousFrame.frameNumber == frameNumber)
				{
					// This chunk belongs to the previous frame (i.e. a chunk arrived out of order)
					previousFrame.AddFragment(data);
				}
				else
				{
					// Our current frame is not yet finished, but we need to move on to a new frame.
					// Shift the current frame to the previous frame position, and make this chunk belong to the new current frame.
					if (previousFrame != null)
						Console.WriteLine("netdrop2");
					previousFrame = currentFrame;
					currentFrame = new FragmentedJpeg(frameNumber);
					currentFrame.AddFragment(data);
				}
			}
			if (currentFrame.isComplete)
			{
				byte[] imgData = currentFrame.finalData;
				currentFrame = null; // The current frame just finished 
				if (previousFrame != null)
					Console.WriteLine("netdrop1");
				previousFrame = null; // the previous frame is now too old as well
				return imgData;
			}
			if (previousFrame != null && previousFrame.isComplete)
			{
				byte[] imgData = previousFrame.finalData;
				previousFrame = null;
				return imgData;
			}
			return null;
		}
	}
	/// <summary>
	/// Stores and assembles chunks of a jpeg image that may arrive out of order from a Lenkeng HDMI over IP Extender.
	/// </summary>
	public class FragmentedJpeg
	{
		public ushort frameNumber;
		public bool isComplete = false;
		public byte[] finalData;

		private int totalLength = 0;

		private bool hasReceivedFinalChunk = false;
		/// <summary>
		/// This is set when the final chunk of the image is received.  This does not mean that the image is complete; merely that the chunk marked as final has arrived.  Some chunks may be lost or out of order, so this FragmentedJpeg may be completed later, or never.
		/// </summary>
		private ushort? finalChunkNumber = null;

		/// <summary>
		/// Ensures we do not add the same chunk twice due to duplication for any reason.
		/// </summary>
		private HashSet<ushort> receivedChunks = new HashSet<ushort>();
		private List<JpegChunk> chunks = new List<JpegChunk>();

		public FragmentedJpeg(ushort frameNumber)
		{
			this.frameNumber = frameNumber;
		}

		/// <summary>
		/// Adds the specified data fragment and returns true if this FragmentedJpeg is complete.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddFragment(byte[] data)
		{
			if(!isComplete)
			{
				JpegChunk chunk = new JpegChunk(data);
				if (!receivedChunks.Contains(chunk.chunkNumber))
				{
					receivedChunks.Add(chunk.chunkNumber);
					chunks.Add(chunk);
					totalLength += chunk.data.Length - 4;
					if (chunk.isFinalChunk)
					{
						finalChunkNumber = chunk.chunkNumber;
						//Console.WriteLine("Got final chunk for " + frameNumber);
					}
					if (finalChunkNumber != null)
					{
						if (chunks.Count == finalChunkNumber + 1)
						{
							chunks.Sort();
							int idx = 0;
							finalData = new byte[totalLength];
							foreach (JpegChunk c in chunks)
							{
								Array.Copy(c.data, 4, finalData, idx, c.data.Length - 4);
								idx += c.data.Length - 4;
							}
							isComplete = true;
						}
					}
				}
			}
			return isComplete;
		}

		private class JpegChunk : IComparable<JpegChunk>
		{
			private static ushort maskMSB = Convert.ToUInt16("1000000000000000", 2);
			private static ushort maskMSB_Inverse = Convert.ToUInt16("0111111111111111", 2);

			public ushort chunkNumber;
			public byte[] data;
			public bool isFinalChunk;

			public JpegChunk(byte[] data)
			{
				this.data = data;
				this.chunkNumber = BitConverter.ToUInt16(data.Skip(2).Take(2).Reverse().ToArray<byte>(), 0);
				// The most significant bit of the chunk number will be set if this is the final chunk.
				this.isFinalChunk = (chunkNumber & maskMSB) > 0;
				if (isFinalChunk)
					chunkNumber = (ushort)(chunkNumber & maskMSB_Inverse);
			}

			public override string ToString()
			{
				return chunkNumber.ToString();
			}

			public int CompareTo(JpegChunk other)
			{
				return this.chunkNumber.CompareTo(other.chunkNumber);
			}
		}
	}
}
