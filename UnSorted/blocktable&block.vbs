    Sub Att2Table()
           On Error Resume Next
           Dim Ent As AcadEntity
           Dim Pnt As Variant
           Do
                   ThisDrawing.Utility.GetEntity Ent, Pnt, vbCrLf & "请选择要提取属性的块："
                   If Err.Number <> 0 Then Exit Sub
                   If Ent.ObjectName = "AcDbBlockReference" Then
                           If Ent.HasAttributes = True Then
                                   Exit Do
                           End If
                   End If
           Loop
           Dim BlkRef As AcadBlockReference
           Set BlkRef = Ent
           Dim BlkName As String
           BlkName = BlkRef.Name
           
           
           Dim SS As AcadSelectionSet
           Set SS = CreatSSet
           Dim FilterType As Variant
           Dim FilterData As Variant
           Dim FType(2) As Integer
           Dim FData(2) As Variant
           FType(0) = 0
           FData(0) = "INSERT" '图元名
           FType(1) = 66
           FData(1) = 1   '带属性
           FType(2) = 2
           FData(2) = BlkName   '图块名
           FilterType = FType
           FilterData = FData
           SS.Select acSelectionSetAll, , , FilterType, FilterData
           Dim i As Integer
           Dim j As Integer
           Dim Blk As AcadBlock
           Dim Att As AcadAttribute
           Dim AttRef As AcadAttributeReference
           Dim AttRefs As Variant
           Dim Rows As Double
           Dim Cols As Double
           Dim Table As AcadTable
           For i = 0 To SS.Count - 1
                   Set BlkRef = SS(i)
                   AttRefs = BlkRef.GetAttributes
                   If i = 0 Then
                           Cols = UBound(AttRefs) + 1
                           Rows = SS.Count
                           Set Table = AddBlkTable(Cols, Rows)
                           Set Blk = ThisDrawing.Blocks(BlkRef.Name)
                           For Each Ent In Blk
                                   If Ent.ObjectName = "AcDbAttributeDefinition" Then
                                           Set Att = Ent
                                           Table.SetText 0, j, Att.PromptString
                                           j = j + 1
                                   End If
                           Next
                   End If
                   For j = 0 To UBound(AttRefs)
                           Set AttRef = AttRefs(j)
                           Table.SetText i + 1, j, AttRef.TextString
                   Next
           Next
    End Sub
    Function AddBlkTable(TableColCount As Double, TableRowCount As Double)
           Dim Table As AcadTable
           Dim InsertionPoint As Variant
           InsertionPoint = ThisDrawing.Utility.GetPoint(, vbCrLf & "请选择表格插入点：")
           Dim RowHeight As Double, Colwidth As Double
           RowHeight = 8: Colwidth = 70 '行高及列宽
           Set Table = ThisDrawing.ModelSpace.AddTable _
                                   (InsertionPoint, TableRowCount + 1, TableColCount, RowHeight, Colwidth)
           Table.HeaderSuppressed = True
           '取消原先表格格式中的首行合并
           Table.UnmergeCells 0, 0, 0, TableColCount - 1 '按顺序为合并的起始行号、结束行号、起始列号、结束列号
           Table.SetTextHeight 7, 7
           'Table.SetAlignment 3, 5
           Set AddBlkTable = Table
           'Debug.Print Table.Rows
    End Function
	
	    Sub Att2Table()
           On Error Resume Next
           Dim Ent As AcadEntity
           Dim Pnt As Variant
           Do
                   ThisDrawing.Utility.GetEntity Ent, Pnt, vbCrLf & "请选择要提取属性的块："
                   If Err.Number <> 0 Then Exit Sub
                   If Ent.ObjectName = "AcDbBlockReference" Then
                           If Ent.HasAttributes = True Then
                                   Exit Do
                           End If
                   End If
           Loop
           Dim BlkRef As AcadBlockReference
           Set BlkRef = Ent
           Dim BlkName As String
           BlkName = BlkRef.Name
           
           
           Dim SS As AcadSelectionSet
           Set SS = CreatSSet
           Dim FilterType As Variant
           Dim FilterData As Variant
           Dim FType(2) As Integer
           Dim FData(2) As Variant
           FType(0) = 0
           FData(0) = "INSERT" '图元名
           FType(1) = 66
           FData(1) = 1   '带属性
           FType(2) = 2
           FData(2) = BlkName   '图块名
           FilterType = FType
           FilterData = FData
           SS.Select acSelectionSetAll, , , FilterType, FilterData
           Dim i As Integer
           Dim j As Integer
           Dim Blk As AcadBlock
           Dim Att As AcadAttribute
           Dim AttRef As AcadAttributeReference
           Dim AttRefs As Variant
           Dim Rows As Double
           Dim Cols As Double
           Dim Table As AcadTable
           For i = 0 To SS.Count - 1
                   Set BlkRef = SS(i)
                   AttRefs = BlkRef.GetAttributes
                   If i = 0 Then
                           Cols = UBound(AttRefs) + 1
                           Rows = SS.Count
                           Set Table = AddBlkTable(Cols, Rows)
                           Set Blk = ThisDrawing.Blocks(BlkRef.Name)
                           For Each Ent In Blk
                                   If Ent.ObjectName = "AcDbAttributeDefinition" Then
                                           Set Att = Ent
                                           Table.SetText 0, j, Att.PromptString
                                           j = j + 1
                                   End If
                           Next
                   End If
                   For j = 0 To UBound(AttRefs)
                           Set AttRef = AttRefs(j)
                           Table.SetText i + 1, j, AttRef.TextString
                   Next
           Next
    End Sub
    Function AddBlkTable(TableColCount As Double, TableRowCount As Double)
           Dim Table As AcadTable
           Dim InsertionPoint As Variant
           InsertionPoint = ThisDrawing.Utility.GetPoint(, vbCrLf & "请选择表格插入点：")
           Dim RowHeight As Double, Colwidth As Double
           RowHeight = 8: Colwidth = 70 '行高及列宽
           Set Table = ThisDrawing.ModelSpace.AddTable _
                                   (InsertionPoint, TableRowCount + 1, TableColCount, RowHeight, Colwidth)
           Table.HeaderSuppressed = True
           '取消原先表格格式中的首行合并
           Table.UnmergeCells 0, 0, 0, TableColCount - 1 '按顺序为合并的起始行号、结束行号、起始列号、结束列号
           Table.SetTextHeight 7, 7
           'Table.SetAlignment 3, 5
           Set AddBlkTable = Table
           'Debug.Print Table.Rows
    End Function