using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices
{
	public static class Helper
	{
		internal class Allocation
		{
			public int Size { get; private set; }

			public object CachedData { get; private set; }

			public bool? IsCachedArrayElementAllocated { get; private set; }

			public Allocation(int size)
			{
				Size = size;
			}

			public void SetCachedData(object data, bool? isCachedArrayElementAllocated = null)
			{
				CachedData = data;
				IsCachedArrayElementAllocated = isCachedArrayElementAllocated;
			}
		}

		private class DelegateHolder
		{
			public Delegate Public { get; private set; }

			public Delegate Private { get; private set; }

			public Delegate[] StructDelegates { get; private set; }

			public ulong? NotificationId { get; set; }

			public DelegateHolder(Delegate publicDelegate, Delegate privateDelegate, params Delegate[] structDelegates)
			{
				Public = publicDelegate;
				Private = privateDelegate;
				StructDelegates = structDelegates;
			}
		}

		private static Dictionary<IntPtr, Allocation> s_Allocations = new Dictionary<IntPtr, Allocation>();

		private static Dictionary<IntPtr, DelegateHolder> s_Callbacks = new Dictionary<IntPtr, DelegateHolder>();

		private static Dictionary<string, DelegateHolder> s_StaticCallbacks = new Dictionary<string, DelegateHolder>();

		public static int GetAllocationCount()
		{
			return s_Allocations.Count;
		}

		internal static bool TryMarshalGet<T>(T[] source, out uint target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T target) where T : Handle, new()
		{
			return TryConvert<T>(source, out target);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(TSource source, out TTarget target) where TTarget : ISettable, new()
		{
			return TryConvert<TSource, TTarget>(source, out target);
		}

		internal static bool TryMarshalGet(int source, out bool target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(long source, out DateTimeOffset? target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			return TryFetch<T>(source, out target, arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength, bool isElementAllocated)
		{
			return TryFetch<T>(source, out target, (int)arrayLength, isElementAllocated);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, int arrayLength)
		{
			return TryMarshalGet<T>(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T[] target, uint arrayLength)
		{
			return TryMarshalGet<T>(source, out target, arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(TSource[] source, out TTarget[] target) where TSource : struct where TTarget : class, ISettable, new()
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet<TSource, TTarget>(IntPtr source, out TTarget[] target, int arrayLength) where TSource : struct where TTarget : class, ISettable, new()
		{
			target = GetDefault<TTarget[]>();
			if (TryMarshalGet(source, out TSource[] target2, arrayLength))
			{
				return TryMarshalGet(target2, out target);
			}
			return false;
		}

		internal static bool TryMarshalGet<TSource, TTarget>(IntPtr source, out TTarget[] target, uint arrayLength) where TSource : struct where TTarget : class, ISettable, new()
		{
			return TryMarshalGet<TSource, TTarget>(source, out target, (int)arrayLength);
		}

		internal static bool TryMarshalGet<T>(IntPtr source, out T? target) where T : struct
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet(byte[] source, out string target)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalGet(IntPtr source, out object target)
		{
			target = null;
			if (TryFetch(source, out BoxedData target2))
			{
				target = target2.Data;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalGet(IntPtr source, out string target)
		{
			return TryFetch(source, out target);
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<T>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalGet<TTarget, TEnum>(ISettable source, out TTarget target, TEnum currentEnum, TEnum comparisonEnum) where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryConvert<ISettable, TTarget>(source, out target);
			}
			return false;
		}

		internal static bool TryMarshalGet<TEnum>(int source, out bool? target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<bool?>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum && TryConvert(source, out var target2))
			{
				target = target2;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(T source, out T? target, TEnum currentEnum, TEnum comparisonEnum) where T : struct
		{
			target = GetDefault<T?>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				target = source;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalGet<T, TEnum>(IntPtr source, out T target, TEnum currentEnum, TEnum comparisonEnum) where T : Handle, new()
		{
			target = GetDefault<T>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}
			return false;
		}

		internal static bool TryMarshalGet<TEnum>(IntPtr source, out IntPtr? target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<IntPtr?>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}
			return false;
		}

		internal static bool TryMarshalGet<TEnum>(IntPtr source, out string target, TEnum currentEnum, TEnum comparisonEnum)
		{
			target = GetDefault<string>();
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryMarshalGet(source, out target);
			}
			return false;
		}

		internal static bool TryMarshalGet<TInternal, TPublic>(IntPtr source, out TPublic target) where TInternal : struct where TPublic : class, ISettable, new()
		{
			target = GetDefault<TPublic>();
			if (TryMarshalGet(source, out TInternal? target2) && target2.HasValue)
			{
				target = new TPublic();
				object other = target2;
				target.Set(other);
				return true;
			}
			return false;
		}

		internal static bool TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallbackInfo callbackInfo, out IntPtr clientDataAddress) where TCallbackInfoInternal : struct, ICallbackInfoInternal where TCallbackInfo : class, ISettable, new()
		{
			callbackInfo = null;
			clientDataAddress = IntPtr.Zero;
			if (TryFetch(callbackInfoAddress, out TCallbackInfoInternal target))
			{
				callbackInfo = new TCallbackInfo();
				object other = target;
				callbackInfo.Set(other);
				clientDataAddress = target.ClientDataAddress;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalSet<T>(ref T target, T source)
		{
			target = source;
			return true;
		}

		internal static bool TryMarshalSet<TTarget>(ref TTarget target, object source) where TTarget : ISettable, new()
		{
			return TryConvert<object, TTarget>(source, out target);
		}

		internal static bool TryMarshalSet(ref IntPtr target, Handle source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T? source) where T : struct
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			return TryAllocate(ref target, source, isElementAllocated);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source)
		{
			return TryMarshalSet(ref target, source, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out int arrayLength, bool isElementAllocated)
		{
			arrayLength = 0;
			if (TryMarshalSet(ref target, source, isElementAllocated))
			{
				arrayLength = source.Length;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength, bool isElementAllocated)
		{
			arrayLength = 0u;
			int arrayLength2 = 0;
			if (TryMarshalSet(ref target, source, out arrayLength2, isElementAllocated))
			{
				arrayLength = (uint)arrayLength2;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalSet<T>(ref IntPtr target, T[] source, out uint arrayLength)
		{
			return TryMarshalSet(ref target, source, out arrayLength, !typeof(T).IsValueType);
		}

		internal static bool TryMarshalSet(ref long target, DateTimeOffset? source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref int target, bool source)
		{
			return TryConvert(source, out target);
		}

		internal static bool TryMarshalSet(ref byte[] target, string source, int length)
		{
			return TryConvert(source, out target, length);
		}

		internal static bool TryMarshalSet(ref IntPtr target, string source)
		{
			return TryAllocate(ref target, source);
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);
				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return false;
		}

		internal static bool TryMarshalSet<TTarget, TEnum>(ref TTarget target, ISettable source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null) where TTarget : ISettable, new()
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);
				if (TryConvert<ISettable, TTarget>(source, out target))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return false;
		}

		internal static bool TryMarshalSet<T, TEnum>(ref T target, T? source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null) where T : struct
		{
			if (source.HasValue)
			{
				TryMarshalDispose(ref disposable);
				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref IntPtr target, Handle source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref disposable);
				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref IntPtr target, string source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source != null)
			{
				TryMarshalDispose(ref target);
				target = IntPtr.Zero;
				TryMarshalDispose(ref disposable);
				if (TryMarshalSet(ref target, source))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return true;
		}

		internal static bool TryMarshalSet<TEnum>(ref int target, bool? source, ref TEnum currentEnum, TEnum comparisonEnum, IDisposable disposable = null)
		{
			if (source.HasValue)
			{
				TryMarshalDispose(ref disposable);
				if (TryMarshalSet(ref target, source.Value))
				{
					currentEnum = comparisonEnum;
					return true;
				}
			}
			return true;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic source) where TInternal : struct, ISettable where TPublic : class
		{
			if (source != null)
			{
				TInternal source2 = new TInternal();
				source2.Set(source);
				if (TryAllocate(ref target, source2))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out int arrayLength) where TInternal : struct, ISettable where TPublic : class
		{
			arrayLength = 0;
			if (source != null)
			{
				TInternal[] array = new TInternal[source.Length];
				for (int i = 0; i < source.Length; i++)
				{
					array[i].Set(source[i]);
				}
				if (TryMarshalSet(ref target, array))
				{
					arrayLength = source.Length;
					return true;
				}
			}
			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out uint arrayLength) where TInternal : struct, ISettable where TPublic : class
		{
			arrayLength = 0u;
			if (Helper.TryMarshalSet<TInternal, TPublic>(ref target, source, out int arrayLength2))
			{
				arrayLength = (uint)arrayLength2;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out int arrayLength, bool isElementAllocated) where TInternal : struct, ISettable where TPublic : class
		{
			arrayLength = 0;
			if (source != null)
			{
				TInternal[] array = new TInternal[source.Length];
				for (int i = 0; i < source.Length; i++)
				{
					array[i].Set(source[i]);
				}
				if (TryMarshalSet(ref target, array, isElementAllocated))
				{
					arrayLength = source.Length;
					return true;
				}
			}
			return false;
		}

		internal static bool TryMarshalSet<TInternal, TPublic>(ref IntPtr target, TPublic[] source, out uint arrayLength, bool isElementAllocated) where TInternal : struct, ISettable where TPublic : class
		{
			arrayLength = 0u;
			if (Helper.TryMarshalSet<TInternal, TPublic>(ref target, source, out int arrayLength2, isElementAllocated))
			{
				arrayLength = (uint)arrayLength2;
				return true;
			}
			return false;
		}

		internal static bool TryMarshalCopy(IntPtr target, byte[] source)
		{
			if (target != IntPtr.Zero && source != null)
			{
				Marshal.Copy(source, 0, target, source.Length);
				return true;
			}
			return false;
		}

		internal static bool TryMarshalAllocate(ref IntPtr target, int size, out Allocation allocation)
		{
			TryMarshalDispose(ref target);
			target = Marshal.AllocHGlobal(size);
			Marshal.WriteByte(target, 0, 0);
			allocation = new Allocation(size);
			s_Allocations.Add(target, allocation);
			return true;
		}

		internal static bool TryMarshalAllocate(ref IntPtr target, uint size, out Allocation allocation)
		{
			return TryMarshalAllocate(ref target, (int)size, out allocation);
		}

		internal static bool TryMarshalDispose<TDisposable>(ref TDisposable disposable) where TDisposable : IDisposable
		{
			if (disposable != null)
			{
				disposable.Dispose();
				return true;
			}
			return false;
		}

		internal static bool TryMarshalDispose(ref IntPtr value)
		{
			return TryRelease(ref value);
		}

		internal static bool TryMarshalDispose<TEnum>(ref IntPtr member, TEnum currentEnum, TEnum comparisonEnum)
		{
			if ((int)(object)currentEnum == (int)(object)comparisonEnum)
			{
				return TryRelease(ref member);
			}
			return false;
		}

		internal static T GetDefault<T>()
		{
			return default(T);
		}

		internal static void AddCallback(ref IntPtr clientDataAddress, object clientData, Delegate publicDelegate, Delegate privateDelegate, params Delegate[] structDelegates)
		{
			TryAllocateCacheOnly(ref clientDataAddress, new BoxedData(clientData));
			s_Callbacks.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate, structDelegates));
		}

		internal static void AddStaticCallback(string key, Delegate publicDelegate, Delegate privateDelegate)
		{
			s_StaticCallbacks[key] = new DelegateHolder(publicDelegate, privateDelegate);
		}

		internal static bool TryAssignNotificationIdToCallback(IntPtr clientDataAddress, ulong notificationId)
		{
			if (notificationId != 0L)
			{
				DelegateHolder value = null;
				if (s_Callbacks.TryGetValue(clientDataAddress, out value))
				{
					value.NotificationId = notificationId;
					return true;
				}
			}
			else
			{
				s_Callbacks.Remove(clientDataAddress);
				TryRelease(ref clientDataAddress);
			}
			return false;
		}

		internal static bool TryRemoveCallbackByNotificationId(ulong notificationId)
		{
			IEnumerable<KeyValuePair<IntPtr, DelegateHolder>> source = s_Callbacks.Where((KeyValuePair<IntPtr, DelegateHolder> pair) => pair.Value.NotificationId.HasValue && pair.Value.NotificationId == notificationId);
			if (source.Any())
			{
				IntPtr target = source.First().Key;
				s_Callbacks.Remove(target);
				TryRelease(ref target);
				return true;
			}
			return false;
		}

		internal static bool TryGetAndRemoveCallback<TCallback, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TCallback callback, out TCallbackInfo callbackInfo) where TCallback : class where TCallbackInfoInternal : struct, ICallbackInfoInternal where TCallbackInfo : class, ICallbackInfo, ISettable, new()
		{
			callback = null;
			callbackInfo = null;
			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress) && TryGetAndRemoveCallback<TCallback, TCallbackInfo>(clientDataAddress, callbackInfo, out callback))
			{
				return true;
			}
			return false;
		}

		internal static bool TryGetStructCallback<TDelegate, TCallbackInfoInternal, TCallbackInfo>(IntPtr callbackInfoAddress, out TDelegate callback, out TCallbackInfo callbackInfo) where TDelegate : class where TCallbackInfoInternal : struct, ICallbackInfoInternal where TCallbackInfo : class, ISettable, new()
		{
			callback = null;
			callbackInfo = null;
			IntPtr clientDataAddress = IntPtr.Zero;
			if (TryMarshalGet<TCallbackInfoInternal, TCallbackInfo>(callbackInfoAddress, out callbackInfo, out clientDataAddress) && TryGetStructCallback<TDelegate>(clientDataAddress, out callback))
			{
				return true;
			}
			return false;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T source)
		{
			TryRelease(ref target);
			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}
			if (source == null)
			{
				return false;
			}
			if (!TryMarshalAllocate(ref target, Marshal.SizeOf(typeof(T)), out var allocation))
			{
				return false;
			}
			allocation.SetCachedData(source);
			Marshal.StructureToPtr(source, target, fDeleteOld: false);
			return true;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T? source) where T : struct
		{
			TryRelease(ref target);
			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}
			if (!source.HasValue)
			{
				return false;
			}
			return TryAllocate(ref target, source.Value);
		}

		private static bool TryAllocate(ref IntPtr target, string source)
		{
			TryRelease(ref target);
			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}
			if (source == null)
			{
				return false;
			}
			if (TryConvert(source, out var target2))
			{
				return TryAllocate(ref target, target2, isElementAllocated: false);
			}
			return false;
		}

		private static bool TryAllocate<T>(ref IntPtr target, T[] source, bool isElementAllocated)
		{
			TryRelease(ref target);
			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}
			if (source == null)
			{
				return false;
			}
			int num = 0;
			num = ((!isElementAllocated) ? Marshal.SizeOf(typeof(T)) : Marshal.SizeOf(typeof(IntPtr)));
			if (!TryMarshalAllocate(ref target, source.Length * num, out var allocation))
			{
				return false;
			}
			allocation.SetCachedData(source, isElementAllocated);
			for (int i = 0; i < source.Length; i++)
			{
				T val = (T)source.GetValue(i);
				if (isElementAllocated)
				{
					IntPtr target2 = IntPtr.Zero;
					if (typeof(T) == typeof(string))
					{
						TryAllocate(ref target2, (string)(object)val);
					}
					else if (typeof(T).BaseType == typeof(Handle))
					{
						TryConvert((Handle)(object)val, out target2);
					}
					else
					{
						TryAllocate(ref target2, val);
					}
					Marshal.StructureToPtr(ptr: new IntPtr(target.ToInt64() + i * num), structure: target2, fDeleteOld: false);
				}
				else
				{
					IntPtr ptr = new IntPtr(target.ToInt64() + i * num);
					Marshal.StructureToPtr(val, ptr, fDeleteOld: false);
				}
			}
			return true;
		}

		private static bool TryAllocateCacheOnly<T>(ref IntPtr target, T source)
		{
			TryRelease(ref target);
			if (target != IntPtr.Zero)
			{
				throw new ExternalAllocationException(target, source.GetType());
			}
			if (source == null)
			{
				return false;
			}
			if (!TryMarshalAllocate(ref target, 1, out var allocation))
			{
				return false;
			}
			allocation.SetCachedData(source);
			return true;
		}

		private static bool TryRelease(ref IntPtr target)
		{
			if (target == IntPtr.Zero)
			{
				return false;
			}
			Allocation value = null;
			if (!s_Allocations.TryGetValue(target, out value))
			{
				return false;
			}
			if (value.IsCachedArrayElementAllocated.HasValue)
			{
				int num = 0;
				num = ((!value.IsCachedArrayElementAllocated.Value) ? Marshal.SizeOf(value.CachedData.GetType().GetElementType()) : Marshal.SizeOf(typeof(IntPtr)));
				Array array = value.CachedData as Array;
				for (int i = 0; i < array.Length; i++)
				{
					if (value.IsCachedArrayElementAllocated.Value)
					{
						IntPtr ptr = new IntPtr(target.ToInt64() + i * num);
						ptr = Marshal.ReadIntPtr(ptr);
						TryRelease(ref ptr);
						continue;
					}
					object value2 = array.GetValue(i);
					if (value2 is IDisposable && value2 is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			if (value.CachedData is IDisposable && value.CachedData is IDisposable disposable2)
			{
				disposable2.Dispose();
			}
			Marshal.FreeHGlobal(target);
			s_Allocations.Remove(target);
			target = IntPtr.Zero;
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T target)
		{
			target = GetDefault<T>();
			if (source == IntPtr.Zero)
			{
				return false;
			}
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T))
					{
						target = (T)allocation.CachedData;
						return true;
					}
					throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T));
				}
			}
			target = (T)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T? target) where T : struct
		{
			target = GetDefault<T?>();
			if (source == IntPtr.Zero)
			{
				return false;
			}
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T))
					{
						target = (T?)allocation.CachedData;
						return true;
					}
					throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T));
				}
			}
			target = (T?)Marshal.PtrToStructure(source, typeof(T));
			return true;
		}

		private static bool TryFetch<T>(IntPtr source, out T[] target, int arrayLength, bool isElementAllocated)
		{
			target = null;
			if (source == IntPtr.Zero)
			{
				return false;
			}
			if (s_Allocations.ContainsKey(source))
			{
				Allocation allocation = s_Allocations[source];
				if (allocation.CachedData != null)
				{
					if (allocation.CachedData.GetType() == typeof(T[]))
					{
						Array array = (Array)allocation.CachedData;
						if (array.Length == arrayLength)
						{
							target = array as T[];
							return true;
						}
						throw new CachedArrayAllocationException(source, array.Length, arrayLength);
					}
					throw new CachedTypeAllocationException(source, allocation.CachedData.GetType(), typeof(T[]));
				}
			}
			int num = 0;
			num = ((!isElementAllocated) ? Marshal.SizeOf(typeof(T)) : Marshal.SizeOf(typeof(IntPtr)));
			List<T> list = new List<T>();
			for (int i = 0; i < arrayLength; i++)
			{
				IntPtr intPtr = new IntPtr(source.ToInt64() + i * num);
				if (isElementAllocated)
				{
					intPtr = Marshal.ReadIntPtr(intPtr);
				}
				TryFetch(intPtr, out T target2);
				list.Add(target2);
			}
			target = list.ToArray();
			return true;
		}

		private static bool TryFetch(IntPtr source, out string target)
		{
			target = null;
			if (source == IntPtr.Zero)
			{
				return false;
			}
			int i;
			for (i = 0; Marshal.ReadByte(source, i) != 0; i++)
			{
			}
			byte[] array = new byte[i];
			Marshal.Copy(source, array, 0, i);
			target = Encoding.UTF8.GetString(array);
			return true;
		}

		private static bool TryConvert<THandle>(IntPtr source, out THandle target) where THandle : Handle, new()
		{
			target = null;
			if (source != IntPtr.Zero)
			{
				target = new THandle();
				target.InnerHandle = source;
			}
			return true;
		}

		internal static bool TryConvert<TSource, TTarget>(TSource source, out TTarget target) where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget>();
			if (source != null)
			{
				target = new TTarget();
				target.Set(source);
			}
			return true;
		}

		private static bool TryConvert(Handle source, out IntPtr target)
		{
			target = IntPtr.Zero;
			if (source != null)
			{
				target = source.InnerHandle;
			}
			return true;
		}

		private static bool TryConvert(byte[] source, out string target)
		{
			target = null;
			if (source == null)
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < source.Length && source[i] != 0; i++)
			{
				num++;
			}
			target = Encoding.UTF8.GetString(source.Take(num).ToArray());
			return true;
		}

		private static bool TryConvert(string source, out byte[] target, int length)
		{
			if (source == null)
			{
				source = "";
			}
			target = Encoding.UTF8.GetBytes(new string(source.Take(length).ToArray()).PadRight(length, '\0'));
			return true;
		}

		private static bool TryConvert(string source, out byte[] target)
		{
			return TryConvert(source, out target, source.Length + 1);
		}

		private static bool TryConvert<T>(T[] source, out int target)
		{
			target = 0;
			if (source != null)
			{
				target = source.Length;
			}
			return true;
		}

		private static bool TryConvert<T>(T[] source, out uint target)
		{
			target = 0u;
			if (TryConvert(source, out int target2))
			{
				target = (uint)target2;
				return true;
			}
			return false;
		}

		internal static bool TryConvert<TSource, TTarget>(TSource[] source, out TTarget[] target) where TTarget : ISettable, new()
		{
			target = GetDefault<TTarget[]>();
			if (source != null)
			{
				target = new TTarget[source.Length];
				for (int i = 0; i < source.Length; i++)
				{
					target[i] = new TTarget();
					ref readonly TTarget reference = ref target[i];
					object other = source[i];
					reference.Set(other);
				}
			}
			return true;
		}

		private static bool TryConvert(int source, out bool target)
		{
			target = source != 0;
			return true;
		}

		private static bool TryConvert(bool source, out int target)
		{
			target = (source ? 1 : 0);
			return true;
		}

		private static bool TryConvert(DateTimeOffset? source, out long target)
		{
			target = -1L;
			if (source.HasValue)
			{
				DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				long num = (source.Value.UtcDateTime - dateTime).Ticks / 10000000;
				target = num;
			}
			return true;
		}

		private static bool TryConvert(long source, out DateTimeOffset? target)
		{
			target = null;
			if (source >= 0)
			{
				DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				long num = source * 10000000;
				target = new DateTimeOffset(dateTime.Ticks + num, TimeSpan.Zero);
			}
			return true;
		}

		private static bool CanRemoveCallback<TCallbackInfo>(IntPtr clientDataAddress, TCallbackInfo callbackInfo) where TCallbackInfo : ICallbackInfo
		{
			DelegateHolder value = null;
			if (s_Callbacks.TryGetValue(clientDataAddress, out value) && value.NotificationId.HasValue)
			{
				return false;
			}
			if (callbackInfo.GetResultCode().HasValue)
			{
				return Common.IsOperationComplete(callbackInfo.GetResultCode().Value);
			}
			return true;
		}

		private static bool TryGetAndRemoveCallback<TCallback, TCallbackInfo>(IntPtr clientDataAddress, TCallbackInfo callbackInfo, out TCallback callback) where TCallback : class where TCallbackInfo : ICallbackInfo
		{
			callback = null;
			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				callback = s_Callbacks[clientDataAddress].Public as TCallback;
				if (callback != null)
				{
					if (CanRemoveCallback(clientDataAddress, callbackInfo))
					{
						s_Callbacks.Remove(clientDataAddress);
						TryRelease(ref clientDataAddress);
					}
					return true;
				}
			}
			return false;
		}

		internal static bool TryGetStaticCallback<TCallback>(string key, out TCallback callback) where TCallback : class
		{
			callback = null;
			if (s_StaticCallbacks.ContainsKey(key))
			{
				callback = s_StaticCallbacks[key].Public as TCallback;
				if (callback != null)
				{
					return true;
				}
			}
			return false;
		}

		private static bool TryGetStructCallback<TCallback>(IntPtr clientDataAddress, out TCallback structCallback) where TCallback : class
		{
			structCallback = null;
			if (clientDataAddress != IntPtr.Zero && s_Callbacks.ContainsKey(clientDataAddress))
			{
				structCallback = s_Callbacks[clientDataAddress].StructDelegates.FirstOrDefault((Delegate delegat) => delegat.GetType() == typeof(TCallback)) as TCallback;
				if (structCallback != null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
