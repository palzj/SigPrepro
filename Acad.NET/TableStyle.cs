1:  /*
2:   * References for AutoCAD 2009:
3:   * - C:\ObjectARX 2009\inc-win32\AcDbMgd.dll; (Copy Local = "False")
4:   * - C:\ObjectARX 2009\inc-win32\AcMgd.dll; (Copy Local = "False")
5:   * - AutoCAD 2009 Type Library ("Reference Managed" dialog box, "COM" tab)
6:   * - AutoCAD/ObjectDBX Common 17.0 Type Library ("Reference Manager" dialog box, "COM" tab)
7:   * 
8:   * References for AutoCAD 2013:
9:   * - C:\ObjectARX 2013\inc\AcCoreMgd.dll; (Copy Local = "False")
10:   * - C:\ObjectARX 2013\inc\AcDbMgd.dll; (Copy Local = "False")
11:   * - C:\ObjectARX 2013\inc\AcMgd.dll; (Copy Local = "False")
12:   * - AutoCAD 2013 Type Library ("Reference Manager" dialog box, "COM" tab)
13:   * - AutoCAD/ObjectDBX Common 19.0 Type Library ("Reference Manager" dialog box, "COM" tab)
14:   */
15:   
16:  // AutoCAD 2009 and the newer AutoCAD versions has different .NET API for 
17:  // working with Tables, and Table Styles.
18:   
19:  // When you'll want compile this code for AutoCAD 2009, 
20:  // you must to uncomment the next "#define Acad2009" row:
21:   
22:  //#define Acad2009
23:   
24:  using System;
25:  using System.Collections.Generic;
26:  using System.Linq;
27:  using System.Text;
28:  //Autodesk namespaces ***************
29:   
30:  #if Acad2009
31:  using AcInt = Autodesk.AutoCAD.Interop;
32:  using AcCom = Autodesk.AutoCAD.Interop.Common;
33:  #else
34:  using AcInt = AutoCAD;
35:  using AcCom = AutoCAD;
36:  #endif
37:   
38:  using acad = Autodesk.AutoCAD.ApplicationServices.Application;
39:  using AcApp = Autodesk.AutoCAD.ApplicationServices;
40:  using AcDb = Autodesk.AutoCAD.DatabaseServices;
41:  using AcEd = Autodesk.AutoCAD.EditorInput;
42:  using AcRtm = Autodesk.AutoCAD.Runtime;
43:  using AcPlt = Autodesk.AutoCAD.PlottingServices;
44:  using AcGSys = Autodesk.AutoCAD.GraphicsSystem;
45:  using AcGem = Autodesk.AutoCAD.Geometry;
46:  using AcCol = Autodesk.AutoCAD.Colors;
47:  //************************************
48:   
49:  [assembly: AcRtm.CommandClass(typeof(AndreyBushman.AutoCAD.Samples.Commands))]
50:   
51:  namespace AndreyBushman.AutoCAD.Samples {
52:   
53:      public class Commands {
54:          [AcRtm.CommandMethod("test")]
55:          public void Test() {
56:   
57:              AcApp.Document doc = acad.DocumentManager.MdiActiveDocument;
58:              AcDb.Database db = doc.Database;
59:              AcEd.Editor ed = doc.Editor;
60:              AcInt.AcadDocument activeDocument = default(AcInt.AcadDocument);
61:  #if Acad2009
62:              activeDocument = (AcInt.AcadDocument)acad.DocumentManager
63:              .MdiActiveDocument.AcadDocument;
64:  #else
65:              activeDocument = (AcInt.AcadDocument)AcApp.DocumentExtension
66:                  .GetAcadDocument(acad.DocumentManager.MdiActiveDocument);
67:  #endif
68:   
69:              AcCom.AcadDatabase _db = activeDocument.Database;
70:              AcCom.AcadDictionaries dictionaries = _db.Dictionaries;
71:              AcCom.AcadDictionary dictObj = (AcCom.AcadDictionary)dictionaries
72:                  .Item("acad_tablestyle");
73:   
74:              // Load 'ACAD_ISO07W100' linetype from 'acadiso.lin' file. I will use it 
75:              // on some cell style on this example.
76:              const string filename = "acadiso.lin";
77:              String lineTypeName = "ACAD_ISO07W100";
78:              try {
79:                  string path = AcDb.HostApplicationServices.Current.FindFile(filename, db,
80:                      AcDb.FindFileHint.Default);
81:                  db.LoadLineTypeFile(lineTypeName, path);
82:              }
83:              catch (AcRtm.Exception ex) {
84:                  ed.WriteMessage("\nException: {0}", ex.Message);
85:              }
86:   
87:              using (AcDb.Transaction tr = db.TransactionManager.StartTransaction()) {
88:   
89:                  // Create new text style. I will use it on some cell style on this example.
90:                  String textStyleName = "Some Text Style";
91:                  AcDb.TextStyleTable tst = tr.GetObject(db.TextStyleTableId,
92:                      AcDb.OpenMode.ForWrite)
93:                      as AcDb.TextStyleTable;
94:                  AcDb.ObjectId textStyleId = AcDb.ObjectId.Null;
95:   
96:                  if (tst.Has(textStyleName)) {
97:                      textStyleId = tst[textStyleName];
98:                  }
99:                  else {
100:                      AcDb.TextStyleTableRecord textStyle = new AcDb.TextStyleTableRecord();
101:                      textStyle.Name = textStyleName;
102:                      tst.Add(textStyle);
103:                      tr.AddNewlyCreatedDBObject(textStyle, true);
104:                      textStyleId = textStyle.ObjectId;
105:                  }
106:   
107:                  AcDb.DBDictionary tableStylesDictionary = tr.GetObject(
108:                      db.TableStyleDictionaryId, AcDb.OpenMode.ForRead) as AcDb.DBDictionary;
109:   
110:                  String tableStyleName = "First Table Style";
111:   
112:                  AcDb.TableStyle tableStyle;
113:                  AcDb.ObjectId tableStyleId = AcDb.ObjectId.Null;
114:   
115:                  // If table style exists, then open it.
116:                  if (tableStylesDictionary.Contains(tableStyleName)) {
117:                      tableStyleId = tableStylesDictionary.GetAt(tableStyleName);
118:                      tableStyle = tr.GetObject(tableStyleId, AcDb.OpenMode.ForWrite)
119:                          as AcDb.TableStyle;
120:                  }
121:                  // If necessary table style is absent, then create new table style
122:                  else {
123:                      tableStyle = new AcDb.TableStyle();
124:                      tableStylesDictionary.UpgradeOpen();
125:                      tableStyleId = tableStylesDictionary.SetAt(tableStyleName, tableStyle);
126:                      tr.AddNewlyCreatedDBObject(tableStyle, true);
127:                      tableStylesDictionary.DowngradeOpen();
128:                  }
129:   
130:                  // Some operations I will do via COM, because I don't know 
131:                  // how to do it through .NET API.
132:  #if Acad2009
133:                  AcCom.IAcadTableStyle2 customTableStyle = (AcCom.IAcadTableStyle2)tableStyle.AcadObject;
134:  #else
135:                  AcCom.IAcadTableStyle customTableStyle = (AcCom.IAcadTableStyle)tableStyle.AcadObject;
136:  #endif
137:                  // Table Style created. Now I must to set all settings for my new table style...    
138:   
139:                  // Table direction
140:                  // this is works fine, but it is COM:
141:                  customTableStyle.FlowDirection = AcCom.AcTableDirection.acTableTopToBottom;
142:   
143:                  // Get existing cell style names
144:                  String[] cellStyleNames = tableStyle.CellStyles.Cast<String>().ToArray();
145:   
146:                  // Description for table style
147:                  tableStyle.Description = "My new Table Style description.";
148:   
149:                  // The most of table style settings is saved at the cell styles.
150:                  // Each of cell style contains some settings, located on tabs 'General', 
151:                  // 'Text', and 'Borders' on "Modify Table Style" dialog window. 
152:                  // Let's set them everything...
153:   
154:                  // Names for new cell styles
155:                  String cellStyle_1 = "My First Cell Style";
156:                  String cellStyle_2 = "My Second Cell Style";
157:                  String cellStyle_3 = "My Third Cell Style";
158:   
159:                  // Create new cell styles
160:                  // Now I do it via COM, but I don't know how to do it through .NET API. 
161:                  customTableStyle.CreateCellStyle(cellStyle_1);
162:                  customTableStyle.CreateCellStyle(cellStyle_2);
163:                  customTableStyle.CreateCellStyle(cellStyle_3);
164:   
165:                  // Rename cell style
166:                  customTableStyle.RenameCellStyle(cellStyle_1, cellStyle_1 += "_X");
167:   
168:                  // Delete cell style
169:                  customTableStyle.DeleteCellStyle(cellStyle_2);
170:   
171:                  //*************************************************************************
172:   
173:                  // 1. 'General' tab settings of "Modify table style" window:
174:   
175:                  // Set Background Color througt COM                
176:                  String strColor = String.Format("Autocad.AcCmColor.{0}", acad.Version.Major);
177:                  AcCom.AcadAcCmColor color = ((AcInt.AcadApplication)acad.AcadApplication)
178:                      .GetInterfaceObject(strColor) as AcCom.AcadAcCmColor;
179:                  color.SetRGB(50, 150, 250);
180:                  customTableStyle.SetBackgroundColor2(cellStyle_1, color);
181:                  // Get Background Color througt COM
182:                  color = customTableStyle.GetBackgroundColor2(cellStyle_1);
183:   
184:                  // Set Background Color througt .NET API
185:                  tableStyle.SetBackgroundColor(AcCol.Color.FromColorIndex(
186:                      AcCol.ColorMethod.Foreground, 31), (Int32)AcDb.RowType.DataRow);
187:                  // Get Background Color througt .NET API
188:                  AcCol.Color dataBgColor = tableStyle.BackgroundColor(AcDb.RowType.DataRow);
189:   
190:                  // Set alignment for cell style througt COM                
191:                  customTableStyle.SetAlignment2(cellStyle_1, AcCom.AcCellAlignment.acBottomRight);
192:                  // Get alignment for cell style througt COM
193:                  AcCom.AcCellAlignment alignment = customTableStyle.GetAlignment2(cellStyle_1);
194:   
195:                  // Set alignment for cell style througt .NET API
196:                  tableStyle.SetAlignment(AcDb.CellAlignment.TopLeft, (Int32)AcDb.RowType.DataRow);
197:                  // Get alignment for cell style througt .NET API
198:                  AcDb.CellAlignment _datAlignment = tableStyle.Alignment(AcDb.RowType.DataRow);
199:   
200:                  // 'Format' option...
201:                  // If I will click the "..." button in the "Modify table style" window, then 
202:                  // will opened "Table Cell Format" window.
203:                  // The 'format' value is a string. The simplest method to get a string value 
204:                  // of the necessary format is such:
205:                  // 1. Set necessary format manually in the "Table Cell Format" window.
206:                  // 2. Get string value of this format via next two code rows:                
207:                  String _format = null;
208:                  // Set cell format througt COM
209:                  customTableStyle.SetFormat2(cellStyle_1, "%au0%pr3");
210:                  // Get cell format througt COM
211:                  customTableStyle.GetFormat2(cellStyle_1, out _format);
212:   
213:                  // Set cell format througt .NET API
214:                  tableStyle.SetFormat("%lu2%pr3%ps[,%]", AcDb.RowType.DataRow);
215:                  // Get cell format througt .NET API
216:                  String datFormat = tableStyle.Format(AcDb.RowType.DataRow);
217:   
218:                  // Set data type, and unit type througt COM
219:                  customTableStyle.SetDataType2(cellStyle_1, AcCom.AcValueDataType.acDouble,
220:                      AcCom.AcValueUnitType.acUnitArea);
221:                  // Get data type, and unit type througt COM
222:                  AcCom.AcValueDataType valueDataType;
223:                  AcCom.AcValueUnitType unitType;
224:                  customTableStyle.GetDataType2(cellStyle_1, out valueDataType, out unitType);
225:   
226:                  // Set datatype, and unit type througt .NET API
227:                  tableStyle.SetDataType(AcDb.DataType.Double, AcDb.UnitType.Distance,
228:                      AcDb.RowType.DataRow);
229:                  // Get datatype, and unit type througt .NET API
230:                  AcDb.DataType _dataType = tableStyle.DataType(AcDb.RowType.DataRow);
231:                  AcDb.UnitType _unitType = tableStyle.UnitType(AcDb.RowType.DataRow);
232:   
233:                  // Set 'horizontal/vertical margins' options
234:  #if Acad2009
235:                  // Acad2009: I don't know how to Get/Set Margin for each of cell styles on 
236:                  // AutoCAD 2009 (through .NET API, COM).
237:  #else
238:                  tableStyle.SetMargin(AcDb.CellMargins.Left, 3.0, cellStyle_1);
239:                  tableStyle.SetMargin(AcDb.CellMargins.Top, 4.0, cellStyle_1);
240:   
241:                  // Get 'horizontal/vertical margins' options
242:                  Double _leftMargin = tableStyle.Margin(AcDb.CellMargins.Left, cellStyle_1);
243:                  Double _topMargin = tableStyle.Margin(AcDb.CellMargins.Top, cellStyle_1);
244:  #endif
245:                  // Get the 'Merge cells on row/column creation' option througt COM
246:                  Boolean isMergedCells = customTableStyle.GetIsMergeAllEnabled(cellStyle_1);
247:                  // Set the 'Merge cells on row/column creation' option througt COM
248:                  customTableStyle.EnableMergeAll(cellStyle_1, true);
249:   
250:                  //******************************************************************************
251:   
252:                  //2. 'Text' tab of "Modify table style" window:
253:   
254:                  // Set the text style for cell style througt COM
255:  #if Acad2009
256:                  customTableStyle.SetTextStyleId(cellStyle_1, textStyleId.OldIdPtr.ToInt32());
257:  #else
258:                  customTableStyle.SetTextStyleId(cellStyle_1, textStyleId.OldIdPtr.ToInt32()); // .ToInt64() for x64.
259:  #endif
260:                  // Get the text style id througt COM
261:                  AcDb.ObjectId _textStyleId = new AcDb.ObjectId(new IntPtr(
262:                      customTableStyle.GetTextStyleId(cellStyle_1)));
263:   
264:                  // Set the text style for cell style througt .NET API
265:                  tableStyle.SetTextStyle(textStyleId, (Int32)AcDb.RowType.DataRow);
266:                  // Get the text style id througt .NET API
267:                  AcDb.ObjectId _dataRowTextStyleId = tableStyle.TextStyle(AcDb.RowType.DataRow);
268:   
269:                  // Set the text height througt COM
270:                  customTableStyle.SetTextHeight2(cellStyle_1, 3.5);
271:                  // Get the text height througt COM
272:                  Double _textHeight = customTableStyle.GetTextHeight2(cellStyle_1);
273:   
274:                  // Set the text height througt .NET API
275:                  tableStyle.SetTextHeight(7.5, (Int32)AcDb.RowType.DataRow);
276:                  // Get the text height througt .NET API
277:                  Double _dataRowTextHeight = tableStyle.TextHeight(AcDb.RowType.DataRow);
278:   
279:                  // Set the text color througt COM
280:                  color.SetRGB(150, 150, 150);
281:                  customTableStyle.SetColor2(cellStyle_1, color);
282:                  // Get the text color througt COM
283:                  AcCom.AcadAcCmColor _textColor = customTableStyle.GetColor2(cellStyle_1);
284:   
285:                  // Set the text color througt .NET API
286:                  Autodesk.AutoCAD.Colors.Color _color = AcCol.Color.FromColorIndex(
287:                      AcCol.ColorMethod.ByAci, 90);
288:                  tableStyle.SetColor(_color, (Int32)AcDb.RowType.DataRow);
289:                  // Get the text color througt .NET API
290:                  AcCol.Color _textColor2 = tableStyle.Color(AcDb.RowType.DataRow);
291:   
292:                  // Set the text angle througt COM                
293:                  Double angle = 45;//degrees
294:                  customTableStyle.SetRotation(cellStyle_1, angle * Math.PI / 180
295:                      /*convert from the degrees to the radians*/);
296:                  // Get the text angle througt COM
297:                  Double _angle = customTableStyle.GetRotation(cellStyle_1) * 180.0 / Math.PI;
298:   
299:                  //**************************************************************************
300:   
301:                  // The 'Borders' tab of "Modify table style" window:
302:   
303:                  // Set lineweight througt COM
304:                  customTableStyle.SetGridLineWeight2(cellStyle_1, AcCom.AcGridLineType.acHorzTop,
305:                      AcCom.ACAD_LWEIGHT.acLnWt050);
306:                  // Get lineweight througt COM
307:                  AcCom.ACAD_LWEIGHT _lineWeight = customTableStyle.GetGridLineWeight2(cellStyle_1,
308:                      AcCom.AcGridLineType.acHorzTop);
309:   
310:                  // Set lineweight througt .NET API
311:                  tableStyle.SetGridLineWeight(AcDb.LineWeight.LineWeight035,
312:                      (Int32)(AcDb.GridLineType.HorizontalInside | AcDb.GridLineType.VerticalLeft),
313:                      (Int32)AcDb.RowType.DataRow);
314:                  // Get lineweight througt .NET API
315:                  AcDb.LineWeight _dataRowLineWeight = tableStyle.GridLineWeight(
316:                      AcDb.GridLineType.HorizontalTop, AcDb.RowType.DataRow);
317:   
318:                  // Set linetype
319:                  AcDb.LinetypeTable linetypeTable = tr.GetObject(db.LinetypeTableId,
320:                      AcDb.OpenMode.ForRead) as AcDb.LinetypeTable;
321:                  if (!linetypeTable.Has(lineTypeName)) {
322:                      ed.WriteMessage("Line type '{0}' not found.", lineTypeName);
323:                      return;
324:                  }
325:                  AcDb.ObjectId linetypeId = linetypeTable[lineTypeName];
326:  #if Acad2009
327:                  // Acad2009: I don't know how to Get/Set GridLinetype for my Cell Style on 
328:                  // AutoCAD 2009 through .NET API, and COM.
329:  #else
330:                  // Set linetype througt .NET API
331:                  tableStyle.SetGridLinetype(linetypeId, AcDb.GridLineType.AllGridLines, "_Data");
332:                  // Get linetype througt .NET API
333:                  AcDb.GridLineStyle _linestyle = tableStyle.GridLineStyle(AcDb.GridLineType.AllGridLines,
334:                      "_Data");
335:  #endif
336:                  // Set grid color througt COM
337:                  color.SetRGB(50, 50, 50);
338:                  customTableStyle.SetGridColor2(cellStyle_1, (AcCom.AcGridLineType.acHorzTop |
339:                      AcCom.AcGridLineType.acVertInside), color);
340:                  // Get grid color througt COM
341:                  AcCom.AcadAcCmColor _acColor = customTableStyle.GetGridColor2(cellStyle_1,
342:                      AcCom.AcGridLineType.acHorzTop);
343:   
344:                  // Set grid color througt .NET API
345:                  tableStyle.SetGridColor(_color, (Int32)(AcDb.GridLineType.HorizontalInside |
346:                      AcDb.GridLineType.VerticalLeft),
347:                      (Int32)AcDb.RowType.DataRow);
348:                  // Get grid color througt .NET API
349:                  tableStyle.GridColor((AcDb.GridLineType.HorizontalInside | AcDb.GridLineType.VerticalLeft),
350:                      AcDb.RowType.DataRow);
351:   
352:                  // Set 'Double line' option
353:  #if Acad2009
354:                  // Acad2009: I don't know how to Get/Set GridLineStyle for my Cell Style on AutoCAD 2009 
355:                  // through .NET API, and COM.
356:                  // Acad2009: I don't know how to Get/Set GridDoubleLineSpacing for my Cell Style on 
357:                  // AutoCAD 2009 through .NET API, and COM.                
358:  #else
359:                  // Set 'Double line' option througt .NET API
360:                  tableStyle.SetGridLineStyle(AcDb.GridLineStyle.Double, AcDb.GridLineType.AllGridLines,
361:                      "_Data");
362:                  // Get 'Double line' option througt .NET API
363:                  AcDb.GridLineStyle _dataGridLinetypeStyle = tableStyle.GridLineStyle(
364:                      AcDb.GridLineType.AllGridLines, "_Data");
365:   
366:                  // Set 'Spacing' option througt .NET API
367:                  tableStyle.SetGridDoubleLineSpacing(3, AcDb.GridLineType.AllGridLines, "_Data");
368:                  // Get 'Spacing' option througt .NET API
369:                  Double _spacing = tableStyle.GridDoubleLineSpacing(AcDb.GridLineType.AllGridLines,
370:                      "_Data");
371:  #endif
372:   
373:                  //*********************************************************************************
374:   
375:                  // Create the clone of table style
376:                  AcDb.TableStyle newTableStyle = (AcDb.TableStyle)tableStyle.Clone();
377:                  tableStylesDictionary.SetAt("myTableStyle Clone", newTableStyle);
378:                  // Add the new Table style to the transaction
379:                  tr.AddNewlyCreatedDBObject(newTableStyle, true);
380:   
381:                  //**********************************************************************************
382:   
383:                  // Create Table Style as via the button named as "select table to start from". 
384:   
385:                  // Step 1: create new Table instance
386:   
387:                  AcDb.BlockTable bt = tr.GetObject(db.BlockTableId, AcDb.OpenMode.ForRead)
388:                      as AcDb.BlockTable;
389:                  AcDb.BlockTableRecord modelSpace = tr.GetObject(bt[AcDb.BlockTableRecord.ModelSpace],
390:                      AcDb.OpenMode.ForWrite) as AcDb.BlockTableRecord;
391:                  AcDb.Table table = new AcDb.Table();
392:                  table.TableStyle = tableStyleId;
393:                  table.Position = new AcGem.Point3d(0, 0, 0);
394:                  table.SetSize(5, 3);
395:                  table.Height = 15;
396:                  table.Width = 20;
397:                  table.ColorIndex = 5;
398:   
399:                  // Fill the table...
400:                  String[,] str = new string[5, 3];
401:                  str[0, 0] = "Part No.";
402:                  str[0, 1] = "Name ";
403:                  str[0, 2] = "Material ";
404:                  str[1, 0] = "1876-1";
405:                  str[1, 1] = "Flange";
406:                  str[1, 2] = "Perspex";
407:                  str[2, 0] = "0985-4";
408:                  str[2, 1] = "Bolt";
409:                  str[2, 2] = "Steel";
410:                  str[3, 0] = "3476-K";
411:                  str[3, 1] = "Tile";
412:                  str[3, 2] = "Ceramic";
413:                  str[4, 0] = "8734-3";
414:                  str[4, 1] = "Kean";
415:                  str[4, 2] = "Mostly water";
416:   
417:                  for (int i = 0; i < 5; i++) {
418:                      for (int j = 0; j < 3; j++) {
419:  #if Acad2009
420:                          table.SetColumnWidth(j, 60);
421:                          table.SetTextHeight(i, j, 5);
422:                          table.SetTextString(i, j, str[i, j]);
423:                          table.SetAlignment(i, j, AcDb.CellAlignment.MiddleCenter);
424:  #else
425:                          table.Cells[i, j].TextHeight = 5;
426:                          table.Columns[j].Width = 60;
427:                          table.Cells[i, j].SetValue(str[i, j], AcDb.ParseOption.ParseOptionNone);
428:                          table.Cells[i, j].Alignment = AcDb.CellAlignment.MiddleCenter;
429:  #endif
430:                      }
431:  #if Acad2009
432:                      table.SetRowHeight(i, 10.0);
433:  #else
434:                      table.Rows[i].Height = 10;
435:  #endif
436:                  }
437:   
438:  #if Acad2009
439:                  AcDb.CellRange rng = new AcDb.CellRange(2, 0, 3, 1);
440:  #else
441:                  AcDb.CellRange rng = AcDb.CellRange.Create(table, 2, 0, 3, 1);
442:  #endif
443:                  table.MergeCells(rng);
444:                  table.GenerateLayout();
445:                  modelSpace.AppendEntity(table);
446:                  tr.AddNewlyCreatedDBObject(table, true);
447:   
448:                  // Step 2: Apply this table as template for table style
449:   
450:                  AcDb.TableTemplate template = new AcDb.TableTemplate(
451:                      table, AcDb.TableCopyOptions.TableCopyColumnWidth |
452:                      AcDb.TableCopyOptions.TableCopyRowHeight |
453:                      AcDb.TableCopyOptions.ConvertFormatToOverrides);
454:                  db.AddDBObject(template);
455:                  tr.AddNewlyCreatedDBObject(template, true);
456:  #if Acad2009
457:                  customTableStyle.TemplateId = template.ObjectId.OldIdPtr.ToInt32();
458:  #else
459:                  customTableStyle.TemplateId = template.ObjectId.OldIdPtr.ToInt32(); // .ToInt64() for x64.
460:  #endif
461:                  // Now create Table instance, which will use this TableStyle.
462:                  AcDb.Table tableInstance = new AcDb.Table();
463:                  tableInstance.SetDatabaseDefaults();
464:                  tableInstance.TableStyle = tableStyle.Id;
465:                  tableInstance.CopyFrom(template, AcDb.TableCopyOptions.None);
466:                  tableInstance.GenerateLayout();
467:                  tableInstance.SetSize(10, 5);
468:                  tableInstance.Position = new AcGem.Point3d(0, 500, 0);
469:                  modelSpace.AppendEntity(tableInstance);
470:                  tr.AddNewlyCreatedDBObject(tableInstance, true);
471:   
472:                  tr.Commit();
473:              }
474:          }
475:      }
476:  }