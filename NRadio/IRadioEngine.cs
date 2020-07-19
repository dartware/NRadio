﻿using System;
using System.Threading.Tasks;

namespace Dartware.NRadio
{
	/// <summary>
	/// Defines the functionality for working with streaming audio, based on third-party URLs.
	/// </summary>
	public interface IRadioEngine
	{
		
		/// <summary>
		/// Gets or sets the volume level. Values in the range from 0 to 100 are allowed.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		Double Volume { get; set; }
		
		/// <summary>
		/// Sets the stream URL.
		/// </summary>
		/// <param name="url">Stream URL.</param>
		void SetURL(String url);

		/// <summary>
		/// Sets the stream URL.
		/// </summary>
		/// <param name="url">Stream URL.</param>
		Task SetURLAsync(String url);

		/// <summary>
		/// Returns current URL of the stream.
		/// </summary>
		/// <returns>URL of the stream.</returns>
		String GetURL();

		/// <summary>
		/// Starts playing.
		/// </summary>
		void Play();

		/// <summary>
		/// Pause playing.
		/// </summary>
		void Pause();

	}
}