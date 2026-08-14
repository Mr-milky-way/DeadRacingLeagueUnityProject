using System;

public class AkPlaylistArray : IDisposable
{
	private IntPtr swigCPtr;

	protected bool swigCMemOwn;

	internal AkPlaylistArray(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = cPtr;
	}

	internal static IntPtr getCPtr(AkPlaylistArray obj)
	{
		return obj?.swigCPtr ?? IntPtr.Zero;
	}

	internal virtual void setCPtr(IntPtr cPtr)
	{
		Dispose();
		swigCPtr = cPtr;
	}

	~AkPlaylistArray()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					AkSoundEnginePINVOKE.CSharp_delete_AkPlaylistArray(swigCPtr);
				}
				swigCPtr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}
	}

	public AkPlaylistArray()
		: this(AkSoundEnginePINVOKE.CSharp_new_AkPlaylistArray(), cMemoryOwn: true)
	{
	}

	public AkIterator Begin()
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Begin(swigCPtr), cMemoryOwn: true);
	}

	public AkIterator End()
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_End(swigCPtr), cMemoryOwn: true);
	}

	public AkIterator FindEx(AkPlaylistItem in_Item)
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_FindEx(swigCPtr, AkPlaylistItem.getCPtr(in_Item)), cMemoryOwn: true);
	}

	public AkIterator Erase(AkIterator in_rIter)
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Erase__SWIG_0(swigCPtr, AkIterator.getCPtr(in_rIter)), cMemoryOwn: true);
	}

	public void Erase(uint in_uIndex)
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Erase__SWIG_1(swigCPtr, in_uIndex);
	}

	public AkIterator EraseSwap(AkIterator in_rIter)
	{
		return new AkIterator(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_EraseSwap(swigCPtr, AkIterator.getCPtr(in_rIter)), cMemoryOwn: true);
	}

	public bool IsGrowingAllowed()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_IsGrowingAllowed(swigCPtr);
	}

	public AKRESULT Reserve(uint in_ulReserve)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Reserve(swigCPtr, in_ulReserve);
	}

	public uint Reserved()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Reserved(swigCPtr);
	}

	public void Term()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Term(swigCPtr);
	}

	public uint Length()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Length(swigCPtr);
	}

	public AkPlaylistItem Data()
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Data(swigCPtr);
		if (!(intPtr == IntPtr.Zero))
		{
			return new AkPlaylistItem(intPtr, cMemoryOwn: false);
		}
		return null;
	}

	public bool IsEmpty()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_IsEmpty(swigCPtr);
	}

	public AkPlaylistItem Exists(AkPlaylistItem in_Item)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Exists(swigCPtr, AkPlaylistItem.getCPtr(in_Item));
		if (!(intPtr == IntPtr.Zero))
		{
			return new AkPlaylistItem(intPtr, cMemoryOwn: false);
		}
		return null;
	}

	public AkPlaylistItem AddLast()
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_AddLast__SWIG_0(swigCPtr);
		if (!(intPtr == IntPtr.Zero))
		{
			return new AkPlaylistItem(intPtr, cMemoryOwn: false);
		}
		return null;
	}

	public AkPlaylistItem AddLast(AkPlaylistItem in_rItem)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_AddLast__SWIG_1(swigCPtr, AkPlaylistItem.getCPtr(in_rItem));
		if (!(intPtr == IntPtr.Zero))
		{
			return new AkPlaylistItem(intPtr, cMemoryOwn: false);
		}
		return null;
	}

	public AkPlaylistItem Last()
	{
		return new AkPlaylistItem(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Last(swigCPtr), cMemoryOwn: false);
	}

	public void RemoveLast()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveLast(swigCPtr);
	}

	public AKRESULT Remove(AkPlaylistItem in_rItem)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Remove(swigCPtr, AkPlaylistItem.getCPtr(in_rItem));
	}

	public AKRESULT RemoveSwap(AkPlaylistItem in_rItem)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveSwap(swigCPtr, AkPlaylistItem.getCPtr(in_rItem));
	}

	public void RemoveAll()
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_RemoveAll(swigCPtr);
	}

	public AkPlaylistItem ItemAtIndex(uint uiIndex)
	{
		return new AkPlaylistItem(AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_ItemAtIndex(swigCPtr, uiIndex), cMemoryOwn: false);
	}

	public AkPlaylistItem Insert(uint in_uIndex)
	{
		IntPtr intPtr = AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Insert(swigCPtr, in_uIndex);
		if (!(intPtr == IntPtr.Zero))
		{
			return new AkPlaylistItem(intPtr, cMemoryOwn: false);
		}
		return null;
	}

	public bool GrowArray()
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_GrowArray__SWIG_0(swigCPtr);
	}

	public bool GrowArray(uint in_uGrowBy)
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_GrowArray__SWIG_1(swigCPtr, in_uGrowBy);
	}

	public bool Resize(uint in_uiSize)
	{
		return AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Resize(swigCPtr, in_uiSize);
	}

	public void Transfer(AkPlaylistArray in_rSource)
	{
		AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Transfer(swigCPtr, getCPtr(in_rSource));
	}

	public AKRESULT Copy(AkPlaylistArray in_rSource)
	{
		return (AKRESULT)AkSoundEnginePINVOKE.CSharp_AkPlaylistArray_Copy(swigCPtr, getCPtr(in_rSource));
	}
}
