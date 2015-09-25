[CommandMethod("test2")]
public void Test02()
{
	Document doc = Application.DocumentManager.MdiActiveDocument;
	Editor ed = doc.Editor;
	Database db = doc.Database;
	Transaction tr = db.TransactionManager.StartTransaction();
	PromptPointResult ppr = ed.GetPoint("Test Point: ");
	if (ppr.Status != PromptStatus.OK) 
		return;
	PromptEntityResult per = ed.GetEntity("Pick Table: ");
	if (per.Status != PromptStatus.OK) 
		return;
	using (tr)
	{
		try
		{
			Point3d pnt = ppr.Value;
			ObjectId id = per.ObjectId;
			Table table = (Table)id.GetObject(OpenMode.ForRead);
			FullSubentityPath[] vv = new FullSubentityPath[] { };
			var p = Application.GetSystemVariable("VIEWDIR");
			Point3d pp = (Point3d)p;
			Vector3d vec = pp.GetAsVector();
			TableHitTestInfo ww = table.Select(pnt, vec, table.Direction, false, false, vv);
			int row = ww.Row;
			int column = ww.Column;
			Cell cell = table.Cells[row, column];
			int ic = cell.Contents.Count;
			for (int i = 0; i < ic; i++)
			{
				string str = cell.Contents[i].TextString;
				ed.WriteMessage("\nRow = {0}, Column = {1}, Text = {2}", row.ToString(), column.ToString(), str);
			}
			tr.Commit();
		}
		catch (Exception)
		{
			throw;
		}
	}
}