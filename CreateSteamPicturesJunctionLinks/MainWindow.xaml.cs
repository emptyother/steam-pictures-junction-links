using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CreateSteamPicturesJunctionLinks.Classes;
using Microsoft.Win32;

namespace CreateSteamPicturesJunctionLinks
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public List<SteamGame> Gamelist = new List<SteamGame>();
		public MainWindow()
		{
			InitializeComponent();
		}

		private void btnSteamFolder_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = InputSteamFolder.Text
			};
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				InputSteamFolder.Text = dialog.SelectedPath;
			}
		}

		private void btnPicturesFolder_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = InputPicturesFolder.Text
			};
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				InputPicturesFolder.Text = dialog.SelectedPath;
			}
		}

		private void inputSteamFolder_TextChanged(object sender, TextChangedEventArgs e)
		{
			Gamelist.Clear();
			var folder = new DirectoryInfo(InputSteamFolder.Text).FullName + Properties.Settings.Default.SteamPicturesSubFolder;
			var steamScreenshotDirectory = Directory.GetDirectories(folder);
			foreach (var subdir in steamScreenshotDirectory.Where(subdir => !JunctionPoint.Exists(subdir + Properties.Settings.Default.ScreenshotSubfolder)))
			{
				var game = new SteamGame(subdir);
				Gamelist.Add(game);
			}
			DatagridFoldersToProcess.ItemsSource = Gamelist;
		}

		private void InputSteamFolder_Loaded(object sender, RoutedEventArgs e)
		{
			var regKey = Registry.CurrentUser;
			regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

			if (regKey != null)
			{
				var installpath = regKey.GetValue("SteamPath").ToString().Replace('/', '\\');
				InputSteamFolder.Text = installpath;
			}
		}

		private void InputPicturesFolder_Loaded(object sender, RoutedEventArgs e)
		{
			InputPicturesFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
		}

		private void BtnRename_Click(object sender, RoutedEventArgs e)
		{
			var selectedSteamGame = (SteamGame)DatagridFoldersToProcess.SelectedItem;
			if (selectedSteamGame != null)
			{
				var inputDialog = new SetGameName(selectedSteamGame.GameName) { Owner = this };
				if (inputDialog.ShowDialog() == true)
				{
					selectedSteamGame.GameName = inputDialog.InputText;
				}
			}
			DatagridFoldersToProcess.Items.Refresh();
		}

		private void DatagridFoldersToProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BtnRename.IsEnabled = (DatagridFoldersToProcess.SelectedItem != null);
		}

		private void DatagridFoldersToProcess_LayoutUpdated(object sender, EventArgs e)
		{
			BtnStartProcess.IsEnabled = (Gamelist.Count(g => (String.IsNullOrEmpty(g.GameName)) && g.GameName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) == 0);
		}
	}
}
