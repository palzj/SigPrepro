    //AcDbPolyline������ֱ���󽻣�ȡ���췽������� 
	//����֪ʶ��AcDbPolyline��ÿ�����Bulge������Ϊ˳ʱ�ӣ�����Ϊ��ʱ��
	//extendStart       �Ƿ�����ǰ��  
    //sPt,ePt;          ��ֱ�ߵ���������  
    AcGePoint3d getExtendClosePt(bool extendStart,const AcDbPolyline* pl,const AcGePoint3d& sPt,const AcGePoint3d& ePt)  
    {  
        double dBulge;                  //����ε�Bulge  
        AcGePoint3d pt;                 //�������ϵĵ�  
        if (extendStart)  
        {  
            pl->getBulgeAt(0,dBulge);  
            pl->getStartPoint(pt);  
            dBulge = -dBulge;           //�������ʱ��������ԭ�����Bulge�෴  
        }  
        else  
        {  
            pl->getBulgeAt(pl->numVerts()-2,dBulge);//�����ߵ�Bulge������ǰһ��  
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