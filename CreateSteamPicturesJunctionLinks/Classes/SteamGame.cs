using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace CreateSteamPicturesJunctionLinks.Classes
{
	public class SteamGame
	{
		public DirectoryInfo SteamFolder;
		public String SteamId;
		public SteamData SteamData;
		private string _gamename;
		public string GameName
		{
			get
			{
				if (_gamename == null && SteamData != null)
				{
					return String.Join("", SteamData.name.Split(Path.GetInvalidFileNameChars()));
				}
				return _gamename ?? "";
			}
			set { _gamename = value; }
		}		
		public DirectoryInfo SteamScreenshotSubFolder
		{
			get
			{
				return new DirectoryInfo(SteamFolder.FullName + Properties.Settings.Default.ScreenshotSubfolder);
			}
		}
		public SteamGame(string steamFolder)
		{
			SteamFolder = new DirectoryInfo(steamFolder);
			SteamId = SteamFolder.FullName.Split('\\').Last();
			var steamdata = GetSteamData();
			if (steamdata.success)
			{
				SteamData = steamdata.data;
			}
		}
		private JsonResponseData GetSteamData()
		{
			JsonResponseData result;			
			var req = WebRequest.Create(String.Format(Properties.Settings.Default.SteamDatabaseURL, SteamId));
			req.ContentType = "application/json; charset=utf-8";
			using (var response = (HttpWebResponse)req.GetResponse())
			// ReSharper disable once AssignNullToNotNullAttribute
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var js = new JavaScriptSerializer();
				var obj = js.Deserialize<Dictionary<string, JsonResponseData>>(reader.ReadToEnd());
				result = obj[SteamId];
			}
			return result;
		}
	}
}
