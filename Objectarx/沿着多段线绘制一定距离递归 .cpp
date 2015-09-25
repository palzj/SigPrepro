//from:��㣬to:�յ㣨������Ҫ���ڣ�  
//paramDis:���Ŷ���߻��೤  
//pl:�����  
//pPoly:�µĶ����  
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
//ָ��һ������һ������ߣ����Ŷ���߻���ָ�����룬�ݹ�ִ�У�ÿ������ǰ���ƶ�һ���㣬ֱ������ָ���ľ��룬  
//pl:�����  
//paramDis:���೤  
//ptStart:��ʼ��  
//polyIndex:��ӵ��ڼ�����  
//plIndex������������ߵڼ�����  
//isSToE��������˳��1����ǰ���  0���Ӻ���ǰ  
//sumDis��Ŀǰ�����ܳ���  
//pPoly:�������Ķ����  
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
		pCurve = &arc2d;////������ע�⣺ָ�����������һ��Ҫ���ڵ���ָ��ı������������ڣ��������release��ָ��Ϳ��ˣ��ٴ�ʹ��ָ�����ֱ�ӱ�������  
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
			//acutPrintf(_T("\nplIndex is %d,poly is %d��is goto next,bulge is %.2f"),plIndex,polyIndex,bulge);  
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

////����������һ������ĵ㣨Ĭ�ϴ���㿪ʼ���㣩  
////pCurve:����ָ�룬dist�����룬point��Ҫ���صĵ�  
////Adesk::Boolean isGotoNext  true:��������Ѱ�ң�false�����ŷ�����Ѱ��  
static void GetPtAtDistOnCurve(const AcGeCurve2d* pCurve,const AcGePoint2d& ptInput,
	double dist,AcGePoint2d& point,Adesk::Boolean isGotoNext)  
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