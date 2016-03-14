using System;
using System.Windows.Forms;
using ge_rethink_zero.forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ge_rethink_zero.controls
{
    public partial class groupUC : DevExpress.XtraEditors.XtraUserControl
    {
        private readonly mainForm _parentForm;
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        public groupUC(mainForm parentForm)
        {
            _parentForm = parentForm;
            InitializeComponent();
            mongoInit();
        }

        private static void mongoInit()
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
            await collection.InsertOneAsync(group);

            _parentForm.groupGridFill();
            var parent = Parent.FindForm();parent?.Close();
        }

        private void parentClose()
        {
            var parent = Parent.FindForm();
            
            parent?.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            parentClose();
        }
    }
}
