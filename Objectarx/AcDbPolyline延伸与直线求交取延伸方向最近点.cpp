    //AcDbPolyline延伸与直线求交，取延伸方向最近点 
	//基础知识：AcDbPolyline中每个点的Bulge，负数为顺时钟，正数为逆时钟
	//extendStart       是否延伸前端  
    //sPt,ePt;          与直线的两个交点  
    AcGePoint3d getExtendClosePt(bool extendStart,const AcDbPolyline* pl,const AcGePoint3d& sPt,const AcGePoint3d& ePt)  
    {  
        double dBulge;                  //延伸段的Bulge  
        AcGePoint3d pt;                 //多义线上的点  
        if (extendStart)  
        {  
            pl->getBulgeAt(0,dBulge);  
            pl->getStartPoint(pt);  
            dBulge = -dBulge;           //起点延伸时，方向与原方向的Bulge相反  
        }  
        else  
        {  
            pl->getBulgeAt(pl->numVerts()-2,dBulge);//多义线的Bulge保存在前一点  
            pl->getEndPoint(pt);  
        }  
        AcGeVector3d sVec = sPt - pt;  
        AcGeVector3d eVec = ePt - pt;  
        AcGeVector3d vec = sVec.crossProduct(eVec);  
      
        AcGePoint3d closePt;  
        if(sign(dBulge) == sign(vec.z))  
        {  
            closePt = sPt;  
        }  
        else  
        {  
            closePt = ePt;  
        }  
      
        return closePt;  
    }  