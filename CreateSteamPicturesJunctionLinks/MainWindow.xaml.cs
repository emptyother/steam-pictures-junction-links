using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using CreateSteamPicturesJunctionLinks.Classes;
using Microsoft.Win32;

namespace CreateSteamPicturesJunctionLinks
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
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

		private void inputSteamFolder_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
			//foreach (SteamGame item in DatagridFoldersToProcess.ItemsSource)
			//{
			//	var row = DatagridFoldersToProcess.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
			//	if (row != null && String.IsNullOrEmpty(item.GameName))
			//	{
			//		row.Background = Brushes.LightPink;
			//	}
			//}
		}

		private void InputSteamFolder_Loaded(object sender, RoutedEventArgs e)
		{
			var regKey = Registry.CurrentUser;
			regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

			if (regKey != null)
			{
				var installpath = regKey.GetValue("SteamPath").ToString().Replace('/','\\');
				InputSteamFolder.Text = installpath;
			}
		}

		private void InputPicturesFolder_Loaded(object sender, RoutedEventArgs e)
		{
			InputPicturesFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
		}
	}
}
