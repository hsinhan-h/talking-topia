using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Settings
{
    public class MongoDbVecotrSearchSettings
    {
        public string ConnectionString { get; set; }
        public string VectorDatabaseName { get; set; }
        public string VectorCollectionName { get; set; }
        public string SearchIndexName { get; set; }
    }
}
