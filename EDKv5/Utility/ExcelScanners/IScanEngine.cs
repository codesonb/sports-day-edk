namespace EDKv5.Utility.Scanners
{
    interface IScanEngine
    {
        dynamic[,] ReadExcel(string path, string[] colNames, out int rowCount, out int colCount);
    }
}
