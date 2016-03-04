using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ge_rethink_zero
{
    public partial class groupUC : DevExpress.XtraEditors.XtraUserControl
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        public groupUC()
        {
            InitializeComponent();
            mongoInit();
        }

        private static void mongoInit()
        {   _client = new MongoClient();
            _database = _client.GetDatabase("ge_dev");
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // vars 
            if (numEdit.Text == String.Empty)
            {
                MessageBox.Show(@"Check Group no.");
                return;
            }
                
            // end vars
            var group = new BsonDocument
            {
                {"groupno", numEdit.Text },
                { "level", lvlEdit.Text},

            };
        }
    }
}
