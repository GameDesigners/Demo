using System.Data;
using System.Text;
using System.IO;

public static class CSVUtil
{
    public static DataTable Read(string filepath,int titleIndex=1)
    {
        if(File.Exists(filepath))
        {
            string debug_str = "";
            DataTable dt = new DataTable();
            StreamReader reader = new StreamReader(filepath, Encoding.Default, false);
            int index = 0;
            while(!reader.EndOfStream)
            {
                index = index + 1;
                string str = reader.ReadLine();
                debug_str += str + "\n";
                string[] split = str.Split(',');
                

                if(index==titleIndex)
                {
                    DataColumn column;
                    for(int c=0;c<split.Length;c++)
                    {
                        column = new DataColumn();
                        column.DataType = System.Type.GetType("System.String");
                        column.ColumnName = split[c];
                        if (dt.Columns.Contains(split[c]))
                            column.ColumnName = split[c] + c;
                        dt.Columns.Add(column);
                    }

                    if(index>=titleIndex+1)
                    {
                        DataRow dr = dt.NewRow();
                        for(int i=0;i<split.Length;i++)
                            dr[i] = split[i];
                        dt.Rows.Add(dr);
                    }
                }
            }
            reader.Close();
            GDebug.Instance.Log(debug_str);
            return dt;
        }
        else if(Path.GetExtension(filepath)!=".csv")
            GDebug.Instance.Error($"当前文件格式不合法[非.CSV] path:{filepath}");
        else
            GDebug.Instance.Error($"读取CSV文件为空 path:{filepath}");
        return default;
    }

    public static void Write(string filepath,DataTable dt)
    {
        if (dt == null || dt.Rows.Count == 0)
        {
            GDebug.Instance.Error($"将要写入的数据为空为空。");
            return;
        }

        if (Path.GetExtension(filepath) != ".csv")
        {
            GDebug.Instance.Error($"将要存储的文件格式不合法[非.CSV] path:{filepath}");
            return;
        }

        string directoryPath = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string strBufferLine = "";
        StreamWriter writer = new StreamWriter(filepath, false, Encoding.Default);
        foreach (DataColumn c in dt.Columns)
            strBufferLine += c.ColumnName + ",";
        strBufferLine = strBufferLine.Substring(0, strBufferLine.Length - 1);
        writer.WriteLine(strBufferLine);

        for(int i=0;i<dt.Rows.Count;i++)
        {
            strBufferLine = "";
            for(int j=0;j<dt.Columns.Count;j++)
            {
                if (j > 0)
                    strBufferLine += ",";
                strBufferLine += dt.Rows[i][j].ToString().Replace(",", "");
            }
            writer.WriteLine(strBufferLine);
        }
        writer.Close();

    }
}
