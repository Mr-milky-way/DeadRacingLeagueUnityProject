using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

namespace PS4.UsbPeripherals
{
	public static class USBDInputPlugin
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ActiveStatusCallback(int stat, int vid, int pid, s_outputMapExp map);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DeviceDataCallback(s_simpleMsg msg);

		private static bool deviceActiveFlag = false;

		private static int activeDeviceVid = -1;

		private static int activeDevicePid = -1;

		private static readonly object deviceStatusLock = new object();

		private static readonly object deviceDataLock = new object();

		private static s_simpleMsg deviceData;

		private static s_outputMapExp deviceMap;

		[MonoPInvokeCallback(typeof(ActiveStatusCallback))]
		public static void ActiveDeviceCb(int stat, int vid, int pid, s_outputMapExp map)
		{
			lock (deviceStatusLock)
			{
				activeDeviceVid = vid;
				activeDevicePid = pid;
				deviceMap = map;
				if (stat == 1)
				{
					deviceActiveFlag = true;
				}
				else
				{
					deviceActiveFlag = false;
				}
			}
		}

		[MonoPInvokeCallback(typeof(DeviceDataCallback))]
		public static void DeviceDataCb(s_simpleMsg msg)
		{
			lock (deviceDataLock)
			{
				deviceData = msg;
			}
		}

		[DllImport("drlGamepadInterface")]
		private static extern int prxDrlGamepadInput_initialize([MarshalAs(UnmanagedType.FunctionPtr)] ActiveStatusCallback activeStatCbPtr, [MarshalAs(UnmanagedType.FunctionPtr)] DeviceDataCallback devDataCbPtr);

		public static int InitializePlugin()
		{
			ActiveStatusCallback activeStatCbPtr = ActiveDeviceCb;
			DeviceDataCallback devDataCbPtr = DeviceDataCb;
			int num = prxDrlGamepadInput_initialize(activeStatCbPtr, devDataCbPtr);
			if (num != 1)
			{
				Console.WriteLine("[PLUGIN] Initialization failed with code {0}", num);
				throw new Exception("[PLUGIN] Failed to initialize USBD library and gamepad plugin.");
			}
			return num;
		}

		[DllImport("drlGamepadInterface")]
		private static extern int prxDrlGamepadInput_finalize();

		public static int FinalizePlugin()
		{
			int num = prxDrlGamepadInput_finalize();
			if (num != 1)
			{
				Console.WriteLine("[PLUGIN] Finalization failed with code {0}", num);
			}
			return num;
		}

		[DllImport("drlGamepadInterface")]
		private static extern int prxDrlGamepadInput_setActiveDev(int vid, int pid);

		public static int SetActiveDevice(int vid, int pid)
		{
			return prxDrlGamepadInput_setActiveDev(vid, pid);
		}

		[DllImport("drlGamepadInterface")]
		private static extern int prxDrlGamepadInput_unsetActiveDevice();

		public static int UnsetActiveDevice()
		{
			return prxDrlGamepadInput_unsetActiveDevice();
		}

		public static bool IsDeviceActive()
		{
			lock (deviceStatusLock)
			{
				return deviceActiveFlag;
			}
		}

		public static s_vidPidPair GetActiveDeviceVidPid()
		{
			s_vidPidPair result = new s_vidPidPair
			{
				vid = 0,
				pid = 0
			};
			if (deviceActiveFlag)
			{
				lock (deviceStatusLock)
				{
					result.vid = activeDeviceVid;
					result.pid = activeDevicePid;
				}
			}
			return result;
		}

		public static s_outputMapExp GetActiveDeviceMap()
		{
			s_outputMapExp result = new s_outputMapExp
			{
				numButtons = 0,
				numJoysticks = 0
			};
			if (deviceActiveFlag)
			{
				lock (deviceStatusLock)
				{
					result.numButtons = deviceMap.numButtons;
					result.numJoysticks = deviceMap.numJoysticks;
					result.channels = deviceMap.channels;
				}
			}
			return result;
		}

		[DllImport("drlGamepadInterface")]
		private static extern int prxDrlGamepadInput_getActiveDeviceData(int timeoutMs);

		public static s_simpleMsg GetActiveDeviceDataRaw(int timeout = 5)
		{
			s_simpleMsg result = new s_simpleMsg
			{
				length = 0
			};
			if (deviceActiveFlag && prxDrlGamepadInput_getActiveDeviceData(timeout) == 1)
			{
				lock (deviceDataLock)
				{
					result = deviceData;
				}
			}
			return result;
		}

		private static int GetIntFromBitArray(BitArray bitArray)
		{
			if (bitArray.Length > 32)
			{
				return -1;
			}
			int[] array = new int[1];
			bitArray.CopyTo(array, 0);
			return array[0];
		}

		private static byte GetByteFromBitArray(BitArray bitArray)
		{
			if (bitArray.Length > 8)
			{
				return 0;
			}
			byte[] array = new byte[1];
			bitArray.CopyTo(array, 0);
			return array[0];
		}

		public static s_structuredDeviceData GetActiveDeviceStructuredData(int timeout = 20)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (deviceActiveFlag)
			{
				s_simpleMsg activeDeviceDataRaw = GetActiveDeviceDataRaw(timeout);
				s_outputMapExp activeDeviceMap = GetActiveDeviceMap();
				if (activeDeviceDataRaw.length == 0)
				{
					return new s_structuredDeviceData(0, 0, default(s_simpleMsg), default(s_outputMapExp), p_doubleBytePrecision: false);
				}
				byte[] array = new byte[activeDeviceDataRaw.length];
				Array.Copy(activeDeviceDataRaw.data, 0, array, 0, activeDeviceDataRaw.length);
				BitArray bitArray = new BitArray(array);
				s_structuredDeviceData result = new s_structuredDeviceData(activeDeviceMap.numButtons, activeDeviceMap.numJoysticks, activeDeviceDataRaw, activeDeviceMap, p_doubleBytePrecision: false);
				for (int i = 0; i < activeDeviceMap.numButtons + activeDeviceMap.numJoysticks; i++)
				{
					if (activeDeviceMap.channels[i].chType == 0)
					{
						num = activeDeviceMap.channels[i].button.reportStartBit;
						num2 = activeDeviceMap.channels[i].button.reportStopBit;
						BitArray bitArray2 = new BitArray(num2 - num);
						num3 = 0;
						for (int j = num; j < num2; j++)
						{
							bitArray2[num3] = bitArray[j];
							num3++;
						}
						result.buttons.Add(new s_structuredButtonData(activeDeviceMap.channels[i].button.numberOfBtns, GetIntFromBitArray(bitArray2)));
					}
					else if (activeDeviceMap.channels[i].chType == 1)
					{
						num = activeDeviceMap.channels[i].joystick.reportStartBit;
						num2 = activeDeviceMap.channels[i].joystick.reportStopBit;
						BitArray bitArray3 = new BitArray(num2 - num);
						num3 = 0;
						for (int k = num; k < num2; k++)
						{
							bitArray3[num3] = bitArray[k];
							num3++;
						}
						int num4 = GetIntFromBitArray(bitArray3);
						if (activeDeviceMap.channels[i].joystick.logicalMin >= activeDeviceMap.channels[i].joystick.logicalMax)
						{
							num4 = (sbyte)GetByteFromBitArray(bitArray3);
						}
						result.doubleBytePrecision = activeDeviceMap.channels[i].joystick.logicalMax > 255;
						result.joysticks.Add(new s_structuredJoystickData(activeDeviceMap.channels[i].joystick.usage, activeDeviceMap.channels[i].joystick.logicalMax, activeDeviceMap.channels[i].joystick.logicalMin, num4));
					}
				}
				return result;
			}
			return new s_structuredDeviceData(0, 0, default(s_simpleMsg), default(s_outputMapExp), p_doubleBytePrecision: false);
		}
	}
}
