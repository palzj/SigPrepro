Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
TypedValue[] value = new TypedValue[1];
value.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), 0);
SelectionFilter filter = new SelectionFilter(value);
PromptSelectionOptions option = new PromptSelectionOptions();
option.MessageForAdding = "Selection polyline for output!";
Database acdb = doc.Database;
using (Transaction tran = acdb.TransactionManager.StartTransaction())
{
	PromptSelectionResult result = ed.GetSelection(option, filter);
	//PromptSelectionResult result = ed.GetSelection(option);
	if (result.Status == PromptStatus.OK)
	{
		SelectionSet selectionSet = result.Value;
		foreach (ObjectId sel in selectionSet.GetObjectIds())
		{
			Entity entity = tran.GetObject(sel, OpenMode.ForRead) as Entity;
			Polyline pline = entity as Polyline;
			if (pline != null)
			{
				TreeNode treeNote = new TreeNode();
				DBElement element = new DBElement(type);
				treeNote.Tag = element;
				treeNote.Text = element.Type.ToString();
				Parent.Nodes.Add(treeNote);
				if (type == DBElementType.Extrusion)
				{
					TreeNode loopNote = new TreeNode();
					DBElement loop = new DBElement(DBElementType.Loop);
					loopNote.Tag = loop;
					loopNote.Text = loop.Type.ToString();
					treeNote.Nodes.Add(loopNote);
					for (int a = 0; a < pline.NumberOfVertices; a++)
					{
						if (pline.GetBulgeAt(a) != 0)
						{
							for (int k = 0; k < 10; k++)
							{
								try
								{
									TreeNode newNote = new TreeNode();
									DBElement vert = new DBElement(DBElementType.Vertex);
									Point3d point = pline.GetPointAtParameter((double)a + (double)k / 10.0);
									vert.Position = new double[] { point.X, point.Y, point.Z };
									newNote.Tag = vert;
									newNote.Text = vert.Type.ToString();
									loopNote.Nodes.Add(newNote);
								}
								catch (Autodesk.AutoCAD.Runtime.Exception e)
								{
									ed.WriteMessage(e.Message);
								}
							}
						}
					}
				}
			}
		}
	}
}