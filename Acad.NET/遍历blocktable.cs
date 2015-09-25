     [CommandMethod("ListEntities")]
        public static void ListEntities()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                int nCnt = 0;
                acDoc.Editor.WriteMessage("\nModel space objects: ");
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    acDoc.Editor.WriteMessage("\n" + acObjId.ObjectClass.DxfName + acObjId.Handle.ToString());
                    nCnt = nCnt + 1;
                }

                if (nCnt == 0)
                {
                    acDoc.Editor.WriteMessage("\nNo objects found.");
                }
                else
                {
                    acDoc.Editor.WriteMessage("\nTotal {0} objects.", nCnt);
                }

            }
        }