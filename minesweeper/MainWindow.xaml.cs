using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace minesweeper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : Window
	{
		public class SizeOption
		{
			public int size { get; set; }
			public string SizeDisplay { get; set; }

			public SizeOption(int size, string display)
			{
				this.size = size; this.SizeDisplay = display;
			}
		}

		public BindingList<SizeOption> GameSizeOptions = new BindingList<SizeOption>(){
			new SizeOption(10, "10 *10"),
			new SizeOption(15, "15 *15"),
			new SizeOption(20, "20 *20")
		};

		// game data.
		public MineSpot[,] minefield;
		public int bombsLeft = 0;
		public int numFlags = 0;

		public MainWindow()
		{
			InitializeComponent();
			GameSize.ItemsSource = GameSizeOptions;
			bombsLeftDisplay.DataContext = bombsLeft;
		}

		private void GameStart(object sender, RoutedEventArgs e)
		{
			status.Text = "Game Started";
			// Generate a game of size * size cells.
			int selectedsize = ((SizeOption)(GameSize.SelectedItem)).size;
			CreateGameBoard(selectedsize);

		}

		private void GameSizeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			int selectedsize = ((SizeOption)(GameSize.SelectedItem)).size;
		}

		private void CreateGameBoard(int selectedsize)
		{
			// create a game board of size x size rectangles, each bound to a different node.
			// set x random nodes as mines.
			ClearGameBoard();
			// bool boardfinished = false;
			int nextX = 0, nextY = 0;
			// int rowCounter = 0;
			double width = GameBoard.Width / selectedsize;
			double height = GameBoard.Height / selectedsize;
			bombsLeft = selectedsize * selectedsize / 5;
			bombsLeftDisplay.Text = bombsLeft.ToString();
			minefield = populateGameData(selectedsize);

			// define rows and columns first, then add a rectangle to spot.
			for (int s = 0; s < selectedsize; s++)
			{
				ColumnDefinition NewColumn = new ColumnDefinition();
				RowDefinition NewRow = new RowDefinition();
				GameBoard.ColumnDefinitions.Add(NewColumn);
				GameBoard.RowDefinitions.Add(NewRow);
			}

			// initialize a minespot for each rectangle in the gameboard.

			while (nextX < selectedsize || nextY < selectedsize)
			{
				nextY = 0;
				while (nextY < selectedsize)
				{
					Button spot = new Button() {
						VerticalAlignment= VerticalAlignment.Stretch,
						HorizontalAlignment = HorizontalAlignment.Stretch,
						Background = Brushes.White
					};
					spot.Click += ClickSpot;
					spot.MouseRightButtonDown += MarkFlag;
					TextBlock text = new TextBlock()
					{
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center
					};
					
					Grid.SetRow(text, nextX);
					Grid.SetColumn(text, nextY);
					Grid.SetRow(spot, nextX);
					Grid.SetColumn(spot, nextY);
					GameBoard.Children.Add(spot);
					//GameBoard.Children.Add(rect);
					GameBoard.Children.Add(text);
					nextY++;
				}
				nextX++;
			}
		}

		// function to create the data for the game, randomize the location of mines.
		private MineSpot[,] populateGameData(int selectedsize)
		{
			MineSpot[,] field = new MineSpot[selectedsize, selectedsize];
			Random rnd = new Random();
			for (int x = 0; x < selectedsize; x++)
			{
				for (int y = 0; y < selectedsize; y++)
				{
					field[x, y] = new MineSpot(false, false);					
				}
			}
			int i = 0;

			// initialize the spots for mines.
			while (i<=bombsLeft)
			{
				MineSpot mine = field[rnd.Next(selectedsize), rnd.Next(selectedsize)];
				if (!mine.getHasMine())
				{
					mine.setMine(true);
					i += 1;
				}
				
			}
			return field;
		} 

		private void ClickSpot(object sender, RoutedEventArgs e)
		{
			Button b = (Button)sender;
			b.IsEnabled = false;
			int x = Grid.GetRow(b); 
			int y = Grid.GetColumn(b);

			MineSpot spot = minefield[x, y];
			if (spot.getHasMine())
			{
				// game over, click all button and reveal the mines
				IEnumerable<Button> clickableSpots = GameBoard.Children.OfType<Button>();
				foreach (Button clickableSpot in clickableSpots)
				{
					clickableSpot.IsEnabled = false;
				}

				IEnumerable<TextBlock> minespots = GameBoard.Children
							 .OfType<TextBlock>();
				foreach (TextBlock minespot in minespots)
				{
					int row = Grid.GetRow(minespot);
					int col = Grid.GetColumn(minespot);
					if (minefield[row, col].getHasMine())
					{
						minespot.Text = "M";
					}
				}

				status.Text = "You Lose!";
				return;
			}

			spot.setClicked(true);
			int total = CheckAdjacent(x, y);

			if (total > 0)
			{
				IEnumerable<TextBlock> celltxt = GameBoard.Children
							 .OfType<TextBlock>()
							 .Where(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);

				foreach (TextBlock txt in celltxt)
				{
					txt.Text = total.ToString();
				}
			}
			// if no adjacent cells contain mines, reveal all nearby cells.
			else
			{
				DepthClick(x-1, y);
				DepthClick(x, y-1);
				DepthClick(x+1, y);
				DepthClick(x, y+1);
			}

		}

		// flags a cell in the gameboard. if it's a mine, decrease bomb count by 1.
		// if all mines are found, end the game.
		// if the cell has a flag on it, remove the flag.
		public void MarkFlag(object sender, RoutedEventArgs e)
		{
			Button b = (Button)sender;
			int x = Grid.GetRow(b);
			int y = Grid.GetColumn(b);

			IEnumerable<TextBlock> celltxt = GameBoard.Children
			 .OfType<TextBlock>()
			 .Where(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);

			MineSpot spot = minefield[x, y];
			if (!spot.HasFlag)
			{
				if (spot.getHasMine())
				{
					bombsLeft -= 1;
					bombsLeftDisplay.Text = bombsLeft.ToString();
					if (bombsLeft == 0) 
					{
						status.Text = "You Win!";
						IEnumerable<Button> clickableSpots = GameBoard.Children.OfType<Button>();
						foreach (Button clickableSpot in clickableSpots)
						{
							clickableSpot.IsEnabled = false;
						}
					};
				}
				spot.setFlag(true);
				foreach (TextBlock txt in celltxt)
				{
					txt.Text = "F";
				}
			}
			else
			{
				if (spot.getHasMine())
				{
					bombsLeft += 1;
					bombsLeftDisplay.Text = bombsLeft.ToString();
				}
				spot.setFlag(false);
				foreach (TextBlock txt in celltxt)
				{
					txt.Text = "";
				}
			}
		}

		// reveal all adjacent cells to current position. If any of them is zero, repeat the process.
		private void DepthClick(int x, int y)
		{
			int maxX = minefield.GetLength(0);
			int maxY = minefield.GetLength(1);
			// make sure the position is valid
			if (x < 0 || x >= maxX || y < 0 || y >= maxY)
			{
				return;
			}
			else if (minefield[x,y].HasClicked)
			{
				return;
			}
			else
			{
				minefield[x, y].setClicked(true);
				// click the button, then find the total. If total is zero, return the process.
				IEnumerable<Button> clickableSpots = GameBoard.Children
															  .OfType<Button>()
															  .Where(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
				foreach (Button button in clickableSpots)
				{
					button.IsEnabled = false;
				}
				int total = CheckAdjacent(x, y);
				if (total > 0)
				{
					IEnumerable<TextBlock> celltxt = GameBoard.Children
															  .OfType<TextBlock>()
															  .Where(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);

					foreach (TextBlock txt in celltxt)
					{
						txt.Text = total.ToString();
					}
					return;
				}
				else
				{
					DepthClick(x - 1, y - 1);
					DepthClick(x - 1, y);
					DepthClick(x, y - 1);
					DepthClick(x + 1, y - 1);
					DepthClick(x + 1, y);
					DepthClick(x - 1, y + 1);
					DepthClick(x, y + 1);
					DepthClick(x + 1, y + 1);
				}
			}
		}

		// get all adjacent cells to the current position. if there's no cell in that position, it's replaced with null
		private MineSpot[] getAdjacentCells(int x, int y)
		{
			int maxX = minefield.GetLength(0);
			int maxY = minefield.GetLength(1);
			MineSpot[] adjacentCells = new MineSpot[8];

			// find the spots we have to check
			// cell order [left, up, right, down, upLeft, upRight, downLeft, downRight]
			adjacentCells[0] = x > 0 ? minefield[x - 1, y] : null;
			adjacentCells[1] = y > 0 ? minefield[x, y - 1] : null;
			adjacentCells[2] = x + 1 < maxX ? minefield[x + 1, y] : null;
			adjacentCells[3] = y + 1 < maxY ? minefield[x, y + 1] : null;
			adjacentCells[4] = x > 0 && y > 0 ? minefield[x - 1, y - 1] : null;
			adjacentCells[5] = x + 1 < maxX && y > 0 ? minefield[x + 1, y - 1] : null;
			adjacentCells[6] = x > 0 && y + 1 < maxY ? minefield[x - 1, y + 1] : null;
			adjacentCells[7] = x + 1 < maxX && y + 1 < maxY ? minefield[x + 1, y + 1] : null;
			
			return adjacentCells;
		}

		// checks all adjacent spots to see if any of them returns a mine, and return the total.
		private int CheckAdjacent(int x, int y)
		{
			int total = 0;

			MineSpot[] adjacentCells = getAdjacentCells(x, y);
			for (int i=0; i<8; i++)
			{
				total += CheckSpot(adjacentCells[i]);
			}
			return total;
		}

		// returns 1 if spot has a mine, 0 otherwise
		private int CheckSpot(MineSpot spot)
		{
			if (spot != null)
			{
				if (spot.getHasMine())
				{
					return 1;
				}
				return 0;
			}
			return 0;
		}

		private void ClearGameBoard()
		{
			GameBoard.Children.Clear();
			GameBoard.ColumnDefinitions.Clear();
			GameBoard.RowDefinitions.Clear();
			status.Text = "";
			bombsLeftDisplay.Text = "";
		}
	}
}
