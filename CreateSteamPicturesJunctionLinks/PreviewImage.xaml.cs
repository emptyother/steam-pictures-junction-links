using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CreateSteamPicturesJunctionLinks
{
	/// <summary>
	/// Interaction logic for PreviewImage.xaml
	/// </summary>
	public partial class PreviewImage
	{
		public PreviewImage(DirectoryInfo folder)
		{
			InitializeComponent();
			var files = folder.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
			if (!files.Any())
			{
				ImagePreview.Visibility = Visibility.Hidden;
				LabelNoImagesFound.Visibility = Visibility.Visible;
			}
			else
			{
				var ran = new Random();
				var number = ran.Next(files.Count());
				ImagePreview.Source = new BitmapImage(new Uri(files[number].FullName));
			}
		}

		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
