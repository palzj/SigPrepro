AcGePoint3d startPoint3d = pline->startPoint();
AcGePoint3d endPoint3d = pline->endPoint();

AcGeVector3d vectorDircetionLine = endPoint3d - startPoint3d;

AcGeVector3d vectorPolyline = vectorDircetionLine;

vectorPolyline.y = 0;
double dRes = normLine.dotProduct(vectorDircetionLine);

AcDbPolyline *pPolyline = new AcDbPolyline();

AcGeVector3d normPolyline = vectorDircetionLine.crossProduct(vectorPolyline);

pPolyline->setNormal(normPolyline);

AcGeMatrix3d tranformMatrix;
pPolyline->getEcs(tranformMatrix);
AcGeMatrix3d inversTranformMM = tranformMatrix.inverse();
endPoint3d.transformBy(inversTranformMM);
startPoint3d.transformBy(inversTranformMM);
AcGePoint2d startPoint2d(startPoint3d.x,startPoint3d.y);
AcGePoint2d endPoint2d(endPoint3d.x,endPoint3d.y);
pPolyline->addVertexAt(0,startPoint2d);
pPolyline->addVertexAt(1,endPoint2d);
pPolyline->setElevation(endPoint3d.z);