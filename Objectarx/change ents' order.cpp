static void ADSProjectSendToBottom(void)
{
    ads_name ent;
    ads_point pt;
 
    Acad::ErrorStatus es;
    int ret = RTNORM;
    ret = acedEntSel( _T("\nSelect Entity: "),ent,pt);
 
    if (RTNORM != ret)
        return;
 
    AcDbObjectId ent_id;
    if (Acad::eOk != acdbGetObjectId( ent_id, ent ))
        return;
 
    configureSortents();
 
    AcDbEntity *pEnt;
    es = acdbOpenObject( pEnt, ent_id, AcDb::kForRead );
    if (Acad::eOk != es)
        return;
 
    AcDbSortentsTable *pSt = GetSortentsTableOf( pEnt );
 
    pEnt->close();
	
    if (NULL == pSt)
        return;
 
    AcDbObjectIdArray entity_array;
    entity_array.append( ent_id );
 
    pSt->moveToBottom( entity_array );
    pSt->close();
 
    // Send regen command or use the
    // undocumented ads_regen method.
    acDocManager->sendStringToExecute( acDocManager->curDocument(),L"_regen\n",false,true);
}


static void MoveBelow(AcDbObjectId &idDown, AcDbObjectId &idUp)
{
	AcDbSortentsTable *pSortTab = NULL;
	AcDbObjectId spaceId = AcDbObjectId::kNull;
	
	AcDbEntityPointer pEnt(idDown,AcDb::kForRead);
	if (pEnt.openStatus() == Acad::eOk) 
	{
		spaceId = pEnt->ownerId();
		pEnt->close();
	}
	
	if (!spaceId.isNull()) 
	{
		AcDbObjectPointer pBTR(spaceId,AcDb::kForRead);
		if (pBTR.openStatus() == Acad::eOk) 
		{
			if (pBTR->getSortentsTable(pSortTab, AcDb::kForWrite, true) == Acad::eOk) 
			{
				AcDbObjectIdArray ar; 
				ar.append(idDown);
				pSortTab->moveBelow(ar,idUp);
				pSortTab->close();
			}
		}
	}
}