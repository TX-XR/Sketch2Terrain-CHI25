namespace Mapbox.Editor
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.SceneManagement;
	using UnityEditor;
	using System.IO;
	using System.Collections;
	using Mapbox.Unity;
	using Mapbox.Tokens;
	using Mapbox.Json;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Utilities.DebugTools;
	using UnityEditor.Callbacks;
	using System;

	public class MapboxConfigurationWindow : MonoBehaviour
	{
		public static MapboxConfigurationWindow instance;
		static MapboxAccess _mapboxAccess;
		static string _accessToken = "pk.eyJ1IjoieGlhb3RpYW55aSIsImEiOiJjbG9weGR1NWUwZWUzMmtwZWlzNG1pZ2x0In0.j7PPGX7ChdStrKFBjZ4B4w";
		[Range(0, 1000)]
		static int _memoryCacheSize = 500;
		[Range(0, 3000)]
		static int _fileCacheSize = 25000;
		static int _webRequestTimeout = 30;
		static bool _autoRefreshCache = false;

        private void Awake()
        {
			SubmitConfiguration();
        }
        /// <summary>
        /// Mapbox access
        /// </summary>
        private static void SubmitConfiguration()
		{
			var mapboxConfiguration = new MapboxConfiguration
			{
				AccessToken = _accessToken,
				MemoryCacheSize = (uint)_memoryCacheSize,
				FileCacheSize = (uint)_fileCacheSize,
				AutoRefreshCache = _autoRefreshCache,
				DefaultTimeout = _webRequestTimeout
			};
			_mapboxAccess.SetConfiguration(mapboxConfiguration, false);
		}

	}
}
