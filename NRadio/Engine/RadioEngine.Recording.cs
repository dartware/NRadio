﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dartware.NRadio.Meta;

namespace Dartware.NRadio
{
	internal sealed partial class RadioEngine
	{

		/// <summary>
		/// The recording status.
		/// </summary>
		private RecordingStatus recordingStatus;

		/// <summary>
		/// The recording status.
		/// </summary>
		public RecordingStatus RecordingStatus
		{
			get => recordingStatus;
			private set
			{
				if (recordingStatus != value)
				{

					recordingStatus = value;

					RecordingStatusChanged?.Invoke(value);

				}
			}
		}

		/// <summary>
		/// Cancellation token source for recording.
		/// </summary>
		private CancellationTokenSource recordingCancellationTokenSource;

		/// <summary>
		/// <see langword="true"/> if necessary splitting by track while recording, otherwise <see langword="false"/>.
		/// </summary>
		private Boolean splitByTrackWhileRecording;

		/// <summary>
		/// Gets or sets the recording path.
		/// </summary>
		public String RecordingPath { get; set; }

		/// <summary>
		/// Occurs when recording status changed.
		/// </summary>
		public event Action<RecordingStatus> RecordingStatusChanged;

		/// <summary>
		/// <see langword="true"/> if necessary splitting by track while recording, otherwise <see langword="false"/>.
		/// </summary>
		public Boolean SplitByTrackWhileRecording
		{
			get => splitByTrackWhileRecording;
			set
			{

				if (value == splitByTrackWhileRecording)
				{
					return;
				}
				
				splitByTrackWhileRecording = value;

				if (value)
				{

					MetadataChanged += OnMetadataChanged;

					RestartRecording();

				}
				else
				{
					
					MetadataChanged -= OnMetadataChanged;

					RestartRecording();

				}

			}
		}

		/// <summary>
		/// Start recording.
		/// </summary>
		public void StartRecording()
		{

			if (RecordingStatus == RecordingStatus.Recording)
			{
				return;
			}

			if (String.IsNullOrEmpty(URL))
			{
				return;
			}

			RecordingStatus = RecordingStatus.Recording;

			IMetadata metadata = Metadata;
			String url = URL;
			String recordingPath = RecordingPath;

			if (String.IsNullOrEmpty(recordingPath))
			{
				recordingPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
			}

			if (recordingCancellationTokenSource == null)
			{
				recordingCancellationTokenSource = new CancellationTokenSource();
			}

			CancellationToken cancellationToken = recordingCancellationTokenSource.Token;

			Task.Run(() =>
			{

				if (!Directory.Exists(recordingPath))
				{
					Directory.CreateDirectory(recordingPath);
				}

				String fileExtension = metadata.Format.ToExtension();
				String fileName = SplitByTrackWhileRecording ? metadata.SongName : DateTime.Now.ToLongTimeString().Replace(':', '-');
				String fullFileName = $"{recordingPath}{Path.DirectorySeparatorChar}{fileName}.{fileExtension}";

				if (File.Exists(fullFileName))
				{
					for (Int32 index = 1; ; index++)
					{

						String newFullFileName = $"{recordingPath}{Path.DirectorySeparatorChar}{fileName} ({index}).{fileExtension}";

						if (!File.Exists(newFullFileName))
						{

							fullFileName = newFullFileName;

							break;

						}

					}
				}

				using FileStream fileStream = new FileStream(fullFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				using WebResponse response = WebRequest.Create(url).GetResponse();
				using Stream stream = response.GetResponseStream();

				Byte[] buffer = new Byte[512];
				Int32 read;

				while ((!cancellationToken.IsCancellationRequested) && ((read = stream.Read(buffer, 0, buffer.Length)) > 0))
				{

					Int64 position = fileStream.Position;

					fileStream.Position = fileStream.Length;

					fileStream.Write(buffer, 0, read);

					fileStream.Position = position;

				}

				fileStream.Flush();

			});

		}

		/// <summary>
		/// Stop recording.
		/// </summary>
		public void StopRecording()
		{

			RecordingStatus = RecordingStatus.Stop;

			recordingCancellationTokenSource?.Cancel();

			recordingCancellationTokenSource = null;

		}

		/// <summary>
		/// Restarting recording if started.
		/// </summary>
		private void RestartRecording()
		{
			if (RecordingStatus == RecordingStatus.Recording)
			{
				StopRecording();
				StartRecording();
			}
		}

		/// <summary>
		/// Metadata changed event handler.
		/// </summary>
		/// <param name="_">Metadata.</param>
		private void OnMetadataChanged(IMetadata _)
		{
			RestartRecording();
		}

	}
}