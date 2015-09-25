//将一个图层的所有对象转移到另一个图层，并删除原图层。

AcDbObjectId eId;
AcDbObject *pObj;
AcDbEntity *pE;ads_name ss;
ads_name ent;
ads_point pt;
long len;
char * layer_tc1;
char * layer_tc2;
struct resbuf eb;
struct resbuf blc;  
int rc = acedEntSel("\n选择被合并图层实体:",ent,pt);
switch(rc)
{
  case RTERROR :
    acutPrintf("\n未选中实体! ");
  break;  case RTCAN :
    acutPrintf("\n取消! ");
  break;  case RTNORM :      
    acdbGetObjectId(eId,ent);
    acdbOpenObject(pE, eId, AcDb::kForRead, false);
    layer_tc1 = pE->layer();//查询实体所在的图层   
    pE->close();    int rcc = acedEntSel("\n选择合并图层实体:",ent,pt);
    switch(rcc)
    {
      case RTERROR :
        acutPrintf("\n未选中实体! ");
      break;      case RTCAN :
        acutPrintf("\n取消! ");
      break;      case RTNORM :            
        acdbGetObjectId(eId,ent);
        acdbOpenObject(pE, eId, AcDb::kForRead, false);
        layer_tc2 = pE->layer();        
        pE->close();        eb.restype = 8;//层名
        eb.resval.rstring = layer_tc1;
        eb.rbnext = NULL;
        acedSSGet("X",NULL,NULL,&eb,ss);
        free(eb.resval.rstring);        acedSSLength(ss,&len);
        for(int i=0; i<len; i++)
        {
          acedSSName(ss,i,ent);
          acdbGetObjectId(eId,ent);
          acdbOpenObject(pE, eId, AcDb::kForWrite, false);
          pE->setLayer(layer_tc2);
          pE->close();   
        }
        acedSSFree(ss);        blc.restype = RTSTR;
        blc.resval.rstring = "0";
        acedSetVar("CLAYER",&blc);//设置当前图层为0层   
        AcDbLayerTable * pLayerTbl;//定义层表指针
        acdbHostApplicationServices()->workingDatabase()->getSymbolTable(pLayerTbl, AcDb::kForWrite);
         
        AcDbLayerTableRecord * pLayerTblRcd; //定义层表记录指针
        if ( Acad::eOk == pLayerTbl->getAt(layer_tc1 , pLayerTblRcd , AcDb::kForWrite))
        {
          pLayerTblRcd->erase(true);//删除图层
        }        
        pLayerTblRcd->close();
        pLayerTbl->close();
        free(layer_tc2);
      break;
    }
    free(layer_tc);      
  break;
}      
