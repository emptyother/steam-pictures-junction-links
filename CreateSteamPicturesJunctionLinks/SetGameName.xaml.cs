using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CreateSteamPicturesJunctionLinks
{
	/// <summary>
	/// Interaction logic for SetGameName.xaml
	/// </summary>
	public partial class SetGameName
	{
		public string InputText;

		public SetGameName(string initialInputText)
		{
			InitializeComponent();
			TxtInput.Text = initialInputText;
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e)
		{
			InputText = TxtInput.Text.Trim();
			DialogResult = true;
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			InputText = null;
			DialogResult = false;
		}

		private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			BtnOk.IsEnabled = (!String.IsNullOrWhiteSpace(TxtInput.Text) && TxtInput.Text.IndexOfAny(Path.GetInvalidFileNameChars()) < 0);
		}
	}
}
