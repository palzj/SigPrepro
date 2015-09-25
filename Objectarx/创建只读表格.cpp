//Here is a sample code that creates a table with the cell state set to "AcDb::kCellStateContentReadOnly"
    1

		AcGePoint3d insPoint;
    2 int ret = acedGetPoint
    3                     (
    4                         NULL,
    5                         _T("\nEnter insertion point: "),
    6                         asDblArray(insPoint)
    7                     );
    8 if(ret != RTNORM)
    9     return;
   10 
   11 Acad::ErrorStatus es;
   12 
   13 AcDbTable *pTable = new AcDbTable();
   14 AcDbDictionary *pDict = NULL;
   15 
   16 AcDbDatabase *pDb
   17         = acdbHostApplicationServices()->workingDatabase();
   18 
   19 es = pDb->getTableStyleDictionary(pDict,AcDb::kForRead);
   20 
   21 AcDbObjectId styleId;
   22 es = pDict->getAt(_T("Standard"), styleId);
   23 es = pDict->close();
   24 
   25 pTable->setTableStyle(styleId);
   26 int rows = 3;
   27 int cols = 2;
   28 pTable->setSize(rows, cols);
   29 
   30 ACHAR content[10];
   31 for(int row = 0; row < rows; row++)
   32 {
   33     for(int col = 0; col < cols; col++)
   34     {
   35         acutSPrintf(content, ACRX_T("%d-%d"), row+1, col+1);
   36         es = pTable->setTextString(row, col, content);
   37 
   38         // Set the cell state to read-only
   39         es = pTable->setCellState
   40                     (
   41                         row,
   42                         col,
   43                         AcDb::kCellStateContentReadOnly
   44                     );
   45     }
   46 }
   47 pTable->generateLayout(); 
   48 pTable->setPosition(insPoint);
   49 
   50 AcDbBlockTable *pBlockTable;
   51 pDb->getSymbolTable(pBlockTable, AcDb::kForRead);
   52 
   53 AcDbBlockTableRecord *pBlockTableRecord;
   54 pBlockTable->getAt
   55                     (
   56                         ACDB_MODEL_SPACE,
   57                         pBlockTableRecord,
   58                         AcDb::kForWrite
   59                     );
   60 pBlockTable->close();
   61 
   62 pBlockTableRecord->appendAcDbEntity(pTable);
   63 pBlockTableRecord->close();
   64 
   65 pTable->close();