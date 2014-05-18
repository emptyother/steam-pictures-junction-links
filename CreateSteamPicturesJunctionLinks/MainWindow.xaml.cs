using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
		private readonly List<SteamGame> _gamelist = new List<SteamGame>();
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
			_gamelist.Clear();
			DropdownUserId.Items.Clear();
			try
			{
				var steamUserdataFolder = new DirectoryInfo(InputSteamFolder.Text).FullName + "\\userdata";
				var steamUserFolders = Directory.GetDirectories(steamUserdataFolder);
				foreach (var subdir in steamUserFolders)
				{
					uint id;
					uint.TryParse(new DirectoryInfo(subdir).Name, out id);
					if (id > 0)
					{
						DropdownUserId.Items.Add(id.ToString(CultureInfo.InvariantCulture));
					}
				}
				if (DropdownUserId.Items.Count > 0)
				{
					DropdownUserId.SelectedIndex = 0;
				}
				else
				{
					MessageBox.Show(this, "Could not find Steam user folders.", "Folders not found", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch(DirectoryNotFoundException)
			{
				MessageBox.Show(this, "Could not find Steam userdata folder.", "Folder not found", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async void dropdownUserId_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DropdownUserId.SelectedIndex == -1) return;
			try
			{
				var folder = new DirectoryInfo(InputSteamFolder.Text).FullName + "\\userdata\\" + DropdownUserId.SelectedValue + "\\760\\remote";
				var steamScreenshotDirectory = Directory.GetDirectories(folder);
				ProgressReadingNames.Maximum = steamScreenshotDirectory.Count(subdir => !JunctionPoint.Exists(subdir + "\\screenshots"));
				ProgressReadingNames.Value = 0;
				ProgressReadingNames.Visibility = Visibility.Visible;
				LabelProgressBar.Visibility = Visibility.Visible;
				await AddGames(steamScreenshotDirectory);
			}
			catch (DirectoryNotFoundException)
			{
				MessageBox.Show(this, "Could not find Steam screenshot folder.", "Folder not found", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			ProgressReadingNames.Visibility = Visibility.Hidden;
			LabelProgressBar.Visibility = Visibility.Hidden;
		}

		private bool _isProcessing;
		private async Task AddGames(IEnumerable<string> steamScreenshotDirectory)
		{
			_isProcessing = true;
			foreach (var subdir in steamScreenshotDirectory.Where(subdir => !JunctionPoint.Exists(subdir + "\\screenshots")))
			{
				var game = new SteamGame();
				await game.LoadAsync(subdir);
				_gamelist.Add(game);

				ProgressReadingNames.Value += 1;
			}
			DatagridFoldersToProcess.ItemsSource = _gamelist;
			DatagridFoldersToProcess.Items.Refresh();
			_isProcessing = false;
		}

		private void InputSteamFolder_Loaded(object sender, RoutedEventArgs e)
		{
			var regKey = Registry.CurrentUser;
			regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

			if (regKey == null) return;
			var installpath = regKey.GetValue("SteamPath").ToString().Replace('/', '\\');
			InputSteamFolder.Text = installpath;
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
			BtnStartProcess.IsEnabled = (!_gamelist.Any(g => (String.IsNullOrEmpty(g.GameName)) && g.GameName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) && _gamelist.Any() && !_isProcessing);
			LabelErrorText.Visibility = _gamelist.Count(g => (String.IsNullOrEmpty(g.GameName))) == 0 || _isProcessing ? Visibility.Hidden : Visibility.Visible;
		}

		private void BtnStartProcess_Click(object sender, RoutedEventArgs e)
		{
			var picturesDirectory = new DirectoryInfo(InputPicturesFolder.Text);
			if (!picturesDirectory.Exists)
			{
				MessageBox.Show(this, "Could not find output folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			foreach (var game in _gamelist)
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
			Close();
		}

		private static void MoveContentOfFolder(DirectoryInfo source, FileSystemInfo destination)
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
