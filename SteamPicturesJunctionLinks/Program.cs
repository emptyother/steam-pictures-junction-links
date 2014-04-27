using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace SteamPicturesJunctionLinks
{
	internal class SteamData
	{
		// ReSharper disable InconsistentNaming
		public string type { get; set; }
		public string name { get; set; }
		public int steam_appid { get; set; }
		// ReSharper restore InconsistentNaming
	}
	internal class JsonResponseData
	{
		// ReSharper disable InconsistentNaming
		public bool success { get; set; }
		public SteamData data { get; set; }
		// ReSharper restore InconsistentNaming
	}
	internal class SteamGame
	{
		public DirectoryInfo SteamFolder;
		public int SteamId;
		public SteamData SteamData;

		public string SafeName
		{
			get { return String.Join("", SteamData.name.Split(Path.GetInvalidFileNameChars())); }
		}
		public DirectoryInfo PictureFolder
		{
			get { return new DirectoryInfo(Properties.Settings.Default.PicturesFolder + "\\" + SafeName); }
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
			SteamId = int.Parse(SteamFolder.FullName.Split('\\').Last());
			SteamData = GetSteamData();
		}
		private SteamData GetSteamData()
		{
			JsonResponseData result;
			Program.WriteLine("Getting data from steam on game {0}", SteamId);
			WebRequest req = WebRequest.Create(String.Format(Properties.Settings.Default.SteamDatabaseURL, SteamId));
			req.ContentType = "application/json; charset=utf-8";
			using (var response = (HttpWebResponse)req.GetResponse())
			// ReSharper disable once AssignNullToNotNullAttribute
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var js = new JavaScriptSerializer();
				var obj = js.Deserialize<Dictionary<string, JsonResponseData>>(reader.ReadToEnd());
				result = obj[SteamId.ToString(CultureInfo.InvariantCulture)];
			}
			return result.data;
		}
	}

	class Program
	{
		static readonly List<SteamGame> FoldersToProcess = new List<SteamGame>();
		public static bool Silent = false;

		static void Main(string[] args)
		{
			Silent = args.Contains("/s");
			if (!Silent)
			{
				WriteLine("SteamPicturesJunctionLinksTool v1.0");
				WriteLine("-------------------------------------------");
				WriteLine("This will move all subfolders from within \n\"{0}\"\nto folders with the Steam Game's name within folder \n\"{1}\",\nand will then create junction links between them. If these folders are wrong, change the configuration file.\nIf you dont want to show this message, use the parameter /s.\nAre you sure you want to continue? (y/n)", Properties.Settings.Default.SteamFolder + Properties.Settings.Default.SteamPicturesSubFolder, Properties.Settings.Default.PicturesFolder);
				var keyPress = Console.ReadKey(true);
				if (keyPress.Key != ConsoleKey.Y)
				{
					WriteLine("Action canceled.");
					return;
				}
			}
			var steamScreenshotDirectory = Directory.GetDirectories(new DirectoryInfo(Properties.Settings.Default.SteamFolder + Properties.Settings.Default.SteamPicturesSubFolder).FullName);
			foreach (var subdir in steamScreenshotDirectory.Where(subdir => !JunctionPoint.Exists(subdir + Properties.Settings.Default.ScreenshotSubfolder)))
			{
				FoldersToProcess.Add(new SteamGame(subdir));
			}
			foreach (var game in FoldersToProcess)
			{
				if (!game.PictureFolder.Exists)
				{
					WriteLine("Creating folder {0}", game.SafeName);
					Directory.CreateDirectory(game.PictureFolder.FullName);
				}
				MoveContentOfFolder(game.SteamScreenshotSubFolder, game.PictureFolder);
				Directory.Delete(game.SteamScreenshotSubFolder.FullName);
				WriteLine("Creates junction point from:\n {0} \nto {1}", game.SteamScreenshotSubFolder.FullName, game.PictureFolder.FullName);
				JunctionPoint.Create(game.SteamScreenshotSubFolder.FullName, game.PictureFolder.FullName, true);
			}
			WriteLine("End of program. Press any key to end.");
			if(!Silent) Console.ReadKey();
		}

		public static void WriteLine(string format, params object[] arg)
		{
			if (Silent) return;
			Console.WriteLine(format, arg);
		}

		private static void MoveContentOfFolder(DirectoryInfo source, DirectoryInfo destination)
		{
			var files = source.GetFiles("*", SearchOption.TopDirectoryOnly);
			var directories = source.GetDirectories();
			foreach (var file in files.Where(file => !new FileInfo(destination + "\\" + file.Name).Exists))
			{
				file.MoveTo(destination + "\\" + file.Name);
			}
			foreach (var dir in directories)
			{
				dir.MoveTo(dir.FullName.Replace(source.FullName, destination.FullName));
			}			
		}
	}
}
