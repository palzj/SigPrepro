// 获取Spline的样本点
bool CCovertSpline::getSplineSamplePoints(AcDbSpline *&spline, AcGePoint3dArray &pnts)
{
	assert(spline != NULL);

	double dbStartParam,dbEndParam;
	if (Acad::eOk != spline->getStartParam(dbStartParam) || Acad::eOk != spline->getEndParam(dbEndParam))
	{
		return false;
	}

	double dStep=m_nImitateMinLenth;
	double dLen1 = 0.0;
	double dLen2 = 0.0;
	double dlength = 0.0;
	spline->getDistAtParam(dbStartParam,dLen1);
	spline->getDistAtParam(dbEndParam,dLen2);
	dlength = dLen2 - dLen1;

	double dPreDis = 0.0;
	double dMidDis = 0.0;

	AcGePoint3d pnt;
	AcGePoint3d predpnt,nextpnt,midPnt;

	spline->getStartPoint(pnt);
	pnts.append(pnt);
	predpnt = pnt;
	if (dlength <0.26)
	{
		dStep /= 2;
		while (dStep > dlength && dStep > 0.001)
		{
			dStep /= 2;
		}
	}
	double dNextDis = dStep;
	do
	{
		/*
		*calculate a next point on segment 
		*/
		bool bFind = false;
		while (dPreDis < dNextDis && (dNextDis < dlength))
		{
			dMidDis = (dPreDis + dNextDis) / 2;
			// get the next step point
			Acad::ErrorStatus es;
			es = spline->getPointAtDist(dMidDis,midPnt);
			es = spline->getPointAtDist(dNextDis,nextpnt);

			assert(midPnt != nextpnt);
			assert(nextpnt != predpnt);
			assert(predpnt != midPnt);
			AcGeVector2d v1 = AcGePoint2d(midPnt.x,midPnt.y)-AcGePoint2d(predpnt.x,predpnt.y);
			AcGeVector2d v2 = AcGePoint2d(nextpnt.x,nextpnt.y)-AcGePoint2d(predpnt.x,predpnt.y);
			// the bulge less than 0.002
			double dBules = tan(v1.angleTo(v2));
			if(dBules > 0.004 )
			{
				if (dNextDis - dPreDis <0.01)
				{
					break;
				}
				dNextDis -= (dNextDis-dMidDis)/2;

			}
			else if(dBules < 0.002)
			{
				if (dlength - dNextDis  <0.001)
				{
					break;
				}
				dNextDis += (dNextDis-dMidDis)/2;

			}
			else
			{
				break;
			}
			assert(dNextDis > dPreDis);
		}
		dPreDis = dNextDis;
		predpnt = nextpnt;
		pnts.append(midPnt);
		pnts.append(predpnt);
		if(dNextDis + dStep > dlength && dStep > 0.007)
		{
			dStep /=2;
		}
		dNextDis += dStep; 
	} while(dNextDis<dlength);
	spline->getDistAtPoint(pnts.last(),dMidDis);
	spline->getPointAtDist((dMidDis+dlength)/2,midPnt);
	pnts.append(midPnt);
	//AcDbCircle *pc = new AcDbCircle;
	//pc->setCenter(midPnt);
	//pc->setColorIndex(1);
	//pc->setRadius(0.5);
	//AddToModelSpace(pc);
	//pc->close();
	spline->getEndPoint(pnt);
	pnts.append(pnt);

	return (pnts.length() > 2);
}
