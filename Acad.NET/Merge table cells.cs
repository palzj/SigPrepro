[CommandMethod("mcit")]
public void MergeCellsInTable()
{
	Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
	Database db = doc.Database;
	Editor ed = doc.Editor;

	int numRows = 7;
	int numCols = 4;
	double rowHeight = 15.0;
	double colWidth = 45.0;
	double verMarg = 2.5;
	double horMarg = 2.5;
	double txtHeight = 10.0;
	using (Transaction tr = db.TransactionManager.StartTransaction())
	{
		BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
		BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
		Table tb = new Table();
		tb.SetDatabaseDefaults();
	   // Pick location
		tb.Position = ed.GetPoint("\nSpecify table location: ").Value;
		tb.Color = Color.FromRgb(107, 27, 0);         
		tb.LineWeight = LineWeight.LineWeight035;
		tb.Normal = new Vector3d(0, 0, 1);// or the same as Vector3d.Zaxis
		//To speed up regenerate table
		tb.SuppressRegenerateTable(true); //<-- optional
		tb.TableStyle = db.Tablestyle;
		// Set the height and width of the initial row and
		// column belonging by default to the blank Table
		tb.Rows[0].Height = rowHeight;
		tb.Rows[0].BackgroundColor = Color.FromRgb(148, 153, 194);
	   
		tb.Columns[0].Width = colWidth;
		// Add the remaining rows and columns
		tb.InsertRows(1, rowHeight, numRows);
		tb.InsertColumns(1, colWidth, numCols - 1);
		tb.Rows[0].IsMergeAllEnabled = true;
		tb.Rows[0].Height = rowHeight;
		tb.Rows[0].TextHeight = txtHeight;
		tb.Rows[1].Height = rowHeight;
		tb.Rows[1].TextHeight = txtHeight;
		// Add the contents of the Title cell
		Cell tc = tb.Cells[0, 0];
		tc.Contents.Add();
		tc.Contents[0].TextHeight = txtHeight;
		tc.Contents[0].TextString = "Title";
		// Add some flowers
		// Column widths might be vary
		List<double> wids = new List<double>(new double[] {	65,70,70,65 });
		List<string[]> tabledata = new List<string[]>(){
			 new string[]{"alpha","bravo","charlie","delta"},
			 new string[]{"echo","foxtrott","golf","hotel"},
			 new string[]{"india","juliett","kilo","lima"},
			 new string[]{"mike","november","oscar","papa"},
			 new string[]{"quebec","romeo","sierra","tango"},
			 new string[]{"uniform","victor","whiskey","x-ray"},
		 new string[]{"yankee","zulu","fixo","over"}};
		for (int j = 0; j < numCols; j++)
		{
			tb.Columns[j].Width = wids[j];
			tb.Columns[j].Alignment = CellAlignment.MiddleCenter;
			short color=16;
			if (j > 0)  color = 16;
			tb.Columns[j].ContentColor = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, color);
		}
		for (int i = 1; i <= numRows; i++)
		{
			tb.Rows[i].Height = rowHeight; tb.Rows[i].TextStyleId = db.Textstyle;
			//tb.Rows[i].TextStyleId = txtId '<-- potential crash of Acad, you can  use SetTextStyle method also, not sure about, maybe it obsolete in this Acad version
			tb.Rows[i].Borders.Horizontal.Margin = rowHeight * 0.1;
			tb.Rows[i].Borders.Vertical.Margin = rowHeight * 0.1;
		}

		// Align Title text to center
		tb.Rows[0].Alignment = CellAlignment.MiddleCenter;
		tb.Rows[0].ContentColor = Color.FromRgb(255, 200, 36);
		// Populate the contents of the header and data sections
		// int n = 0;
		for (int i = 1; i <= numRows; i++)
		{
			string[] items = tabledata[i - 1];
			// tb.Rows[i].Style = "Data";

			for (int j = 0; j <= numCols - 1; j++)
			{
				Cell c = tb.Cells[i, j];
				c.Contents.Add();
				c.Contents[0].TextHeight = txtHeight;
				string txt = items[j];
				c.Contents[0].TextString = txt;
				c.Alignment = CellAlignment.MiddleCenter;
				// Set the vertical margins
				c.Borders.Top.Margin = verMarg;
				c.Borders.Bottom.Margin = verMarg;
				// Set the horizontal margins
				c.Borders.Left.Margin = horMarg;
				c.Borders.Right.Margin = horMarg;
			}
		}
		tb.Rows[1].Alignment = CellAlignment.MiddleCenter;
		tb.InsertRows(1, rowHeight, 2);
		tb.Rows[1].BackgroundColor = Color.FromRgb(226, 214, 187);
		tb.Rows[2].BackgroundColor = Color.FromRgb(226, 214, 187);
	   
	   CellRange mcells = CellRange.Create(tb, 1, 0, 1, 1);
	   tb.MergeCells(mcells);
	   Cell mc = tb.Cells[1, 0];
	   mc.Contents.Add();
	   mc.Contents[0].TextHeight = txtHeight;
	   string mtxt = "Merged";
	   mc.Contents[0].TextString = mtxt;
	   mc.Alignment = CellAlignment.MiddleCenter;


	   mcells = CellRange.Create(tb, 2, 1, 2, 2);
	   tb.MergeCells(mcells);
		mc = tb.Cells[2, 1];
	   mc.Contents.Add();
	   mc.Contents[0].TextHeight = txtHeight;
		mtxt = "Merged";
	   mc.Contents[0].TextString = mtxt;
	   mc.Alignment = CellAlignment.MiddleCenter;

	   mcells = CellRange.Create(tb, 1, 3, 2, 3);
	   tb.MergeCells(mcells);
	   mc = tb.Cells[1, 3];
	   mc.Contents.Add();
	   mc.Contents[0].TextHeight = txtHeight;
	   mtxt = "Merged";
	   mc.Contents[0].TextString = mtxt;
	   mc.Alignment = CellAlignment.MiddleCenter;

	   //tb.Height = tb.Rows.Count * rowHeight; //<-- optional

	   tb.Rows[0].Height *=2.0;
		// insert very last row
	   tb.InsertRows(tb.Rows.Count, rowHeight, 1);
	   tb.Rows[tb.Rows.Count-1].Style = "Data";
		// merge the first 3 cells in row
	   mcells = CellRange.Create(tb, tb.Rows.Count-1, 0, tb.Rows.Count-1, 2);
	   tb.MergeCells(mcells);
	   mc = tb.Cells[tb.Rows.Count-1, 0];
	   mc.Contents.Add();
	   mc.Contents[0].TextHeight = txtHeight;
	   mtxt = "Total:";
	   mc.Contents[0].TextString = mtxt;
	   mc.Alignment = CellAlignment.MiddleRight;

		// alternate coloring rows
	   for (int i = 3; i < tb.Rows.Count-1; i++)
	   {
		   if (i % 2 == 0)
			   tb.Rows[i].BackgroundColor = Color.FromRgb(241, 238, 234);
		   else tb.Rows[i].BackgroundColor = Color.FromRgb(233, 241, 246);

	   }
	   tb.Rows[tb.Rows.Count - 1].BackgroundColor = Color.FromRgb(222, 248, 242);
		tb.GenerateLayout();
		 tb.SuppressRegenerateTable(false); //<-- optional

		 btr.AppendEntity(tb);

		tr.AddNewlyCreatedDBObject(tb, true);
		
		tr.Commit();
		// change lwdisplay to on so the display will be show the table lineweight
		Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("lwdisplay", 1);
		
	}
}