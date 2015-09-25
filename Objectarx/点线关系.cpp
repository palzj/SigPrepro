static void LESQGBPoly17_GETPOLY(void)
{
	ads_name sspol;
	resbuf *rbFilter = acutBuildList(RTDXF0, _T("LWPOLYLINE"), RTNONE);
	if (acedSSGet(NULL, NULL, NULL, rbFilter, sspol) != RTNORM)
	{
		acutRelRb(rbFilter); 
		return;
	}
	acutRelRb(rbFilter);

	long len = 0;
	if ((acedSSLength(sspol, &len) != RTNORM) || (len == 0))
	{
		acedSSFree(sspol); 
		return;
	}

	AcGePoint3d worldPt;
	if (acedGetPoint(NULL,_T("\nPick internal point: "),asDblArray(worldPt)) != RTNORM)
		return;
	for (int i=0; i<len; i++)
	{
		AcDbMPolygon mpol;
		AcDbObjectId objId;
		ads_name ename;
		acedSSName(sspol, i, ename);
		if (acdbGetObjectId(objId, ename) != Acad::eOk) 
			return;
		AcDbObjectPointer <AcDbPolyline> pPoly(objId, AcDb::kForWrite);//for write test to erase, or color change
		if (pPoly.openStatus() == Acad::eOk)
		{
			acdbUcs2Wcs(asDblArray(worldPt),asDblArray(worldPt),false);
			AcGeVector3d vDir = AcGeVector3d::kZAxis;
			resbuf rb; acedGetVar(_T("viewdir"),&rb);
			vDir = asVec3d(rb.resval.rpoint);
			AcDb::Planarity plan_type;
			AcGePlane plane;
			pPoly->getPlane(plane,plan_type);
			worldPt = worldPt.project(plane,vDir);
			AcGeIntArray loopsArray; //loopsArray.setLogicalLength(0);
			if (mpol.appendLoopFromBoundary(pPoly) == Acad::eOk)
			{
				if ( mpol.isPointInsideMPolygon(worldPt, loopsArray) > 0)
				{
					acutPrintf(_T("\nPoint inside..."));
					pPoly.object()->setColorIndex(6);
				}
			}
		}
	}
	acedSSFree(sspol);
}
	
static void BrepPointCheckPoint(void)
{
	Acad::ErrorStatus es;
	AcBr::ErrorStatus ebr;
	
	ads_name en;
	ads_point pt;
	if (acedEntSel(_T("\nSelect contour: "), en, pt) != RTNORM)
		return;
	AcDbObjectId eId; 
	acdbGetObjectId(eId,en);
	AcDbObjectPointer<AcDbCurve> pline(eId,AcDb::kForRead) ;
	if ((es = pline.openStatus()) != Acad::eOk) 
	{
		acutPrintf(_T("\npline.openStatus()=%s"),acadErrorStatusText(es));
		return;
	}
	if (acedGetPoint(pt,_T("\nPick point: "), pt) != RTNORM)
		return;

	AcDbVoidPtrArray ar, regions;
	ar.append(pline.object());
	if ((es = AcDbRegion::createFromCurves(ar,regions)) != Acad::eOk)  
	{
		acutPrintf(_T("\nAcDbRegion::createFromCurves(ar,regions)=%s"),acadErrorStatusText(es));
		return;
	}

	AcDbRegion reg; 
	reg.copyFrom((AcDbRegion *)regions[0]);
	for (int i=0; i<regions.length();i++) 
		delete regions[i];

	AcBrBrep brEnt;  
	ebr = brEnt.set(reg);
	if (ebr != AcBr::eOk) 
	{
		acutPrintf(_T("\nbrEnt.set(sol)=%s"),acadErrorStatusText((Acad::ErrorStatus)(Adesk::UInt32)ebr));
		return;
	}

	AcGe::PointContainment pDesc;
	AcBrEntity *pCont = NULL;
	AcBrBrepFaceTraverser brepFaceTrav; 
	brepFaceTrav.setBrep(brEnt);
	AcBr::ErrorStatus err = AcBr::eInvalidInput;
	while (!brepFaceTrav.done()) 
	{
		AcBrFace brFace; 
		brepFaceTrav.getFace(brFace);
		err = brFace.getPointContainment(asPnt3d(pt),pDesc,pCont);
		if (err == Acad::eOk && pDesc == AcGe::kInside) 
		{
			acedAlert(_T("In")); 
			return;
		} 
		else if (err == Acad::eOk && pDesc == AcGe::kOnBoundary) 
		{
			acedAlert(_T("On")); 
			return;
		}
		brepFaceTrav.next();
	}
  
	if (err == Acad::eOk) 
	{
		acedAlert(_T("Out"));
	} 
	else 
	{
		acedAlert(_T("Unknown error"));
	}

	return;
}