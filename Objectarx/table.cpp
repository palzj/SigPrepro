   AcDbBlockTable *pBlkTbl;
   acdbHostApplicationServices()->workingDatabase()->getBlockTable(pBlkTbl, AcDb::kForWrite);
   // ���ģ�Ϳռ�Ŀ���¼
   AcDbBlockTableRecord *pBlkTblRcd;
   pBlkTbl->getAt(ACDB_MODEL_SPACE, pBlkTblRcd,AcDb::kForWrite);

   AcDbTable* pTable = new AcDbTable;

   AcDbDictionary *pDict = NULL;
   AcDbObjectId idTblStyle;
   acdbHostApplicationServices()->workingDatabase()->getTableStyleDictionary(pDict,AcDb::kForRead);
   pDict->getAt(_T("Standard"),idTblStyle);
   pDict->close();

   pTable->setTableStyle( idTblStyle );


   AcDbTextStyleTable* pTextStyle = NULL;
   acdbHostApplicationServices()->workingDatabase()->getTextStyleTable(pTextStyle,AcDb::kForRead);
   AcDbObjectId textID;
   pTextStyle->getAt(_T("Standard"),textID);
   pTextStyle->close();

   if( !textID.isNull() )
   {
    pTable->setTextStyle(textID);
   }

   pTable->setNumColumns(2);
   pTable->setNumRows(4);
  
   pTable->generateLayout(); 
   pTable->suppressHeaderRow(false);//���ñ���


   //��������
   pTable->setPosition(AcGePoint3d(100,100, 0));

   //�����и�
   pTable->setRowHeight(0,30);
   pTable->setRowHeight(1,5);
   pTable->setRowHeight(2,5);
   pTable->setRowHeight(3,5);

   //�����п�
   pTable->setColumnWidth(0,45);
   pTable->setColumnWidth(1,40);

   pTable->setTextString(1,1,_T("sfsfsdfsd"));
   pTable->setAutoScale(1,1,true);

   pBlkTblRcd->appendAcDbEntity(pTable);

   pTable->setRegen();

   pTable->close();
   pBlkTblRcd->close();
   pBlkTbl->close();

   //ˢ����Ļ
   actrTransactionManager->flushGraphics(); /*refresh screen*/
   acedUpdateDisplay(); 