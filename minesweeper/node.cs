using System;

namespace minesweeper {
	public class MineSpot
	{
		public Boolean HasClicked;
		public Boolean HasMine;
		public Boolean HasFlag;

		public MineSpot(Boolean HasClicked, Boolean HasMine)
		{
			this.HasClicked = HasClicked;
			this.HasMine = HasMine;
			this.HasFlag = false;
		}

		public void setClicked(Boolean click)
		{
			this.HasClicked = click;
		}

		public void setFlag(Boolean flag)
		{
			this.HasFlag = flag;
		}

		public void setMine(Boolean mine)
		{
			this.HasMine = mine;
		}

		public Boolean getHasMine()
		{
			return this.HasMine;
		}
		public override string ToString()
		{
			return "HasClicked = " + this.HasClicked + "\n" + "HasMine = " + this.HasMine + "\n";
		}
	}
}
