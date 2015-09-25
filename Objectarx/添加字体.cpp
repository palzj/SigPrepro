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
SetFont（）函数，用于将TrueType字型设置为字体样式。
Acad::ErrorStatus setFont(
   const char* pTypeface,
   Adesk::Boolean bold,
   Adesk::Boolean italic,
   int charset,
   int pitchAndFamily) const;
pTypeface        字体名
bold        true为粗体
italic        true为斜体
charset        字符集的值
pitchAndFamily        字距和字符族的值
TrueType常用字体
TrueType字体名        charset        pitchAndFamily
仿宋_GB2312          134        49
宋体                  134        2
黑体                  134        2
隶书               134        49
幼园                  134        49
楷体_GB2312        134        49


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
        pRecord->setFont("仿宋_GB2312",FALSE,FALSE,134,49);
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
    pTextStyleTableRecord->setName("机械制图1");
//        pTextStyleTableRecord->setFont("宋体",0,0,134,2);
    pTextStyleTableRecord->setFont("仿宋_GB2312",FALSE,FALSE,134,49);
    pTextStyleTableRecord->setXScale(0.67);
        AcDbObjectId Id;
        pTextStyleTable->add(Id,pTextStyleTableRecord);
    pTextStyleTable->close();
    pTextStyleTableRecord->close();

        return Id;
		
		