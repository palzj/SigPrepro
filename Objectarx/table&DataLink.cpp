void createAndSetDataLink()
	{
		Acad::ErrorStatus es;
	 AcDbTable *pTbl; 
	 pTbl = new AcDbTable();

	 const TCHAR *pTblStyle = ACRX_T("MY_Style");
	   
		// Set the Table Style
		AcDbDictionary *pDict = NULL;
		AcDbObjectId idTblStyle;
		acdbHostApplicationServices()->workingDatabase()->getTableStyleDictionary(pDict,AcDb::kForWrite);
		es = pDict->getAt(pTblStyle,idTblStyle);
		pDict->close();
	   
	 
	 if(Acad::eOk == es) pTbl->setTableStyle(idTblStyle);
		setTableColAndRows(pTbl, 5, 5);

		pTbl->setTextString(3,3,L"my love");

		pTbl->generateLayout(); // Very very important, else expect crashes later on

	  if(NULL == pTbl)
	 {
	  acutPrintf(ACRX_T("\nSelected entity was not a table!"));
	  return;
	 }

	 // Get an Excel file
	 //
	 struct resbuf *result;
	 int rc;

	 if ( (result = acutNewRb(RTSTR)) == NULL)
	 {
	  pTbl->close();
	  acutPrintf(ACRX_T("\nUnable to allocate buffer!"));
	  return;
	 }

	 result->resval.rstring=NULL;

	 rc = acedGetFileD( ACRX_T("Excel File"), // Title
	  ACRX_T("c:\\"),  // Default pathname 
	  ACRX_T("xls"),   // Default extension
	  16,      // Control flags
	  result);    // The path selected by the user.
	 if (rc != RTNORM)
	 {
	  pTbl->close();
	  acutPrintf(ACRX_T("\nError in selecting an EXCEL file!"));
	  return;
	 }

	 // Retrieve the file name from the ResBuf.
	 ACHAR fileName[MAX_PATH];
	 _tcscpy(fileName, result->resval.rstring);
	 rc = acutRelRb(result);

	 static ACHAR sMyDataLink[MAX_PATH] = ACRX_T("TLLink");
	 // Get the Data Link Manager.
	 AcDbDataLinkManager* pDlMan = acdbHostApplicationServices()->workingDatabase()->getDataLinkManager();
	 assert(pDlMan);

	 AcDbObjectId idDL;
	 AcDbDataLink *pDL = NULL; 
	 // Check if a Data Link with the name already exists. If so, remove it.
	 if( pDlMan->getDataLink(sMyDataLink, pDL, AcDb::kForRead) == Acad::eOk && pDL)
	 {
	  pDL->close();
	  es = pDlMan->removeDataLink(sMyDataLink, idDL);
	  if( es != Acad::eOk )
	  {
	   pTbl->close();
	   acutPrintf(ACRX_T("\nError in removing the Data Link!"));
	   return;
	  }
	 }
	 

	 // Create a Data Link with the name.
	 es = pDlMan->createDataLink(ACRX_T("AcExcel"), sMyDataLink, ACRX_T("This is a test for Excel type data link."), fileName, idDL);
	 if( es != Acad::eOk )
	 {
	  pTbl->close();
	  acutPrintf(ACRX_T("\nError in creating Data Link!\nPlease check if there is a sheet named 'Sheet1' in the XLS file."));
	  return;
	 }

	 // Open the Data Link.
	 es = acdbOpenObject<AcDbDataLink>(pDL, idDL, AcDb::kForWrite);
	 if ( es != Acad::eOk || !pDL )
	 {
	  pTbl->close();
	  acutPrintf(ACRX_T("\nError in opening the Data Link object!"));
	  return;
	 }

	 //  Set options of the Data Link
	 es = pDL->setOption(AcDb::kDataLinkOptionPersistCache);
	 es = pDL->setUpdateOption(pDL->updateOption() | AcDb::kUpdateOptionAllowSourceUpdate);

	 // Close the Data Link.
	 pDL->close();

	 // Set data link to the table object at cell(2,2).
	 es = pTbl->setDataLink(0,0, idDL, true);
	 if( es != Acad::eOk )
	 {
	  pTbl->close();
	  acutPrintf(ACRX_T("\nError in setting Data Link to the selected table!\nPlease check if there is a sheet named 'Sheet1' in the XLS file."));
	  return;
	 }
	 pTbl->setPosition(AcGePoint3d(0, 0, 0));
	 // Don't forget to close the table object.
	 
	 // Don't forget to post to the Modelspace.
		pTbl->close();
		
	}