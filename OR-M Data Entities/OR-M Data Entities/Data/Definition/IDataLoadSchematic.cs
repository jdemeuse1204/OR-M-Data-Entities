using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IDataLoadSchematic
    {
        HashSet<IDataLoadSchematic> Children { get; }

        Type ActualType { get; }

        string[] PrimaryKeyNames { get; }

        OSchematicLoadedKeys LoadedCompositePrimaryKeys { get; }

        object ReferenceToCurrent { get; set; }

        /// <summary>
        /// used to identity Foreign Key because object can have Foreign Key with same type,
        /// but load different data.  IE - User CreatedBy, User EditedBy
        /// </summary>
        string PropertyName { get; }

        Type Type { get; }
    }
}
