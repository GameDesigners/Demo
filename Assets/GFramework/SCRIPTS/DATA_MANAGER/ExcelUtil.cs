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

public class ExcelData
{
    private Dictionary<string, List<List<string>>> excel_data_map;

    public ExcelData()
    {
        excel_data_map = new Dictionary<string, List<List<string>>>();
    }

    public Dictionary<string,List<List<string>>> GetData() =>excel_data_map;

    public bool Add(string sheetName,List<List<string>> data)
    {
        if(excel_data_map.ContainsKey(sheetName))
        {
            GDebug.Instance.Error($"ExcelData中：表格{sheetName}已经存在");
            return false;
        }    
        excel_data_map.Add(sheetName, data);
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

    public List<List<string>> this[string sheetName]
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
            ExcelData ed = new ExcelData();
            FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (ExcelPackage package = new ExcelPackage(fs))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {

                    if (worksheet != null)
                    {
                        List<List<string>> dt = new List<List<string>>();
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;
                        for (int row = titleIndex; row <= rowCount; row++)
                        {
                            List<string> col_list = new List<string>();
                            for (int col = 1; col <= colCount; col++)
                                col_list.Add(worksheet.Cells[row, col].Value.ToString());
                            dt.Add(col_list);
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
        else if (Path.GetExtension(filepath) != ".xlsx")
            GDebug.Instance.Error($"当前文件格式不合法[非.xlsx] path:{filepath}");
        else
            GDebug.Instance.Error($"读取Excel文件为空 path:{filepath}");
        return default;
    }

    public static void Write(string filepath,ExcelData ed)
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
        using(ExcelPackage package=new ExcelPackage(fs))
        {
            foreach(var worksheet in ed.GetData())
            {
                ExcelWorksheet worksheetIn = package.Workbook.Worksheets.Add(worksheet.Key);
                for (int i = 0; i < worksheet.Value.Count; i++)
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
}
