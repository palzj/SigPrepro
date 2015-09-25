 [CommandMethod("SendToBottom")]
 public void commandDrawOrderChange()
 {
	 Document activeDoc = Application.DocumentManager.MdiActiveDocument;
	 Database db = activeDoc.Database;
	 Editor ed = activeDoc.Editor;

	 PromptEntityOptions peo = new PromptEntityOptions("Select an entity : ");
	 PromptEntityResult per = ed.GetEntity(peo);
	 if (per.Status != PromptStatus.OK)
	 {
		 return;
	 }
	 ObjectId oid = per.ObjectId;

	 SortedList<long, ObjectId> drawOrder = new SortedList<long, ObjectId>();

	 using (Transaction tr = db.TransactionManager.StartTransaction())
	 {
		 BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
		 BlockTableRecord btrModelSpace = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

		 DrawOrderTable dot = tr.GetObject(btrModelSpace.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;

		 ObjectIdCollection objToMove = new ObjectIdCollection();
		 objToMove.Add(oid);
		 dot.MoveToBottom(objToMove);

		 tr.Commit();
	 }
	 ed.WriteMessage("Done");
 }
 
