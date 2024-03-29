﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccessCommunication;
using FoodPlanner.SQLObj;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for WeekPlanPage.xaml
	/// </summary>
	public partial class WeekPlanPage : Page
	{
		private int[] _currRecipes;

		private int[] _currRecipesDistinct;

		private static readonly Random Rng = new Random();

		//TODO: Add ability to manually edit week plan items
		public WeekPlanPage()
		{
			InitializeComponent();
		}

		private async void WeekPlanPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			if(IsEnabled)
				return;

			await Task.Delay(100);

			QueryResult currWeekPlan = await App.ExecuteQuery("select Wochentag, IDRezepte from Wochenplan");

			_currRecipes = currWeekPlan.ReturnedRows.Select(a => (int)a[1]).ToArray();
			_currRecipesDistinct = _currRecipes.Distinct().ToArray();

			string currRecipesQuery = $"select ID, Gerichtname from Rezepte where ID = {_currRecipesDistinct[0]}";
			for(int i = 1; i < _currRecipesDistinct.Length; i++)
			{
				currRecipesQuery += $" or ID = {_currRecipesDistinct[i]}";
			}
			QueryResult currRecipes = await App.ExecuteQuery(currRecipesQuery);
			
			var currWeekPlanObj = new WeekPlan
			{
				Monday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Montag"))?[1] ?? default(int))))?[1].ToString(),

				Tuesday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Dienstag"))?[1] ?? default(int))))?[1].ToString(),

				Wednesday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Mittwoch"))?[1] ?? default(int))))?[1].ToString(),

				Thursday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Donnerstag"))?[1] ?? default(int))))?[1].ToString(),

				Friday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Freitag"))?[1] ?? default(int))))?[1].ToString(),

				Saturday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Samstag"))?[1] ?? default(int))))?[1].ToString(),

				Sunday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Sonntag"))?[1] ?? default(int))))?[1].ToString()
			};

			DataContext = currWeekPlanObj;

			IsEnabled = true;
		}

		private async void GenerateShoppingList(object sender, RoutedEventArgs e)
		{
			string allIngredsQuery =
				$"select Rezepte_ID, Zutat, Menge from ZusammenfassungRezeptzutatenliste " +
				$"where Rezepte_ID = {_currRecipesDistinct[0]}";
			for(int i = 1; i < _currRecipesDistinct.Length; i++)
			{
				allIngredsQuery += $" or Rezepte_ID = {_currRecipesDistinct[i]}";
			}

			QueryResult allIngreds = await App.ExecuteQuery(allIngredsQuery);

			await using var stream = new FileStream("shoppinglist.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
			await using var writer = new StreamWriter(stream);

			await writer.WriteLineAsync(
				Languages.Resources.ShoppingListTitle.Replace("$1", _currRecipesDistinct.Length.ToString()));

			for(int i = 0; i < allIngreds.ReturnedRows.Count; i++)
			{
				await writer.WriteLineAsync($"☐ {allIngreds.ReturnedRows[i][2]} {allIngreds.ReturnedRows[i][1]} " +
				                            $"(x{_currRecipes.Count(a => a.Equals((int)allIngreds.ReturnedRows[i][0]))})");
			}

			MessageBox.Show(Languages.Resources.MsgWeekPlanGen.Replace("$1", stream.Name),
				Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private async void ShuffleWeekPlan(object sender, RoutedEventArgs e)
		{
			int[] allRecipes = (await App.ExecuteQuery("select ID from Rezepte"))
				.ReturnedRows.Select(a => (int)a[0]).ToArray();

			var shuffledItems = new int[7];
			for(int i = 0; i < shuffledItems.Length; i++)
			{
				shuffledItems[i] = allRecipes[Rng.Next(0, allRecipes.Length)];
			}

			var queries = new Task[7];
			queries[0] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[0]} where Wochentag = 'Montag'"));
			queries[1] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[1]} where Wochentag = 'Dienstag'"));
			queries[2] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[2]} where Wochentag = 'Mittwoch'"));
			queries[3] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[3]} where Wochentag = 'Donnerstag'"));
			queries[4] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[4]} where Wochentag = 'Freitag'"));
			queries[5] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[5]} where Wochentag = 'Samstag'"));
			queries[6] = Task.Factory.StartNew(async() => 
				await App.ExecuteQuery($"update Wochenplan set IDRezepte = {shuffledItems[6]} where Wochentag = 'Sonntag'"));

			Task.WaitAll(queries);

			NavigationService?.Refresh();
		}
	}
}
