#region ³ÌÐò¼¯ acdbmgd.dll, v17.1.0.0
// C:\Program Files (x86)\AutoCAD 2008\acdbmgd.dll
#endregion

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.ComponentModel;

namespace Autodesk.AutoCAD.DatabaseServices
{
    [TypeDescriptionProvider("Autodesk.AutoCAD.ComponentModel.TypeDescriptionProvider`1[[Autodesk.AutoCAD.DatabaseServices.Table, acdbmgd]], acdbmgd")]
    [Wrapper("AcDbTable")]
    public class Table : BlockReference, IEnumerable
    {
        public Table();
        protected internal Table(IntPtr unmanagedPointer, bool autoDelete);

        public virtual bool BreakEnabled { get; set; }
        public virtual TableBreakFlowDirection BreakFlowDirection { get; set; }
        public virtual TableBreakOptions BreakOptions { get; set; }
        [Category("Geometry")]
        public virtual Vector3d Direction { get; set; }
        [Category("Table")]
        public virtual FlowDirection FlowDirection { get; set; }
        [Category("Table")]
        public virtual bool HasSubSelection { get; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double Height { get; set; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double HorizontalCellMargin { get; set; }
        [Category("Table")]
        public virtual bool IsHeaderSuppressed { get; set; }
        [Category("Table")]
        public virtual bool IsTitleSuppressed { get; set; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double MinimumTableHeight { get; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double MinimumTableWidth { get; }
        [Category("Table")]
        public virtual int NumColumns { get; set; }
        [Category("Table")]
        public virtual int NumRows { get; set; }
        [Category("Table")]
        public virtual TableRegion SubSelection { get; set; }
        [Category("Table")]
        public virtual ObjectId TableStyle { get; set; }
        [Category("Table")]
        public string TableStyleName { get; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double VerticalCellMargin { get; set; }
        [Category("Table")]
        [UnitType(UnitType.Distance)]
        public virtual double Width { get; set; }

        public virtual CellAlignment Alignment(RowType type);
        public virtual CellAlignment Alignment(int row, int col);
        public virtual Point3d AttachmentPoint(int row, int col);
        public virtual Color BackgroundColor(RowType type);
        public virtual Color BackgroundColor(int row, int col);
        public virtual double BlockRotation(int row, int col);
        public virtual double BlockScale(int row, int col);
        public virtual ObjectId BlockTableRecordId(int row, int col);
        public virtual bool CanDeleteColumns(int index, int nCount);
        public virtual bool CanDeleteRows(int index, int nCount);
        public virtual bool CanInsertColumn(int index);
        public virtual bool CanInsertRow(int index);
        public virtual TableStyleOverride[] CellStyleOverrides(int row, int col);
        public virtual CellType CellType(int row, int col);
        public virtual void ClearSubSelection();
        public virtual void ClearTableStyleOverrides(int options);
        public virtual double ColumnWidth(int col);
        public virtual Color ContentColor(RowType type);
        public virtual Color ContentColor(int row, int col);
        public virtual void CopyFrom(LinkedTableData source, TableCopyOptions options);
        public virtual CellRange CopyFrom(LinkedTableData source, TableCopyOptions options, CellRange sourceRange, CellRange targetRange);
        public virtual CellRange CopyFrom(Table source, TableCopyOptions options, CellRange sourceRange, CellRange targetRange);
        public virtual int CreateContent(int row, int column, int contentIndex);
        public virtual DataType DataType(RowType type);
        public virtual DataType DataType(int row, int col);
        public virtual void DeleteCellContent(int row, int col);
        public virtual void DeleteColumns(int col, int columns);
        public virtual void DeleteContent(CellRange value);
        public virtual void DeleteContent(int row, int column);
        public virtual void DeleteContent(int row, int column, int contentIndex);
        public virtual void DeleteRows(int row, int rows);
        public virtual ObjectId FieldId(int row, int col);
        public virtual void Fill(CellRange fillRange, CellRange sourceRange, TableFillOptions options);
        public virtual string Format(RowType type);
        public virtual string Format(int row, int col);
        public virtual void GenerateLayout();
        public virtual string GetBlockAttributeValue(int row, int col, ObjectId attributeDefinitionId);
        public virtual string GetBlockAttributeValue(int row, int column, int contentIndex, ObjectId attDefId);
        public virtual ObjectId GetBlockTableRecordId(int row, int column, int contentIndex);
        public virtual double GetBreakHeight(int index);
        public virtual Vector3d GetBreakOffset(int index);
        public virtual double GetBreakSpacing();
        public virtual void GetCellExtents(int row, int col, bool isOuterCell, Point3dCollection pts);
        public virtual CellStates GetCellState(int row, int column);
        public virtual string GetCellStyle(int row, int column);
        public virtual string GetColumnName(int index);
        public virtual Color GetContentColor(int row, int column, int contentIndex);
        public virtual CellContentLayout GetContentLayout(int row, int column);
        public virtual CellContentTypes GetContentTypes(int row, int column);
        public virtual CellContentTypes GetContentTypes(int row, int column, int contentIndex);
        public virtual int GetCustomData(int row, int column);
        public virtual object GetCustomData(int row, int column, string key);
        public virtual string GetDataFormat(int row, int column);
        public virtual string GetDataFormat(int row, int column, int contentIndex);
        public virtual ObjectIdCollection GetDataLink();
        public virtual ObjectIdCollection GetDataLink(CellRange range);
        public virtual ObjectId GetDataLink(int row, int column);
        public virtual CellRange GetDataLinkRange(int row, int column);
        public virtual DataTypeParameter GetDataType(int row, int column, int contentIndex);
        public virtual TableEnumerator GetEnumerator();
        public virtual TableEnumerator GetEnumerator(CellRange range, TableEnumeratorOption option);
        public virtual ObjectId GetFieldId(int row, int column, int contentIndex);
        public virtual string GetFormula(int row, int column, int contentIndex);
        public virtual Color GetGridColor(int row, int column, GridLineType gridLineType);
        public virtual double GetGridDoubleLineSpacing(int row, int column, GridLineType gridLineType);
        public virtual GridLineStyle GetGridLineStyle(int row, int column, GridLineType gridLineType);
        public virtual ObjectId GetGridLinetype(int row, int column, GridLineType gridLineType);
        public virtual LineWeight GetGridLineWeight(int row, int column, GridLineType gridLineType);
        public virtual GridPropertyParameter GetGridProperty(int row, int column, GridLineType gridLineType);
        public virtual Visibility GetGridVisibility(int row, int column, GridLineType gridLineType);
        public virtual bool GetIsAutoScale(int row, int column, int contentIndex);
        public virtual double GetMargin(int row, int column, CellMargins margin);
        public virtual bool GetMergeAllEnabled(int row, int column);
        public virtual CellRange GetMergeRange(int row, int column);
        public virtual int GetNumberOfContents(int row, int column);
        public virtual CellProperties GetOverrides(int row, int column, GridLineType gridLineType);
        public virtual CellProperties GetOverrides(int row, int column, int contentIndex);
        public virtual double GetRotation(int row, int column, int contentIndex);
        public virtual double GetScale(int row, int column, int contentIndex);
        public virtual double GetTextHeight(int row, int column, int contentIndex);
        public virtual string GetTextString(int row, int column, int contentIndex);
        public virtual string GetTextString(int row, int column, int contentIndex, FormatOption formatOption);
        public virtual ObjectId GetTextStyleId(int row, int column, int contentIndex);
        public virtual string GetToolTip(int row, int column);
        public virtual object GetValue(int row, int column, int contentIndex);
        public virtual object GetValue(int row, int column, int contentIndex, FormatOption formatOption);
        public virtual Color GridColor(GridLineType gridlineType, RowType type);
        public virtual Color GridColor(int row, int col, CellEdgeMasks edge);
        public virtual LineWeight GridLineWeight(GridLineType gridlineType, RowType type);
        public virtual LineWeight GridLineWeight(int row, int col, CellEdgeMasks edge);
        public virtual bool GridVisibility(GridLineType gridlineType, RowType type);
        public virtual bool GridVisibility(int row, int col, CellEdgeMasks edge);
        public virtual bool HasFormula(int row, int column, int contentIndex);
        public virtual TableHitTestInfo HitTest(Point3d point, Vector3d viewVector);
        public virtual void InsertColumns(int col, double width, int columns);
        public virtual void InsertColumnsAndInherit(int col, int inheritFrom, int numCols);
        public virtual void InsertRows(int row, double height, int rows);
        public virtual void InsertRowsAndInherit(int index, int inheritFrom, int numRows);
        public virtual bool IsAutoScale(int row, int col);
        public virtual bool IsBackgroundColorNone(RowType type);
        public virtual bool IsBackgroundColorNone(int row, int col);
        public virtual bool IsContentEditable(int row, int column);
        public virtual bool IsEmpty(int row, int column);
        public virtual bool IsFormatEditable(int row, int column);
        public virtual bool IsLinked(int row, int column);
        public virtual TableRegion IsMergedCell(int row, int col);
        public virtual void MergeCells(CellRange range);
        public virtual void MergeCells(TableRegion value);
        public virtual double MinimumColumnWidth(int col);
        public virtual double MinimumRowHeight(int row);
        public virtual void MoveContent(int row, int column, int fromIndex, int toIndex);
        public virtual void RecomputeTableBlock(bool forceUpdate);
        public virtual void RemoveAllOverrides(int row, int column);
        public virtual void RemoveDataLink();
        public virtual void RemoveDataLink(int row, int column);
        public virtual void ReselectSubRegion(FullSubentityPath[] paths);
        public virtual void ResetValue(int row, int col);
        public virtual double RowHeight(int row);
        public virtual RowType RowType(int row);
        public virtual TableHitTestInfo Select(Point3d pickingPoint, Vector3d hitTestViewDirection, Vector3d hitTestViewOrientation, bool allowOutside, bool inPickFirst, FullSubentityPath[] paths);
        public virtual TableRegion SelectSubRegion(Point3d cornerPoint1, Point3d cornerPoint2, Vector3d selectionViewDirection, Vector3d hitTestViewDirection, SelectType selectionType, bool includeCurrentSelection, bool inPickFirst, FullSubentityPath[] paths);
        public virtual void SetAlignment(CellAlignment align, int rowTypes);
        public virtual void SetAlignment(int row, int col, CellAlignment align);
        public virtual void SetAutoScale(int row, int col, bool autoFit);
        public virtual void SetBackgroundColor(Color color, int rowTypes);
        public virtual void SetBackgroundColor(int row, int col, Color color);
        public virtual void SetBackgroundColorNone(bool value, int rowTypes);
        public virtual void SetBackgroundColorNone(int row, int col, bool value);
        public virtual void SetBlockAttributeValue(int row, int col, ObjectId attributeDefinitionId, string value);
        public virtual void SetBlockAttributeValue(int row, int column, int contentIndex, ObjectId attDefId, string value);
        public virtual void SetBlockRotation(int row, int col, double rotationalAngle);
        public virtual void SetBlockScale(int row, int col, double scale);
        public virtual void SetBlockTableRecordId(int row, int col, ObjectId blockId, bool autoFit);
        public virtual void SetBlockTableRecordId(int row, int column, int contentIndex, ObjectId blockId, bool autoFit);
        public virtual void SetBreakHeight(int index, double height);
        public virtual void SetBreakOffset(int index, Vector3d offset);
        public virtual void SetBreakSpacing(double spacing);
        public virtual void SetCellState(int row, int column, CellStates cellState);
        public virtual void SetCellStyle(int row, int column, string styleName);
        public virtual void SetCellType(int row, int col, CellType type);
        public virtual void SetColumnName(int index, string name);
        public virtual void SetColumnWidth(double width);
        public virtual void SetColumnWidth(int col, double width);
        public virtual void SetContentColor(Color color, int rowType);
        public virtual void SetContentColor(int row, int col, Color color);
        public virtual void SetContentColor(int row, int column, int contentIndex, Color color);
        public virtual void SetContentLayout(int row, int column, CellContentLayout layout);
        public virtual void SetCustomData(int row, int column, int data);
        public virtual void SetCustomData(int row, int column, string key, object value);
        public virtual void SetDataFormat(int row, int column, string format);
        public virtual void SetDataFormat(int row, int column, int contentIndex, string format);
        public virtual void SetDataLink(CellRange range, ObjectId dataLinkId, bool bUpdate);
        public virtual void SetDataLink(int row, int column, ObjectId dataLinkId, bool bUpdate);
        public virtual void SetDataType(DataType nDataType, UnitType nUnitType, int rowTypes);
        public virtual void SetDataType(int row, int col, DataType nDataType, UnitType nUnitType);
        public virtual void SetDataType(int row, int column, int contentIndex, DataTypeParameter dataType);
        public virtual void SetFieldId(int row, int col, ObjectId fieldId);
        public virtual void SetFieldId(int row, int column, int contentIndex, ObjectId fieldId, CellOption option);
        public virtual void SetFormat(string pFormat, int rowTypes);
        public virtual void SetFormat(int row, int col, string pFormat);
        public virtual void SetFormula(int row, int column, int contentIndex, string formula);
        public virtual void SetGridColor(Color color, int borders, int rows);
        public virtual void SetGridColor(int row, int column, GridLineType gridLineType, Color color);
        public virtual void SetGridColor(int row, int col, short edges, Color color);
        public virtual void SetGridDoubleLineSpacing(int row, int column, GridLineType gridLineType, double spacing);
        public virtual void SetGridLineStyle(int row, int column, GridLineType gridLineType, GridLineStyle lineStyle);
        public virtual void SetGridLinetype(int row, int column, GridLineType gridLineType, ObjectId linetype);
        public virtual void SetGridLineWeight(LineWeight lineWeight, int borders, int rows);
        public virtual void SetGridLineWeight(int row, int column, GridLineType gridLineType, LineWeight lineWeight);
        public virtual void SetGridLineWeight(int row, int col, short edges, LineWeight value);
        public virtual void SetGridProperty(CellRange rangeIn, GridLineType gridLineTypes, GridPropertyParameter gridProp);
        public virtual void SetGridProperty(int row, int column, GridLineType gridLineType, GridPropertyParameter gridProperty);
        public virtual void SetGridVisibility(bool visible, int borders, int rows);
        public virtual void SetGridVisibility(int row, int column, GridLineType gridLineType, Visibility visibility);
        public virtual void SetGridVisibility(int row, int col, short edges, bool value);
        public virtual void SetIsAutoScale(int row, int column, int contentIndex, bool autoFit);
        public virtual void SetMargin(int row, int column, CellMargins margin, double value);
        public virtual void SetMergeAllEnabled(int row, int column, bool enable);
        public virtual void SetOverrides(int row, int column, GridLineType gridLineType, CellProperties @override);
        public virtual void SetOverrides(int row, int column, int contentIndex, CellProperties @override);
        public virtual void SetRotation(int row, int column, int contentIndex, double angle);
        public virtual void SetRowHeight(double height);
        public virtual void SetRowHeight(int row, double height);
        public virtual void SetScale(int row, int column, int contentIndex, double scale);
        public virtual void SetSize(int numRows, int numCols);
        public virtual void SetTextHeight(double height, int rowTypes);
        public virtual void SetTextHeight(int row, int col, double height);
        public virtual void SetTextHeight(int row, int column, int contentIndex, double height);
        public virtual void SetTextRotation(int row, int col, RotationAngle rot);
        public virtual void SetTextString(int row, int col, string value);
        public virtual void SetTextString(int row, int column, int contentIndex, string text);
        public virtual void SetTextStyle(ObjectId id, int rowTypes);
        public virtual void SetTextStyle(int row, int col, ObjectId id);
        public virtual void SetTextStyleId(int row, int column, int contentIndex, ObjectId id);
        public virtual void SetToolTip(int row, int column, string toolTip);
        public virtual void SetValue(int row, int col, object pValue);
        public virtual void SetValue(int row, int column, int contentIndex, object value);
        public virtual void SetValue(int row, int col, string pText, ParseOption nOption);
        public virtual void SetValue(int row, int column, int contentIndex, object value, ParseOption parseOption);
        public virtual void SetValue(int row, int column, int contentIndex, string value, ParseOption parseOption);
        public virtual TableStyleOverride[] TableStyleOverrides();
        public virtual double TextHeight(RowType type);
        public virtual double TextHeight(int row, int col);
        public virtual RotationAngle TextRotation(int row, int col);
        public virtual string TextString(int row, int col);
        public virtual string TextString(int row, int col, FormatOption nOption);
        public virtual string TextStringConst(int row, int col);
        public virtual ObjectId TextStyle(RowType type);
        public virtual ObjectId TextStyle(int row, int col);
        public virtual UnitType UnitType(RowType type);
        public virtual UnitType UnitType(int row, int col);
        public virtual void UnmergeCells(CellRange range);
        public virtual void UnmergeCells(TableRegion value);
        public virtual void UpdateDataLink(UpdateDirection dir, UpdateOption option);
        public virtual void UpdateDataLink(int row, int column, UpdateDirection dir, UpdateOption option);
        public virtual object Value(int row, int col);
    }
}
