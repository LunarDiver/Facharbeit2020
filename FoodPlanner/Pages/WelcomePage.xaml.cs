﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for WelcomePage.xaml
	/// </summary>
	public partial class WelcomePage : Page
	{
		/// <summary>
		/// Initializes the page.
		/// </summary>
		public WelcomePage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Opens the recipe creation window and switches to the recipe tab.
		/// </summary>
		private async void GoToCreateRecipe(object sender, RoutedEventArgs e)
		{
			//Switches the current tab
			MainWindow parentWin = App.CurrWindow;
			if(parentWin != null)
			{
				parentWin.MainTabs.SelectedIndex = 2;
			}

			//Opens the recipe creation window
			await SqlHelper.InsertNewRecipeInteractive(true);
			
			//Refreshes the recipe page
			App.CurrRecipePage?.NavigationService?.Refresh();
		}
	}
}
