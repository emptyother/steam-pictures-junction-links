using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CreateSteamPicturesJunctionLinks.Classes
{
	public class SteamGame
	{
		private DirectoryInfo _steamFolder;
		private String _steamId;
		private SteamData _steamData;
		private string _gamename;
		public string GameName
		{
			get
			{
				if (_gamename == null && _steamData != null)
				{
					return String.Join("", _steamData.name.Split(Path.GetInvalidFileNameChars()));
				}
				return _gamename ?? "";
			}
			set { _gamename = value; }
		}		
		public DirectoryInfo SteamScreenshotSubFolder
		{
			get
			{
				return _steamFolder == null ? new DirectoryInfo("") : new DirectoryInfo(_steamFolder.FullName + "\\screenshots");
			}
		}
		public string NewFolder
		{
			get
			{
				return String.Format("<Your screenshot folder>\\{0}", GameName);
			}
		}
		public SteamGame()
		{
			_steamFolder = null;
			_steamId = "0";
			_steamData = null;
		}
		public async Task<bool> LoadAsync(string steamFolder)
		{
			_steamFolder = new DirectoryInfo(steamFolder);
			_steamId = _steamFolder.FullName.Split('\\').Last();
			var steamdata = await GetSteamData();
			if (steamdata.success)
			{
				_steamData = steamdata.data;
			}
			return true;
		}
		private async Task<JsonResponseData> GetSteamData()
		{
			JsonResponseData result;			
			var req = WebRequest.Create(String.Format(Properties.Settings.Default.SteamDatabaseURL, _steamId));
			req.ContentType = "application/json; charset=utf-8";
			using (var response = await req.GetResponseAsync())
			// ReSharper disable once AssignNullToNotNullAttribute
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var js = new JavaScriptSerializer();
				var obj = js.Deserialize<Dictionary<string, JsonResponseData>>(reader.ReadToEnd());				
				result = obj.First().Value;
			}
			return result;
		}
	}
}
