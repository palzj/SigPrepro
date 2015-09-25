////合成多段线  
static void AppendPLinePoint(const AcDbObjectId& id,const bool& gotoNext,AcDbPolyline* pLine,int& plIndex)  
{  
	AcDbEntity* pEnt = NULL;  
	Acad::ErrorStatus es = acdbOpenObject(pEnt,id,AcDb::OpenMode::kForRead);  
	if(es != Acad::eOk)  
	{  
		acutPrintf(_T("open object failed in combine pline"));  
		return;  
	}  
	if(!pEnt->isKindOf(AcDbPolyline::desc()))  
	{  
		pEnt->close();  
		return;  
	}  

	AcDbPolyline* pPoly = NULL;  
	pPoly = (AcDbPolyline*)pEnt;  
	AcGePoint2dArray ptArr;  
	int count = pPoly->numVerts();  
	AcGePoint2d pt ;  
	double bulge = 0.0;  
	if(gotoNext)  
	{  
		for(int i = 0;i < count ; i++)  
		{  
			pPoly->getPointAt(i,pt);  
			pPoly->getBulgeAt(i,bulge);  
			pLine->addVertexAt(plIndex,pt,bulge);  
			plIndex++;  
		}  
	}  
	else  
	{  
		for(int i = count - 1;i > 0; i--)  
		{  
			pPoly->getPointAt(i,pt);  
			if(i > 0)  
			{  
				pPoly->getBulgeAt(i - 1,bulge);  
			}  
			else  
			{  
				pPoly->getBulgeAt(0,bulge);  
			}  
			pLine->addVertexAt(plIndex,pt,-bulge);  
			plIndex++;  
		}  
	}  
	pEnt->close();  
}  