static AcDbObjectId LoadEntity(AcDbEntity* entity)
{
	AcDbBlockTable* pBlockTable;
	acdbHostApplicationServices()->workingDatabase()->getBlockTable(pBlockTable,AcDb::kForRead);
	
	AcDbBlockTableRecord* pBlockTableRecord;
	pBlockTable->getAt(ACDB_MODEL_SPACE,pBlockTableRecord,AcDb::kForWrite);

	AcDbObjectId Id;
	pBlockTableRecord->appendAcDbEntity(Id,entity);

	pBlockTable->close();
	pBlockTableRecord->close();

	entity->close();
	return Id;
}


static void SetLineWeight(double& weight,bool& isCancel)
{
	CString str;
	str.Format(_T("\n禁止开口带线宽: <%.2f>"),weight);


	acedInitGet(RSG_NONEG+RSG_NOZERO,NULL);
	int ret = acedGetReal(str,&weight);
	if(ret == RTCAN)
	{
		isCancel = true;
	}
	else
	{
		isCancel = false;
	}
}

static void TESTcornercmd()
{
	//设置线宽-----------------------------------
	bool isCan = false;
	ads_real lineWeight = DEFAULT_WEIGHT;
	SetLineWeight(lineWeight,isCan);
	if(isCan)
	{
		return;
	}


	//选择一点-----------------------------------


	ads_point ptCorner;
	while(true)
	{
		acedInitGet(RSG_NONULL,_T("S"));
		int retPt = acedGetPoint(NULL,_T("\n点选地块转角[设置线宽(S)]:"),ptCorner);
		if(retPt == RTNORM)
		{
			break;
		}
		else if(retPt == RTKWORD)
		{
			CString strInput;
			acedGetInput(strInput.GetBuffer(10));
			strInput.ReleaseBuffer();
			if(strInput.CompareNoCase(_T("S")) == 0)
			{
				SetLineWeight(lineWeight,isCan);
				if(isCan)
				{
					return;
				}
			}
		}
		else
		{
			return;
		}
	}
	AcGePoint3d ptCorner3d = asPnt3d(ptCorner);


	//设置过滤器--------------------------------------
	resbuf* strFilter = NULL;
	strFilter = acutBuildList(-4,_T("<and"),
		-4,_T("<or"),
		8,_T("C1"),//C1层
		8,_T("C2"),//C2层
		8,_T("C3"),//C3层
		8,_T("R2"),//R2层
		-4,_T("or>"),
		RTDXF0,_T("LWPOLYLINE"),///多段线
		-4,_T("and>"),
		RTNONE);


	ads_name ssName;
	if(acedSSGet(_T("X"),NULL,NULL,strFilter,ssName) != RTNORM)
	{
		acutPrintf(_T("\n未找到符合条件的图形"));
		acutRelRb(strFilter);
		return;
	}


	long len = 0;
	acedSSLength(ssName,&len);
	if(len <=0)
	{
		acutPrintf(_T("\n图形数量是0"));
		acedSSFree(ssName);
		acutRelRb(strFilter);
		return;
	}


	//遍历选择集，找到距离点击位置最近的线段，计算出相邻线段的单位向量----------------------------------
	ads_name en;
	AcDbObjectId eId;
	AcDbPolyline* plMin = NULL;
	int plIndex = 0;
	AcDbPolyline::SegType segType;


	AcGePoint3d ptClosest;
	AcGePoint3d ptMinClose;
	double minDist = 0.0;


	for(int i = 0;i < len;i++)
	{


		int rName = acedSSName(ssName,i,en);
		if(rName != RTNORM)
		{
			acutPrintf(_T("\n获取名称失败"));
			continue;
		}


		Acad::ErrorStatus es = acdbGetObjectId(eId,en);
		if(es!=Acad::eOk)
		{
			acutPrintf(_T("\n获取ID失败"));
			continue;
		}
		AcDbEntity* pEnt = NULL;

		es = acdbOpenObject(pEnt,eId,AcDb::OpenMode::kForRead);
		if(es != Acad::eOk)
		{
			acutPrintf(_T("\n打开实体失败"));
			continue;
		}


		if(!pEnt->isKindOf(AcDbPolyline::desc()))
		{
			acutPrintf(_T("\n第%d个实体不是多段线"),i);
			pEnt->close();
			continue;
		}


		AcDbPolyline* pPoly = NULL;
		pPoly = (AcDbPolyline*)pEnt;


		es = pPoly->getClosestPointTo(ptCorner3d,ptClosest);
		if(es != Acad::eOk)
		{
			acutPrintf(_T("\n获取最小距离点出错"));
			pEnt->close();
			continue;
		}


		double minParam = 0.0;
		es = pPoly->getParamAtPoint(ptClosest,minParam);
		if(es != Acad::eOk)
		{
			pEnt->close();
			continue;
		}


		double dist = ptCorner3d.distanceTo(ptClosest);
		if(i == 0)
		{
			minDist = dist;
		}

		else if(dist >= minDist && i > 0)
		{
			pEnt->close();
			continue;
		}


		minDist = dist;
		AcDbPolyline::SegType st = pPoly->segType(minParam);
		if(st == AcDbPolyline::SegType::kArc)
		{
			AcGeCircArc3d arc3d;
			pPoly->getArcSegAt(minParam,arc3d);


			segType = st;
			plMin = AcDbPolyline::cast(pPoly->clone());
			plIndex = minParam;
			ptMinClose = ptClosest;


		}
		else if(st == AcDbPolyline::SegType::kLine)
		{
			AcGeLineSeg3d line3d;
			pPoly->getLineSegAt(minParam,line3d);


			segType = st;
			plMin = AcDbPolyline::cast(pPoly->clone());
			plIndex = minParam;
			ptMinClose = ptClosest;


		}
		pEnt->close();
	}


	//释放资源-------------------------------------------
	//pEnt->close();
	acutRelRb(strFilter);
	acedSSFree(ssName);


	//判断最小距离是否超出了系统允许的最大值
	if(minDist >MINDISTANCE)
	{
		acutPrintf(_T("\n最小距离至少为%.2f"),MINDISTANCE);
		delete plMin;
		return;
	}


	//绘制转角线段-------------------------------------------------
	AcDbPolyline * pLine = new AcDbPolyline();

	////线段
	if(segType == AcDbPolyline::SegType::kLine)
	{
		AcGeLineSeg2d line2d;
		plMin->getLineSegAt(plIndex,line2d);


		pLine->addVertexAt(0,line2d.startPoint());
		pLine->addVertexAt(1,line2d.endPoint());
	}
	////弧
	else if(segType == AcDbPolyline::SegType::kArc)
	{
		AcGeCircArc2d arc2d;
		plMin->getArcSegAt(plIndex,arc2d);
		double bulge = 0.0;
		plMin->getBulgeAt(plIndex,bulge);


		pLine->addVertexAt(0,arc2d.startPoint(),bulge);
		pLine->addVertexAt(1,arc2d.endPoint());
	}


	Acad::ErrorStatus es = pLine->setLayer(strLayer);
	if(Acad::eOk != es)
	{
		acutPrintf(_T("\n设置层名称出错"));
	}


	es = pLine->setConstantWidth(lineWeight);
	if(Acad::eOk != es)
	{
		acutPrintf(_T("\n设置曲线宽度出错"));
	}


	AcDbObjectIdArray idArr;
	AcDbObjectId idLine = LoadEntity(pLine);
	idArr.append(idLine);////记录ObjId


	//绘制禁止开口两边-------------------------


	////第一个邻边
	//--------------箭头-----------------------

	AcGePoint3d pt3dS;
	AcGePoint3d pt3dE;
	AcGePoint2d pt2dS;
	AcGePoint2d pt2dE;

	bool gotonext = true;
	if(segType == AcDbPolyline::SegType::kArc)
	{
		AcGeCircArc3d arc3d;
		plMin->getArcSegAt(plIndex,arc3d);
		AcGeCircArc2d arc2d;
		plMin->getArcSegAt(plIndex,arc2d);
		pt3dS = arc3d.startPoint();
		pt3dE = arc3d.endPoint();
		pt2dS = arc2d.startPoint();
		pt2dE = arc2d.endPoint();


	}
	else
	{
		AcGeLineSeg3d line3d;
		plMin->getLineSegAt(plIndex,line3d);
		AcGeLineSeg2d line2d;
		plMin->getLineSegAt(plIndex,line2d);


		pt3dS = line3d.startPoint();
		pt3dE = line3d.endPoint();
		pt2dS = line2d.startPoint();
		pt2dE = line2d.endPoint();
	}


	AcDbObjectId idPointer1;
	DrawPointer(plMin,!gotonext,plIndex,pt3dS,idPointer1);


	ads_real dis=DEFAULT_LEN;


	acedInitGet(RSG_NOZERO+RSG_NONEG,_T("P"));
	CString strDis ;
	strDis.Format(_T("\n输入该方向的禁止开口长度[从点击位置算起(P)]<%.2f>:"),dis);
	int ret = acedGetReal(strDis,&dis);


	int polyIndex = 0;
	if(ret == RTCAN)
	{
		EraseIds(idArr);
		RemoveEntity(idPointer1);
		delete plMin;
		return;
	}

	else if(ret == RTKWORD)
	{
		CString str;
		acedGetInput(str.GetBuffer(2));
		str.ReleaseBuffer();
		double length = 0.0;
		EraseIds(idArr);
		if(str.CompareNoCase(_T("P")) == 0)
		{
			AcDbObjectIdArray pIdArr;


			strDis.Format(_T("\n输入该方向的禁止开口长度<%.2f>:"),dis);
			acedInitGet(RSG_NOZERO+RSG_NONEG,NULL);
			ret = acedGetReal(strDis,&dis);
			if(ret == RTCAN)
			{
				RemoveEntity(idPointer1);
				delete plMin;
				return;
			}

			double disS = 0.0;
			double disE = 0.0;
			double disC = 0.0;
			plMin->getDistAtPoint(ptMinClose,disC);
			plMin->getDistAtPoint(pt3dS,disS);
			plMin->getDistAtPoint(pt3dE,disE);


			AcGePoint2d ptClose2d;
			Pt3dTo2d(ptMinClose,ptClose2d);
			double curBulge = 0.0;
			plMin->getBulgeAt(plIndex,curBulge);


			////向前面画
			length = abs(disS - disC);

			AcDbPolyline* pl1 = new AcDbPolyline();
			pl1->setConstantWidth(lineWeight);
			pl1->setLayer(strLayer);
			int pl1Index = 0;

			StartWithP(plMin,plIndex,dis,length,segType,ptClose2d,!gotonext,pl1,pl1Index);


			AcDbObjectId idPl1 = LoadEntity(pl1);
			pIdArr.append(idPl1);
			////移除第1个箭头
			RemoveEntity(idPointer1);


			////--------------------向后面画--先画箭头------------------------
			AcDbObjectId idPointer2;
			DrawPointer(plMin,gotonext,plIndex,pt3dE,idPointer2);

			int count = plMin->numVerts();


			AcDbPolyline* pl2 = new AcDbPolyline();
			pl2->setConstantWidth(lineWeight);
			pl2->setLayer(strLayer);
			int pl2Index = 0;
			if(plIndex == count - 1)
			{
				double total = 0.0;
				double endParam = 0.0;
				plMin->getEndParam(endParam);
				plMin->getDistAtParam(endParam,total);
				disE = total;
			}
			length = abs(disC - disE);


			strDis.Format(_T("\n输入该方向的禁止开口长度<%.2f>:"),dis);
			acedInitGet(RSG_NOZERO+RSG_NONEG,NULL);
			ret = acedGetReal(strDis,&dis);
			if(ret == RTCAN)
			{
				delete plMin;
				RemoveEntity(idPointer2);
				EraseIds(pIdArr);
				return;
			}

			StartWithP(plMin,plIndex,dis,length,segType,ptClose2d,gotonext,pl2,pl2Index);


			AcDbObjectId idPl2 = LoadEntity(pl2);
			pIdArr.append(idPl2);

			////移除第二个箭头
			RemoveEntity(idPointer2);



			AcDbPolyline* plAll = new AcDbPolyline();
			plAll->setConstantWidth(lineWeight);
			plAll->setLayer(strLayer);

			int plAllIndex = 0;
			AppendPLinePoint(idPl1,!gotonext,plAll,plAllIndex);
			AppendPLinePoint(idPl2,gotonext,plAll,plAllIndex);
			LoadEntity(plAll);


			EraseIds(pIdArr);
		}


		delete plMin;
		return;
	}
	//不是点击位置开始的情况------------------------------------



	//第一条边

	AcDbPolyline* pLine1 = new AcDbPolyline();
	pLine1->setConstantWidth(lineWeight);
	pLine1->setLayer(strLayer);
	int pLine1Index = 0;
	AcGePoint2d ptNextS;
	AcGePoint2d ptNextE;
	GetNextPt(plMin,!gotonext,plIndex,ptNextS,ptNextE);


	DrawByLen(!gotonext,ptNextE,ptNextS,dis,plMin,pLine1,pLine1Index);

	AcDbObjectId idLine1 = LoadEntity(pLine1);
	idArr.append(idLine1);
	RemoveEntity(idPointer1);

	//第二条边

	strDis.Format(_T("\n输入该方向的禁止开口长度<%.2f>:"),dis);
	acedInitGet(RSG_NOZERO+RSG_NONEG,NULL);
	ret = acedGetReal(strDis,&dis);
	if(ret == RTCAN)
	{
		delete plMin;
		EraseIds(idArr);
		return;
	}


	AcDbObjectId idPointer2;
	DrawPointer(plMin,gotonext,plIndex,pt3dE,idPointer2);


	AcDbPolyline* pLine2 = new AcDbPolyline();
	pLine2->setConstantWidth(lineWeight);
	pLine2->setLayer(strLayer);


	int pLine2Index = 0;
	GetNextPt(plMin,gotonext,plIndex,ptNextS,ptNextE);
	DrawByLen(gotonext,ptNextS,ptNextE,dis,plMin,pLine2,pLine2Index);
	AcDbObjectId idLine2 = LoadEntity(pLine2);


	idArr.append(idLine2);
	RemoveEntity(idPointer2);
	delete plMin;


	//合并多段线----------------------------------------
	AcDbPolyline* pl = new AcDbPolyline();
	pl->setConstantWidth(lineWeight);
	pl->setLayer(strLayer);
	int plIndex1 = 0;
	AppendPLinePoint(idLine1,false,pl,plIndex1);
	AppendPLinePoint(idLine,true,pl,plIndex1);
	AppendPLinePoint(idLine2,true,pl,plIndex1);


	LoadEntity(pl);


	//////移除之前画上去的3条多段线
	EraseIds(idArr);

}


static void StartWithP(const AcDbPolyline* plMin,const int& plIndex,const double& dis,
	const double& length,const AcDbPolyline::SegType& segType,const AcGePoint2d& ptClose2d,const bool& gotonext,AcDbPolyline* pl1,int& pl1Index)
{
	AcGePoint2d ptS;
	AcGePoint2d ptE;
	double curBulge = 0.0;
	plMin->getBulgeAt(plIndex,curBulge);
	double thisLen = 0.0;


	if(segType == AcDbPolyline::SegType::kArc)
	{
		AcGeCircArc2d arc2d;
		plMin->getArcSegAt(plIndex,arc2d);
		ptS = arc2d.startPoint();
		ptE = arc2d.endPoint();
		thisLen = arc2d.length(arc2d.paramOf(ptS),arc2d.paramOf(ptE));
	}
	else
	{
		AcGeLineSeg2d line2d;
		plMin->getLineSegAt(plIndex,line2d);
		ptS = line2d.startPoint();
		ptE = line2d.endPoint();
	}




	if(dis > length)
	{
		double bulge1 = 0.0;
		if(segType == AcDbPolyline::SegType::kArc)
		{
			bulge1 = tan(atan(curBulge) * length / thisLen);
		}

		if(!gotonext)
		{
			pl1->addVertexAt(pl1Index,ptClose2d,-bulge1);
			pl1Index++;
			pl1->addVertexAt(pl1Index,ptS);
			pl1Index ++;
		}
		else
		{
			pl1->addVertexAt(pl1Index,ptClose2d,bulge1);
			pl1Index++;
			pl1->addVertexAt(pl1Index,ptE);
			pl1Index ++;
		}


		AcGePoint2d ptNextS;
		AcGePoint2d ptNextE;
		GetNextPt(plMin,gotonext,plIndex,ptNextS,ptNextE);


		DrawByLen(gotonext,ptNextS,ptNextE,dis-length,plMin,pl1,pl1Index);
	}
	else
	{
		AcGePoint2d ptEnding;
		if(segType == AcDbPolyline::SegType::kArc)
		{
			AcGeCircArc2d arc2d;
			plMin->getArcSegAt(plIndex,arc2d);
			if(!gotonext)
			{
				GetPtAtDistOnCurve(&arc2d,ptClose2d,dis,ptEnding,Adesk::kFalse);
			}
			else
			{
				GetPtAtDistOnCurve(&arc2d,ptClose2d,dis,ptEnding,Adesk::kTrue);
			}
		}
		else
		{
			AcGeLineSeg2d line2d;
			plMin->getLineSegAt(plIndex,line2d);

			if(!gotonext)
			{
				GetPtAtDistOnCurve(&line2d,ptClose2d,dis,ptEnding,Adesk::kFalse);
			}
			else
			{
				GetPtAtDistOnCurve(&line2d,ptClose2d,dis,ptEnding,Adesk::kTrue);
			}
		}


		double bulge1 = 0.0;
		if(segType == AcDbPolyline::SegType::kArc)
		{
			bulge1 = tan(atan(curBulge) * dis / thisLen);
		}


		if(!gotonext)
		{
			pl1->addVertexAt(pl1Index,ptClose2d,-bulge1);
		}
		else
		{
			pl1->addVertexAt(pl1Index,ptClose2d,bulge1);
		}
		pl1Index++;
		pl1->addVertexAt(pl1Index,ptEnding);
	}

}


static void GetNextPt(const AcDbPolyline* plMin,const bool& gotonext,const int& plIndex,AcGePoint2d& ptNextS,AcGePoint2d& ptNextE)
{
	int nextIndex = 0;
	int count = plMin->numVerts();
	if(!gotonext)
	{
		if(plIndex > 0)
		{
			nextIndex = plIndex - 1;
		}
		else
		{
			nextIndex = count - 1;
		}
	}
	else
	{
		if(plIndex < count - 1)
		{
			nextIndex = plIndex + 1;
		}
		else
		{
			nextIndex = 0;
		}
	}


	AcDbPolyline::SegType nextType = plMin->segType(nextIndex);


	if(nextType == AcDbPolyline::SegType::kArc)
	{
		AcGeCircArc2d arc2d;
		plMin->getArcSegAt(nextIndex,arc2d);
		ptNextS = arc2d.startPoint();
		ptNextE = arc2d.endPoint();
	}
	else
	{
		AcGeLineSeg2d line2d;
		plMin->getLineSegAt(nextIndex,line2d);
		ptNextS = line2d.startPoint();
		ptNextE = line2d.endPoint();
	}


}


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


static void EraseIds(AcDbObjectIdArray idArr)
{
	if(idArr == NULL || idArr.length() == 0)
	{
		return;
	}
	for(int i = 0;i < idArr.length(); i++)
	{
		AcDbEntity* pDel = NULL;
		if(Acad::eOk != acdbOpenObject(pDel,idArr.at(i),AcDb::OpenMode::kForWrite))
		{
			continue;
		}
		if(Acad::eOk != pDel->erase())
		{
			acutPrintf(_T("\n删除第%d个实体失败"),i);
		}
		pDel->close();
	}
}


////绘制箭头(ptStart 为转角线段的起点)
static void DrawPointer(const AcDbPolyline* pl,bool gotonext,const int& plIndex,const AcGePoint3d& ptDraw,AcDbObjectId& idPointer)
{
	//--当前边相关信息
	AcGePoint3d ptCurStart;
	AcGePoint3d ptCurEnd;
	AcGeCircArc3d arc3dCur;
	AcGeLineSeg3d line3dCur;


	AcDbPolyline::SegType curType = pl->segType(plIndex);
	if(curType == AcDbPolyline::SegType::kArc)
	{
		pl->getArcSegAt(plIndex,arc3dCur);
		ptCurStart = arc3dCur.startPoint();
		ptCurEnd = arc3dCur.endPoint();
	}
	else
	{
		pl->getLineSegAt(plIndex,line3dCur);
		ptCurStart = line3dCur.startPoint();
		ptCurEnd = line3dCur.endPoint();
	}

	double paramDraw = 0.0;
	if(pl->getParamAtPoint(ptDraw,paramDraw)!=Acad::eOk)
	{
		return;
	}
	AcGeVector3d v3d;
	pl->getFirstDeriv(paramDraw,v3d);
	AcGeVector2d v(v3d[X],v3d[Y]);
	AcGePoint2d pt0(ptDraw[X],ptDraw[Y]);

	v.normalize();
	if(!gotonext)
	{
		v = -v;
	}
	AcGeVector2d vVer = v;
	////绘制箭头
	AcDbPolyline* pLine = new AcDbPolyline();
	pLine->addVertexAt(0,pt0);


	vVer.rotateBy(PI/2);
	AcGePoint2d pt1 = pt0 + vVer *POINTERWIDTH / 2;
	pLine->addVertexAt(1,pt1);


	vVer.rotateBy(-PI/2);
	AcGePoint2d pt2 = pt1 + vVer * POINTERLENGTH;
	pLine->addVertexAt(2,pt2);


	vVer.rotateBy(PI/2);
	AcGePoint2d pt3 = pt2 + vVer * POINTERWIDTH / 2;
	pLine->addVertexAt(3,pt3);


	vVer.rotateBy(- 3 * PI/4);
	AcGePoint2d pt4 = pt3 + vVer * sqrt(2.0) * POINTERWIDTH;
	pLine->addVertexAt(4,pt4);


	vVer.rotateBy(-PI/2);
	AcGePoint2d pt5 = pt4 + vVer * sqrt(2.0) * POINTERWIDTH;
	pLine->addVertexAt(5,pt5);


	vVer.rotateBy(-3 * PI/4);
	AcGePoint2d pt6 = pt5 + vVer * POINTERWIDTH / 2;
	pLine->addVertexAt(6,pt6);


	vVer.rotateBy(PI/2);
	AcGePoint2d pt7 = pt6 + vVer * POINTERLENGTH ;
	pLine->addVertexAt(7,pt7);


	vVer.rotateBy(-PI/2);
	AcGePoint2d pt8 = pt7 + vVer * POINTERWIDTH / 2;
	pLine->addVertexAt(8,pt8);
	idPointer = LoadEntity(pLine);
}


static void RemoveEntity(AcDbObjectId entId)
{
	AcDbEntity* pEnt = NULL;
	if(acdbOpenObject(pEnt,entId,AcDb::OpenMode::kForWrite) == Acad::eOk)
	{
		if(pEnt->erase() != Acad::eOk)
		{
			acutPrintf(_T("\n移除对象失败"));
			return;
		}
	}
	else
	{
		acutPrintf(_T("\n打开对象失败"));
	}
	pEnt->close();
}

////2d坐标转为3d
static void Pt2dTo3d(AcGePoint2d& pt2d,AcGePoint3d& pt3d)
{
	pt3d[X] = pt2d[X];
	pt3d[Y] = pt2d[Y];
	pt3d[Z] = 0;
}


////3d坐标转化为X,Y轴上的2D坐标
static void Pt3dTo2d(const AcGePoint3d& pt3d,AcGePoint2d& pt2d)
{
	pt2d = pt3d.convert2d(AcGePlane(AcGePoint3d::kOrigin, AcGeVector3d(0,0, 1)));
}


//from:起点，to:终点（这两点要相邻）
//paramDis:沿着多段线画多长
//pl:多段线
//pPoly:新的多段线
static void DrawByLen(const bool& gotoNext ,const AcGePoint2d& from,const AcGePoint2d& to,const double& paramDis,const AcDbPolyline* pl,AcDbPolyline* pPoly,int& polyIndex)
{
	if(paramDis <= 0)
	{
		return;
	}
	int len = pl->numVerts();

	AcGeCircArc2d arc2d;
	AcGeLineSeg2d line2d;


	AcGePoint2d ptS;
	AcGePoint2d ptE;
	bool isFind = false;
	int plIndex = 0;
	AcGeCurve2d* pCurve = NULL;

	for(int i = 0;i < len;i++)
	{
		AcDbPolyline::SegType st = pl->segType(i);

		if(st == AcDbPolyline::SegType::kArc)
		{
			pl->getArcSegAt(i,arc2d);
			pCurve = &arc2d;
		}
		else if(st == AcDbPolyline::SegType::kLine)
		{
			pl->getLineSegAt(i,line2d);
			pCurve = &line2d;
		}


		if(!pCurve->hasStartPoint(ptS) || !pCurve->hasEndPoint(ptE))
		{
			continue;
		}

		if(ptS == from && ptE == to || ptS == to && ptE == from)
		{
			plIndex = i;
			isFind = true;
			break;
		}
	}


	double sumDis = 0.0;
	if(isFind)
	{
		DrawIt(gotoNext,pl,paramDis,from,polyIndex,plIndex,sumDis,pPoly);
	}
	else
	{
		acutPrintf(_T("\nnot found"));
	}
}


//summary////
//指定一个起点和一条多段线，沿着多段线画出指定距离，递归执行，每次往后（前）移动一个点，直到画完指定的距离，
//pl:多段线
//paramDis:画多长
//ptStart:起始点
//polyIndex:添加到第几个了
//plIndex，遍历到多段线第几条线
//isSToE，遍历的顺序1：从前向后  0：从后向前
//sumDis，目前画的总长度
//pPoly:画出来的多段线
static void DrawIt(const bool& gotoNext,const AcDbPolyline* pl,const double& paramDis,const AcGePoint2d& ptStart,int& polyIndex,int& plIndex,double& sumDis,AcDbPolyline* pPoly)
{

	AcDbPolyline::SegType st = pl->segType(plIndex);
	AcGePoint2d ptS;
	AcGePoint2d ptE;
	double leftDis = 0.0;
	double curveDis = 0.0;
	double bulge = 0.0;
	AcGeCurve2d* pCurve = NULL;
	AcGeCircArc2d arc2d;
	AcGeLineSeg2d line2d;
	int len = pl->numVerts();


	if(polyIndex == 2*(len - 2))
	{
		acutPrintf(_T("\nend poly is %d"),polyIndex);
		return;
	}


	if(st == AcDbPolyline::SegType::kArc)
	{
		pl->getArcSegAt(plIndex,arc2d);
		pCurve = &arc2d;////！！！注意：指针的生命周期一定要大于等于指向的变量的生命周期，否则变量release掉指针就空了，再次使用指针程序直接崩溃！！
	}
	else if(st == AcDbPolyline::SegType::kLine)
	{
		pl->getLineSegAt(plIndex,line2d);
		pCurve = &line2d;
	}
	if(!pCurve->hasStartPoint(ptS) || !pCurve->hasEndPoint(ptE))
	{
		return;
	}
	curveDis = pCurve->length(pCurve->paramOf(ptS),pCurve->paramOf(ptE));
	leftDis = paramDis - sumDis;


	pl->getBulgeAt(plIndex,bulge);


	if(curveDis > leftDis)
	{
		double paramEnding = 0.0;

		if(gotoNext)
		{
			AcGePoint2d ptEnding;
			AcGePoint2d ptS;
			pCurve->hasStartPoint(ptS);
			GetPtAtDistOnCurve(pCurve,ptS,leftDis,ptEnding,Adesk::kTrue);


			bulge = tan(atan(bulge) * leftDis/curveDis);


			pPoly->addVertexAt(polyIndex,ptS,bulge);
			polyIndex ++;
			pPoly->addVertexAt(polyIndex,ptEnding);
		}
		else
		{
			AcGePoint2d ptEnding;


			AcGePoint2d ptE;
			pCurve->hasEndPoint(ptE);
			GetPtAtDistOnCurve(pCurve,ptE,leftDis,ptEnding,Adesk::kFalse);


			bulge = tan(atan(bulge) * leftDis/curveDis);


			pPoly->addVertexAt(polyIndex,ptE,-bulge);
			polyIndex ++;
			pPoly->addVertexAt(polyIndex,ptEnding);
		}
		return;
	}
	else
	{
		if(gotoNext)
		{
			pPoly->addVertexAt(polyIndex,ptS,bulge);
			polyIndex ++;
			pPoly->addVertexAt(polyIndex,ptE);
			polyIndex ++;
			//acutPrintf(_T("\nplIndex is %d,poly is %d。is goto next,bulge is %.2f"),plIndex,polyIndex,bulge);
		}
		else
		{
			pPoly->addVertexAt(polyIndex,ptE,-bulge);
			polyIndex ++;
			pPoly->addVertexAt(polyIndex,ptS);
			polyIndex ++;

		}
		/*acutPrintf(_T("\nptS[X] :%.2f,ptS[Y]:%.2f,ptE[X]:%.2f,ptE[Y]:%.2f"),ptS[X],ptS[Y],ptE[X],ptE[Y]);*/
		sumDis += curveDis;

	}


	if(gotoNext)
	{
		plIndex = plIndex < len - 1  ? ++plIndex : 0;
	}
	else
	{
		plIndex = plIndex > 0 ? --plIndex : len - 1;
	}


	DrawIt(gotoNext,pl,paramDis,ptStart,polyIndex,plIndex,sumDis,pPoly);


}

////反回曲线上一定距离的点（默认从起点开始计算）
////pCurve:曲线指针，dist：距离，point：要返回的点
////Adesk::Boolean isGotoNext  true:沿着正向寻找，false：沿着反方向寻找
static void GetPtAtDistOnCurve(const AcGeCurve2d* pCurve,const AcGePoint2d& ptInput,double dist,AcGePoint2d& point,Adesk::Boolean isGotoNext)
{
	if(pCurve == NULL)
	{
		return;
	}
	AcGePoint2d ptS;
	ptS = ptInput;
	double pa = 0.0;
	double datumParam = 0.0;
	//Adesk::Boolean posParamDir = Adesk::kTrue;

	datumParam = pCurve->paramOf(ptS);
	pa = pCurve->paramAtLength(datumParam,dist,isGotoNext);
	point = pCurve->evalPoint(pa);
}