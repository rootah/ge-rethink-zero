using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
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
            mongoInit();}

        private void mongoInit()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("rth_dev");
        }
        private async void okBtn_Click(object sender, EventArgs e)
        {
            BsonArray daysArray = new BsonArray();
            for (int i = 0; i < daysEdit.Properties.Items.Count; i++)
            {
                var item = daysEdit.Properties.Items[i];
                if (item.CheckState != CheckState.Checked) continue;
                daysArray.Add(daysEdit.Properties.Items[i].Value.ToString());
            }
            var group = new BsonDocument
            {
                {"groupno", numEdit.Text },
                {"level", lvlEdit.Text },
                {"days", daysArray },
                {"time", timeEdit.Time.ToShortTimeString() },
                {"isindividual", indCheck.CheckState }
            };

            var collection = _database.GetCollection<BsonDocument>("groups");
            await collection.InsertOneAsync(group);}

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            listBoxControl1.Items.Clear();
            for (int i = 0; i < daysEdit.Properties.Items.Count; i++)
            {
                var item = daysEdit.Properties.Items[i];
                if (item.CheckState != CheckState.Checked) continue;
                
                listBoxControl1.Items.Add(i.ToString());
            }
        }

        //private class MyObject
        //{
        //    public MyObject() { }

        //    public int ID { get; set; }
        //    public string Val { get; set; }
        //}
    }
}
