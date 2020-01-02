// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Leacme.Lib.LiteDbTraveler;
using LiteDB;

namespace Leacme.App.LiteDbTraveler {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();
		private Dictionary<string, List<Dictionary<string, string>>> currDb = new Dictionary<string, List<Dictionary<string, string>>>();

		public AppUI() {

			var rootGrid = new Grid();
			rootGrid.ColumnDefinitions = new ColumnDefinitions("200,*");
			rootGrid.RowDefinitions = new RowDefinitions("Auto,*");
			rootGrid.Margin = App.Margin;
			rootGrid.Height = 480;
			rootGrid.HorizontalAlignment = HorizontalAlignment.Center;

			var collectionsPanel = App.DataGrid;
			collectionsPanel.SetValue(DataGrid.WidthProperty, AvaloniaProperty.UnsetValue);
			collectionsPanel.SetValue(DataGrid.HeightProperty, AvaloniaProperty.UnsetValue);
			collectionsPanel[Grid.ColumnProperty] = 0;
			collectionsPanel[Grid.RowSpanProperty] = 2;
			collectionsPanel.AutoGeneratingColumn += (z, zz) => { zz.Column.Header = "Collections"; };

			var docDataPanel = App.DataGrid;
			docDataPanel.SetValue(DataGrid.WidthProperty, AvaloniaProperty.UnsetValue);
			docDataPanel.SetValue(DataGrid.HeightProperty, AvaloniaProperty.UnsetValue);
			docDataPanel[Grid.ColumnProperty] = 1;
			docDataPanel[Grid.RowProperty] = 1;

			collectionsPanel.SelectionChanged += (z, zz) => {
				var docs = new List<dynamic>();
				foreach (var doc in currDb[collectionsPanel.SelectedItem.ToString()]) {
					foreach (var docData in doc) {
						docs.Add(new {
							Key = docData.Key,
							Value = docData.Value,
						});
					}
				}
				docDataPanel.Items = docs;
			};

			var openDbPan = App.HorizontalFieldWithButton;
			openDbPan.holder[Grid.ColumnProperty] = 1;
			openDbPan.holder[Grid.RowProperty] = 0;
			openDbPan.label.Text = "Load LiteDB database to view:";
			openDbPan.field.Width = 350;
			openDbPan.field.IsReadOnly = true;
			openDbPan.button.Content = "Load Datafile...";
			openDbPan.button.Click += async (z, zz) => {
				openDbPan.field.Text = (await OpenFile()).First();
				openDbPan.field.Watermark = "";
				if (!string.IsNullOrWhiteSpace(openDbPan.field.Text)) {
					try {
						currDb = lib.GetDbValues(new LiteDatabase(openDbPan.field.Text));
					} catch (LiteException e) {
						openDbPan.field.Text = "";
						openDbPan.field.Watermark = e.Message;
					}
					collectionsPanel.Items = currDb.Keys;
					collectionsPanel.SelectedItem = collectionsPanel.Items.Cast<object>().First();
				}
			};

			rootGrid.Children.Add(collectionsPanel);
			rootGrid.Children.Add(openDbPan.holder);
			rootGrid.Children.Add(docDataPanel);
			rootPan.Children.Add(rootGrid);

		}

		private async Task<IEnumerable<string>> OpenFile() {
			var dialog = new OpenFileDialog() {
				Title = "Open LiteDB Database",
				InitialDirectory = Directory.GetCurrentDirectory(),
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (res?.Any() == true) ? res : Enumerable.Empty<string>();
		}
	}
}