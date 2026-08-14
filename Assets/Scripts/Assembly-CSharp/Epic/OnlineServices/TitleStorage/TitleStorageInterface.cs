using System;

namespace Epic.OnlineServices.TitleStorage
{
	public sealed class TitleStorageInterface : Handle
	{
		public const int CopyfilemetadataatindexoptionsApiLatest = 1;

		public const int CopyfilemetadatabyfilenameoptionsApiLatest = 1;

		public const int DeletecacheoptionsApiLatest = 1;

		public const int FilemetadataApiLatest = 2;

		public const int FilenameMaxLengthBytes = 64;

		public const int GetfilemetadatacountoptionsApiLatest = 1;

		public const int QueryfilelistoptionsApiLatest = 1;

		public const int QueryfileoptionsApiLatest = 1;

		public const int ReadfileoptionsApiLatest = 1;

		public TitleStorageInterface()
		{
		}

		public TitleStorageInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyFileMetadataAtIndex(CopyFileMetadataAtIndexOptions options, out FileMetadata outMetadata)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyFileMetadataAtIndexOptionsInternal, CopyFileMetadataAtIndexOptions>(ref target, options);
			IntPtr outMetadata2 = IntPtr.Zero;
			Result result = Bindings.EOS_TitleStorage_CopyFileMetadataAtIndex(base.InnerHandle, target, ref outMetadata2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
			{
				Bindings.EOS_TitleStorage_FileMetadata_Release(outMetadata2);
			}
			return result;
		}

		public Result CopyFileMetadataByFilename(CopyFileMetadataByFilenameOptions options, out FileMetadata outMetadata)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyFileMetadataByFilenameOptionsInternal, CopyFileMetadataByFilenameOptions>(ref target, options);
			IntPtr outMetadata2 = IntPtr.Zero;
			Result result = Bindings.EOS_TitleStorage_CopyFileMetadataByFilename(base.InnerHandle, target, ref outMetadata2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
			{
				Bindings.EOS_TitleStorage_FileMetadata_Release(outMetadata2);
			}
			return result;
		}

		public Result DeleteCache(DeleteCacheOptions options, object clientData, OnDeleteCacheCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DeleteCacheOptionsInternal, DeleteCacheOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDeleteCacheCompleteCallbackInternal onDeleteCacheCompleteCallbackInternal = OnDeleteCacheCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onDeleteCacheCompleteCallbackInternal);
			Result result = Bindings.EOS_TitleStorage_DeleteCache(base.InnerHandle, target, clientDataAddress, onDeleteCacheCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public uint GetFileMetadataCount(GetFileMetadataCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetFileMetadataCountOptionsInternal, GetFileMetadataCountOptions>(ref target, options);
			uint result = Bindings.EOS_TitleStorage_GetFileMetadataCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryFile(QueryFileOptions options, object clientData, OnQueryFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryFileOptionsInternal, QueryFileOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryFileCompleteCallbackInternal onQueryFileCompleteCallbackInternal = OnQueryFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onQueryFileCompleteCallbackInternal);
			Bindings.EOS_TitleStorage_QueryFile(base.InnerHandle, target, clientDataAddress, onQueryFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryFileList(QueryFileListOptions options, object clientData, OnQueryFileListCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryFileListOptionsInternal, QueryFileListOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryFileListCompleteCallbackInternal onQueryFileListCompleteCallbackInternal = OnQueryFileListCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onQueryFileListCompleteCallbackInternal);
			Bindings.EOS_TitleStorage_QueryFileList(base.InnerHandle, target, clientDataAddress, onQueryFileListCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public TitleStorageFileTransferRequest ReadFile(ReadFileOptions options, object clientData, OnReadFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReadFileOptionsInternal, ReadFileOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnReadFileCompleteCallbackInternal onReadFileCompleteCallbackInternal = OnReadFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onReadFileCompleteCallbackInternal, options.ReadFileDataCallback, ReadFileOptionsInternal.ReadFileDataCallback, options.FileTransferProgressCallback, ReadFileOptionsInternal.FileTransferProgressCallback);
			IntPtr source = Bindings.EOS_TitleStorage_ReadFile(base.InnerHandle, target, clientDataAddress, onReadFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out TitleStorageFileTransferRequest target2);
			return target2;
		}

		[MonoPInvokeCallback(typeof(OnDeleteCacheCompleteCallbackInternal))]
		internal static void OnDeleteCacheCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDeleteCacheCompleteCallback, DeleteCacheCallbackInfoInternal, DeleteCacheCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnFileTransferProgressCallbackInternal))]
		internal static void OnFileTransferProgressCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetStructCallback<OnFileTransferProgressCallback, FileTransferProgressCallbackInfoInternal, FileTransferProgressCallbackInfo>(data, out var callback, out var _))
			{
				Helper.TryMarshalGet<FileTransferProgressCallbackInfoInternal, FileTransferProgressCallbackInfo>(data, out var target);
				callback(target);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryFileCompleteCallbackInternal))]
		internal static void OnQueryFileCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryFileCompleteCallback, QueryFileCallbackInfoInternal, QueryFileCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryFileListCompleteCallbackInternal))]
		internal static void OnQueryFileListCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryFileListCompleteCallback, QueryFileListCallbackInfoInternal, QueryFileListCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnReadFileCompleteCallbackInternal))]
		internal static void OnReadFileCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnReadFileCompleteCallback, ReadFileCallbackInfoInternal, ReadFileCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnReadFileDataCallbackInternal))]
		internal static ReadResult OnReadFileDataCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetStructCallback<OnReadFileDataCallback, ReadFileDataCallbackInfoInternal, ReadFileDataCallbackInfo>(data, out var callback, out var _))
			{
				Helper.TryMarshalGet<ReadFileDataCallbackInfoInternal, ReadFileDataCallbackInfo>(data, out var target);
				return callback(target);
			}
			return Helper.GetDefault<ReadResult>();
		}
	}
}
