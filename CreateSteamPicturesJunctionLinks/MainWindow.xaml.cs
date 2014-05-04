using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CreateSteamPicturesJunctionLinks.Classes;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

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
			BtnPreview.IsEnabled = (DatagridFoldersToProcess.SelectedItem != null);
		}

		private void DatagridFoldersToProcess_LayoutUpdated(object sender, EventArgs e)
		{
			BtnStartProcess.IsEnabled = (Gamelist.Count(g => (String.IsNullOrEmpty(g.GameName)) && g.GameName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) == 0);
			if (Gamelist.Count(g => (String.IsNullOrEmpty(g.GameName))) == 0)
			{
				LabelErrorText.Visibility = Visibility.Hidden;
			}
			else
			{
				LabelErrorText.Visibility = Visibility.Visible;
			}
		}

		private void BtnStartProcess_Click(object sender, RoutedEventArgs e)
		{
			var picturesDirectory = new DirectoryInfo(InputPicturesFolder.Text);
			if (!picturesDirectory.Exists)
			{
				MessageBox.Show(this, "Could not find output folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			foreach (var game in Gamelist)
			{
				var pictureFolder = new DirectoryInfo(picturesDirectory.FullName + "\\" + game.GameName);
				if (!pictureFolder.Exists)
				{					
					Directory.CreateDirectory(pictureFolder.FullName);
				}
				MoveContentOfFolder(game.SteamScreenshotSubFolder, pictureFolder);
				Directory.Delete(game.SteamScreenshotSubFolder.FullName);				
				JunctionPoint.Create(game.SteamScreenshotSubFolder.FullName, pictureFolder.FullName, true);
			}
			MessageBox.Show(this, "Done.", "Created junction links", MessageBoxButton.OK, MessageBoxImage.Information);
			this.Close();
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

		private void BtnPreview_Click(object sender, RoutedEventArgs e)
		{
			var selectedSteamGame = (SteamGame)DatagridFoldersToProcess.SelectedItem;
			if (selectedSteamGame == null) return;

			var inputDialog = new PreviewImage(selectedSteamGame.SteamScreenshotSubFolder) { Owner = this };
			inputDialog.ShowDialog();
		}
	}
}
