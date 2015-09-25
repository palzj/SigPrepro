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
   //����Ƿ������Զ���
   AcDbAttributeDefinition *pAttDef;
   pAttDef=AcDbAttributeDefinition::cast(pEnt);
   if(pAttDef!=NULL)
   {
    //pAttDef->setTag(Data);
    
    //����һ���µ����Զ���
    AcDbAttribute *pAtt=new AcDbAttribute();
    //�����Զ��������Զ�����������ԣ�
    pAtt->setPropertiesFrom(pAttDef);
    //�������Զ�����������ԣ�
    pAtt->setInvisible(pAttDef->isInvisible());
    AcGePoint3d ptBase=pAttDef->position();
    ptBase+=pBlkRef->position().asVector();
    pAtt->setPosition(pAttDef->position());
    pAtt->setHeight(pAttDef->height());
    pAtt->setRotation(pAttDef->rotation());
    //������Զ����Tag��Prompt��TextString;
    char *pStr;
    pStr=pAttDef->tag();
    pAtt->setTag("123456789");

    pAtt->setTextString("Yuan");

    //������׷�����Զ���;
    pBlkRef->appendAttribute(pAtt);
    pAtt->close();
   }
   pEnt->close();
  }
  delete(pIter);
 }
 pRec->close();