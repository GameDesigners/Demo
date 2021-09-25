using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Data;
using System.Text;
using System.IO;
using System;

/// <summary>
/// Sheet表格
/// </summary>
public class Sheet
{
    private List<List<string>> sheetData;
    private List<int> primaryKeys;

    public List<string> ColumnTitles()
    {
        if (sheetData.Count > 0)
            return sheetData[0];
        else
            return default;
    }
    public List<List<string>> SheetData() => sheetData;
    public int GetRowCount() => sheetData.Count;
    public int GetColumnCount()
    {
        if (sheetData.Count > 0)
            return sheetData[0].Count;
        else
            return 0;
    }
    public int GetRowCountWithoutTitle() => GetRowCount() - 1;
    public int GetColumnCountWithoutTitle() => GetColumnCount() - 1;

    public string this[int row,int col]
    {
        get
        {
            if (sheetData.Count > 0)
            {
                if (row < sheetData.Count && col < sheetData[0].Count)
                {
                    return sheetData[row][col];
                }
                return default;
            }
            return default;
        }
        set
        {
            if (sheetData.Count > 0)
            {
                if (row < sheetData.Count && col <= sheetData[0].Count)
                {
                    sheetData[row][col] = value;
                }
            }
        }
    }

    public List<string> this[int row]
    {
        get
        {
            if (row >= 0)
                if (row < sheetData.Count)
                    return sheetData[row];
            return default;
        }
        set
        {
            if (sheetData[0].Count != value.Count)
                return;
            if (row >= 0)
                if (row < sheetData.Count)
                    sheetData[row] = value;
        }
    }

    public Sheet()
    {
        sheetData = new List<List<string>>();
        primaryKeys = new List<int>();
    }

    public Sheet(List<string> titles)
    {
        sheetData = new List<List<string>>();
        primaryKeys = new List<int>();
    }

    public bool AddPrimaryKey(int index)
    {
        if (index >= sheetData[0].Count)
            return false;
        if (!primaryKeys.Contains(index))
            primaryKeys.Add(index);
        return true;
    }

    public bool AddPrimaryKey(string title)
    {
        if (sheetData.Count > 0)
        {
            if (!sheetData[0].Contains(title))
                return false;
            int index = sheetData[0].IndexOf(title);
            if (!primaryKeys.Contains(index))
                primaryKeys.Add(index);
            return true;
        }
        else
            return false;
    }

    public bool AddTtile(List<string> titles,List<Type> types=default)
    {
        if (sheetData.Count == 0)
            sheetData.Add(titles);
        else
            sheetData[0] = titles;
        return true;
    }

    public bool AddColumn(string title)
    {
        if(sheetData.Count>0)
        {
            if (sheetData[0].Contains(title))
                return false;
            return true;
        }
        else
        {
            sheetData.Add(new List<string> { title });
            return true;
        }
    }

    public bool RemoveColume(string title)
    {
        if (sheetData.Count > 0)
        {
            if (sheetData[0].Contains(title))
            {
                int index = sheetData[0].IndexOf(title);
                for (int i = 0; i < sheetData.Count; i++)
                    sheetData.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    public bool AddRow(List<string> row)
    {
        if(sheetData.Count>0)
        {
            if (row.Count != GetColumnCount())
            {
                GDebug.Instance.Error($"添加数据错误：当前表格列数：{GetColumnCount()}  添加Row的列数:{row.Count}");
                return false;
            }
        }
        else
        {
            GDebug.Instance.Warn("当前表格为添加表头就填充数据。");
        }

        foreach(var r in sheetData)
        {
            foreach(var key in primaryKeys)
            {
                if (r[key] == row[key])
                    return false;
            }
        }

        sheetData.Add(row);
        return true;
    }

    public bool RemoveRow(int rowIndex)
    {
        if (sheetData.Count <= rowIndex)
            return false;
        sheetData.RemoveAt(rowIndex);
        return true;
    }

    public void Clear()
    {
        sheetData.Clear();
        primaryKeys.Clear();
    }
}

public class ExcelData
{
    /// <summary>
    /// excel_data_map:
    /// Key:表格的名称
    /// value:二维数组
    /// </summary>
    private Dictionary<string, Sheet> excel_data_map;

    public ExcelData()
    {
        excel_data_map = new Dictionary<string, Sheet>();
    }

    public Dictionary<string,Sheet> GetData() =>excel_data_map;

    public bool Add(string sheetName, Sheet sheet)
    {
        if(excel_data_map.ContainsKey(sheetName))
        {
            GDebug.Instance.Error($"ExcelData中：表格{sheetName}已经存在");
            return false;
        }    
        excel_data_map.Add(sheetName, sheet);
        return true;
    }

    public bool Remove(string sheetName)
    {
        if (!excel_data_map.ContainsKey(sheetName))
        {
            GDebug.Instance.Error($"ExcelData中：表格{sheetName}不存在存在");
            return false;
        }
        excel_data_map.Remove(sheetName);
        return true;
    }

    public bool ContainsKey(string sheetName) => excel_data_map.ContainsKey(sheetName);

    public Sheet this[string sheetName]
    {
        get
        {
            if (!excel_data_map.ContainsKey(sheetName))
            {
                GDebug.Instance.Error($"ExcelData中：表格{sheetName}不存在存在");
                return default;
            }
            return excel_data_map[sheetName];
        }
        set
        {
            if (!excel_data_map.ContainsKey(sheetName))
                GDebug.Instance.Error($"ExcelData中：表格{sheetName}不存在存在");
            excel_data_map[sheetName] = value;
        }
    }
}

public class ExcelUtil
{
    public static ExcelData Read(string filepath, int titleIndex = 1)
    {
        if (File.Exists(filepath))
        {
            try
            {
                ExcelData ed = new ExcelData();
                FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {

                        if (worksheet != null)
                        {
                            Sheet dt = new Sheet();
                            int rowCount = worksheet.Dimension.Rows;
                            int colCount = worksheet.Dimension.Columns;
                            for (int row = titleIndex; row <= rowCount; row++)
                            {
                                List<string> col_list = new List<string>();
                                for (int col = 1; col <= colCount; col++)
                                    col_list.Add(worksheet.Cells[row, col].Value.ToString());
                                if (row == 1)
                                    dt.AddTtile(col_list);
                                else
                                    dt.AddRow(col_list);
                            }
                            ed.Add(worksheet.Name, dt);
                        }
                        else
                            GDebug.Instance.Error($"当前获取的WorkSheet为空...");
                    }
                    fs.Close();
                    return ed;
                }
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error($"写入失败！【很有可能是当前文件处于打开状态而无法写入】\n{ex}");
            }
        }
        else if (Path.GetExtension(filepath) != ".xlsx")
            GDebug.Instance.Error($"当前文件格式不合法[非.xlsx] path:{filepath}");
        else
            GDebug.Instance.Error($"读取Excel文件为空 path:{filepath}");
        return default;
    }

    public static void Write(string filepath,ExcelData ed)
    {
        try
        {
            if (Path.GetExtension(filepath) != ".xlsx")
            {
                GDebug.Instance.Error($"将要存储的文件格式不合法[非.xlsx] path:{filepath}");
                return;
            }

            string directoryPath = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (File.Exists(filepath))
                File.Delete(filepath);

            FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (ExcelPackage package = new ExcelPackage(fs))
            {
                foreach (var worksheet in ed.GetData())
                {
                    ExcelWorksheet worksheetIn = package.Workbook.Worksheets.Add(worksheet.Key);
                    for (int i = 0; i < worksheet.Value.GetRowCount(); i++)
                    {
                        for (int j = 0; j < worksheet.Value[i].Count; j++)
                        {
                            worksheetIn.Cells[i + 1, j + 1].Value = worksheet.Value[i][j];
                        }
                    }
                }
                package.Save();
            }
            fs.Close();
        }
        catch(Exception ex)
        {
            GDebug.Instance.Error($"写入失败！【很有可能是当前文件处于打开状态而无法写入】\n{ex}");
        }
    }
}
