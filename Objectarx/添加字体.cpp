Acad::ErrorStatus SetFont()
{
        Acad::ErrorStatus Er;       
        AcDbTextStyleTable * pTxt;
        Er=acdbCurDwg()->getTextStyleTable(pTxt,AcDb::kForWrite);
        if (Er!=Acad::eOk)
        {
                ads_alert("Error Get TextStyleTable");
                return Er;
        };
        AcDbTextStyleTableRecord* pRecord=new AcDbTextStyleTableRecord;
        pRecord->setFont("simfang.ttf",FALSE,FALSE,0,0);
           pRecord->setName("fqj");
        Er=pTxt->add(pRecord);
        pTxt->close();
        return Er;


}


re
SetFont�������������ڽ�TrueType��������Ϊ������ʽ��
Acad::ErrorStatus setFont(
   const char* pTypeface,
   Adesk::Boolean bold,
   Adesk::Boolean italic,
   int charset,
   int pitchAndFamily) const;
pTypeface        ������
bold        trueΪ����
italic        trueΪб��
charset        �ַ�����ֵ
pitchAndFamily        �־���ַ����ֵ
TrueType��������
TrueType������        charset        pitchAndFamily
����_GB2312          134        49
����                  134        2
����                  134        2
����               134        49
��԰                  134        49
����_GB2312        134        49


Acad::ErrorStatus SetFont()
{
        Acad::ErrorStatus Er;       
        AcDbTextStyleTable * pTxt;
        Er=acdbCurDwg()->getTextStyleTable(pTxt,AcDb::kForWrite);
        if (Er!=Acad::eOk)
        {
                ads_alert("Error Get TextStyleTable");
                return Er;
        };
        AcDbTextStyleTableRecord* pRecord=new AcDbTextStyleTableRecord;
        pRecord->setFont("����_GB2312",FALSE,FALSE,134,49);
        pRecord->setName("fqj");
        Er=pTxt->add(pRecord);
        pTxt->close();
        return Er;
}


re
AcDbTextStyleTable *pTextStyleTable;
   acdbHostApplicationServices()->workingDatabase()
        ->getSymbolTable(pTextStyleTable, AcDb::kForWrite);
    AcDbTextStyleTableRecord *pTextStyleTableRecord =
        new AcDbTextStyleTableRecord;
    pTextStyleTableRecord->setName("��е��ͼ1");
//        pTextStyleTableRecord->setFont("����",0,0,134,2);
    pTextStyleTableRecord->setFont("����_GB2312",FALSE,FALSE,134,49);
    pTextStyleTableRecord->setXScale(0.67);
        AcDbObjectId Id;
        pTextStyleTable->add(Id,pTextStyleTableRecord);
    pTextStyleTable->close();
    pTextStyleTableRecord->close();

        return Id;
		
		