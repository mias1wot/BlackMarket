using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace BlackMarket_API.Data.Services.Repositories
{
	public static class AzureStorage
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


		//Gets product photo from Azure Block Blob Storage
		public static List<byte[]> GetPhotosFromAzureStorage(string containerName, List<string> photoNameList)
		{
			var photos = new List<byte[]>();

			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			BlobClient blobClient;
			foreach (string photoName in photoNameList)
			{
				blobClient = containerClient.GetBlobClient(photoName);
				if (photoName != null && blobClient.Exists())
				{
					Stream fileStream;
					try
					{
						fileStream = blobClient.Download().Value.Content;
					}
					catch(Exception e)
					{
						_logger.Error(e, "Failed to DOWNLOAD a photo {photoName} from Azure Blob Storage", photoName);
						return null;
					}
					

					//Reads Stream as byte[]
					byte[] photo;
					using (var memoryStream = new MemoryStream())
					{
						fileStream.CopyTo(memoryStream);
						photo = memoryStream.ToArray();
					}

					photos.Add(photo);
				}
				else
				{
					photos.Add(null);
				}
			}

			return photos;
		}

		public static bool UploadPhotoToAzureStorage(string containerName, string photoName, Stream newPhoto, bool replacePhotoIfExists)
		{
			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			BlobClient blobClient = containerClient.GetBlobClient(photoName);

			try
			{
				blobClient.Upload(newPhoto, replacePhotoIfExists);
			}
			catch(Exception e)
			{
				_logger.Error(e, "Failed to UPLOAD a photo {photoName} to Azure Blob Storage (container = {containerName})", photoName, containerName);
				return false;
			}

			return true;
		}

		//returns true if the photo existed and was deletet; false if photo doesn't exist
		public static bool DeletePhotoInAzureStorage(string containerName, string photoName)
		{
			BlobServiceClient blobServiceClient = new BlobServiceClient(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			BlobClient blobClient = containerClient.GetBlobClient(photoName);
			try
			{
				return blobClient.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
			}
			catch(Exception e)
			{
				_logger.Error(e, "Failed to DELETE a photo {photoName} from Azure Blob Storage (container = {containerName})", photoName, containerName);
			}

			return false;
		}
	}
}