using System;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class Script : Attribute
	{
		public Script(string fileName)
		{
            FileName = fileName;
		}

        public Script(string fileName, string path)
        {
            FileName = fileName;
            Path = path;
        }

		public string FileName { get; private set; }

        public string Path { get; private set; }
	}
}
