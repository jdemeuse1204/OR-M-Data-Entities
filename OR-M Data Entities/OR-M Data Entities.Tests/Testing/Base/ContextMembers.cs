using System;
using System.IO;
using System.Text;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public static class ContextMembers
    {
        public static void OnOnSqlGeneration(string sql)
        {
            const string path = "C:\\users\\jdemeuse\\desktop\\OR-M Data Entities Tests\\";
            var fileName = string.Format("OR-M Sql_{0}.txt", DateTime.Now.ToString("MM-dd-yyyy"));

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (var writetext = File.AppendText(string.Format("{0}{1}", path, fileName)))
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("-----------------");
                stringBuilder.AppendLine("----- START -----");
                stringBuilder.AppendLine("-----------------");
                stringBuilder.AppendLine(string.Format("TIME:  {0}", DateTime.Now));
                stringBuilder.AppendLine(sql);
                stringBuilder.AppendLine("-----------------");
                stringBuilder.AppendLine("-----  END  -----");
                stringBuilder.AppendLine("-----------------\r");

                writetext.WriteLine(stringBuilder.ToString());
            }
        }
    }
}
