acdbOpenObject(pRec,RecId,AcDb::kForRead);
 if(pRec->hasAttributeDefinitions())
 {
  AcDbBlockTableRecordIterator *pIter;
  pRec->newIterator(pIter);
  AcDbEntity *pEnt;
  for(pIter->start();!pIter->done();pIter->step())
  {
   pIter->getEntity(pEnt,AcDb::kForRead);
   //acdbOpenObject(pEnt,blkDefId,AcDb::kForWrite);
   //检查是否是属性定义
   AcDbAttributeDefinition *pAttDef;
   pAttDef=AcDbAttributeDefinition::cast(pEnt);
   if(pAttDef!=NULL)
   {
    //pAttDef->setTag(Data);
    
    //创建一个新的属性对象
    AcDbAttribute *pAtt=new AcDbAttribute();
    //从属性定义获得属性对象的属性特性；
    pAtt->setPropertiesFrom(pAttDef);
    //设置属性对象的其它特性；
    pAtt->setInvisible(pAttDef->isInvisible());
    AcGePoint3d ptBase=pAttDef->position();
    ptBase+=pBlkRef->position().asVector();
    pAtt->setPosition(pAttDef->position());
    pAtt->setHeight(pAttDef->height());
    pAtt->setRotation(pAttDef->rotation());
    //获得属性对象的Tag、Prompt和TextString;
    char *pStr;
    pStr=pAttDef->tag();
    pAtt->setTag("123456789");

    pAtt->setTextString("Yuan");

    //向块参照追加属性对象;
    pBlkRef->appendAttribute(pAtt);
    pAtt->close();
   }
   pEnt->close();
  }
  delete(pIter);
 }
 pRec->close();