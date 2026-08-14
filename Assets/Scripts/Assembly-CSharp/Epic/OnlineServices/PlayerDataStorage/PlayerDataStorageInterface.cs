using System;

namespace Epic.OnlineServices.PlayerDataStorage
{
	public sealed class PlayerDataStorageInterface : Handle
	{
		public const int CopyfilemetadataatindexoptionsApiLatest = 1;

		public const int CopyfilemetadatabyfilenameoptionsApiLatest = 1;

		public const int DeletecacheoptionsApiLatest = 1;

		public const int DeletefileoptionsApiLatest = 1;

		public const int DuplicatefileoptionsApiLatest = 1;

		public const int FileMaxSizeBytes = 67108864;

		public const int FilemetadataApiLatest = 3;

		public const int FilenameMaxLengthBytes = 64;

		public const int GetfilemetadatacountoptionsApiLatest = 1;

		public const int QueryfilelistoptionsApiLatest = 1;

		public const int QueryfileoptionsApiLatest = 1;

		public const int ReadfileoptionsApiLatest = 1;

		public const int WritefileoptionsApiLatest = 1;

		public PlayerDataStorageInterface()
		{
		}

		public PlayerDataStorageInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyFileMetadataAtIndex(CopyFileMetadataAtIndexOptions copyFileMetadataOptions, out FileMetadata outMetadata)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyFileMetadataAtIndexOptionsInternal, CopyFileMetadataAtIndexOptions>(ref target, copyFileMetadataOptions);
			IntPtr outMetadata2 = IntPtr.Zero;
			Result result = Bindings.EOS_PlayerDataStorage_CopyFileMetadataAtIndex(base.InnerHandle, target, ref outMetadata2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
			{
				Bindings.EOS_PlayerDataStorage_FileMetadata_Release(outMetadata2);
			}
			return result;
		}

		public Result CopyFileMetadataByFilename(CopyFileMetadataByFilenameOptions copyFileMetadataOptions, out FileMetadata outMetadata)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyFileMetadataByFilenameOptionsInternal, CopyFileMetadataByFilenameOptions>(ref target, copyFileMetadataOptions);
			IntPtr outMetadata2 = IntPtr.Zero;
			Result result = Bindings.EOS_PlayerDataStorage_CopyFileMetadataByFilename(base.InnerHandle, target, ref outMetadata2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<FileMetadataInternal, FileMetadata>(outMetadata2, out outMetadata))
			{
				Bindings.EOS_PlayerDataStorage_FileMetadata_Release(outMetadata2);
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
			Result result = Bindings.EOS_PlayerDataStorage_DeleteCache(base.InnerHandle, target, clientDataAddress, onDeleteCacheCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void DeleteFile(DeleteFileOptions deleteOptions, object clientData, OnDeleteFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DeleteFileOptionsInternal, DeleteFileOptions>(ref target, deleteOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDeleteFileCompleteCallbackInternal onDeleteFileCompleteCallbackInternal = OnDeleteFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onDeleteFileCompleteCallbackInternal);
			Bindings.EOS_PlayerDataStorage_DeleteFile(base.InnerHandle, target, clientDataAddress, onDeleteFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void DuplicateFile(DuplicateFileOptions duplicateOptions, object clientData, OnDuplicateFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<DuplicateFileOptionsInternal, DuplicateFileOptions>(ref target, duplicateOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnDuplicateFileCompleteCallbackInternal onDuplicateFileCompleteCallbackInternal = OnDuplicateFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onDuplicateFileCompleteCallbackInternal);
			Bindings.EOS_PlayerDataStorage_DuplicateFile(base.InnerHandle, target, clientDataAddress, onDuplicateFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result GetFileMetadataCount(GetFileMetadataCountOptions getFileMetadataCountOptions, out int outFileMetadataCount)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetFileMetadataCountOptionsInternal, GetFileMetadataCountOptions>(ref target, getFileMetadataCountOptions);
			outFileMetadataCount = Helper.GetDefault<int>();
			Result result = Bindings.EOS_PlayerDataStorage_GetFileMetadataCount(base.InnerHandle, target, ref outFileMetadataCount);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryFile(QueryFileOptions queryFileOptions, object clientData, OnQueryFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryFileOptionsInternal, QueryFileOptions>(ref target, queryFileOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryFileCompleteCallbackInternal onQueryFileCompleteCallbackInternal = OnQueryFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onQueryFileCompleteCallbackInternal);
			Bindings.EOS_PlayerDataStorage_QueryFile(base.InnerHandle, target, clientDataAddress, onQueryFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void QueryFileList(QueryFileListOptions queryFileListOptions, object clientData, OnQueryFileListCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryFileListOptionsInternal, QueryFileListOptions>(ref target, queryFileListOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryFileListCompleteCallbackInternal onQueryFileListCompleteCallbackInternal = OnQueryFileListCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onQueryFileListCompleteCallbackInternal);
			Bindings.EOS_PlayerDataStorage_QueryFileList(base.InnerHandle, target, clientDataAddress, onQueryFileListCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public PlayerDataStorageFileTransferRequest ReadFile(ReadFileOptions readOptions, object clientData, OnReadFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReadFileOptionsInternal, ReadFileOptions>(ref target, readOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnReadFileCompleteCallbackInternal onReadFileCompleteCallbackInternal = OnReadFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onReadFileCompleteCallbackInternal, readOptions.ReadFileDataCallback, ReadFileOptionsInternal.ReadFileDataCallback, readOptions.FileTransferProgressCallback, ReadFileOptionsInternal.FileTransferProgressCallback);
			IntPtr source = Bindings.EOS_PlayerDataStorage_ReadFile(base.InnerHandle, target, clientDataAddress, onReadFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out PlayerDataStorageFileTransferRequest target2);
			return target2;
		}

		public PlayerDataStorageFileTransferRequest WriteFile(WriteFileOptions writeOptions, object clientData, OnWriteFileCompleteCallback completionCallback)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<WriteFileOptionsInternal, WriteFileOptions>(ref target, writeOptions);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnWriteFileCompleteCallbackInternal onWriteFileCompleteCallbackInternal = OnWriteFileCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionCallback, onWriteFileCompleteCallbackInternal, writeOptions.WriteFileDataCallback, WriteFileOptionsInternal.WriteFileDataCallback, writeOptions.FileTransferProgressCallback, WriteFileOptionsInternal.FileTransferProgressCallback);
			IntPtr source = Bindings.EOS_PlayerDataStorage_WriteFile(base.InnerHandle, target, clientDataAddress, onWriteFileCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out PlayerDataStorageFileTransferRequest target2);
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

		[MonoPInvokeCallback(typeof(OnDeleteFileCompleteCallbackInternal))]
		internal static void OnDeleteFileCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDeleteFileCompleteCallback, DeleteFileCallbackInfoInternal, DeleteFileCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnDuplicateFileCompleteCallbackInternal))]
		internal static void OnDuplicateFileCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnDuplicateFileCompleteCallback, DuplicateFileCallbackInfoInternal, DuplicateFileCallbackInfo>(data, out var callback, out var callbackInfo))
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

		[MonoPInvokeCallback(typeof(OnWriteFileCompleteCallbackInternal))]
		internal static void OnWriteFileCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnWriteFileCompleteCallback, WriteFileCallbackInfoInternal, WriteFileCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnWriteFileDataCallbackInternal))]
		internal static WriteResult OnWriteFileDataCallbackInternalImplementation(IntPtr data, IntPtr outDataBuffer, ref uint outDataWritten)
		{
			if (Helper.TryGetStructCallback<OnWriteFileDataCallback, WriteFileDataCallbackInfoInternal, WriteFileDataCallbackInfo>(data, out var callback, out var _))
			{
				Helper.TryMarshalGet<WriteFileDataCallbackInfoInternal, WriteFileDataCallbackInfo>(data, out var target);
				byte[] outDataBuffer2;
				WriteResult result = callback(target, out outDataBuffer2);
				Helper.TryMarshalGet(outDataBuffer2, out outDataWritten);
				Helper.TryMarshalCopy(outDataBuffer, outDataBuffer2);
				return result;
			}
			return Helper.GetDefault<WriteResult>();
		}
	}
}
