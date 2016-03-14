using System;
using System.Linq;
using ge_rethink_zero.forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ge_rethink_zero.controls
{
    public partial class stdUC : DevExpress.XtraEditors.XtraUserControl
    {
        private readonly mainForm _parentForm;
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        public stdUC(mainForm parentForm)
        {
            _parentForm = parentForm;
            InitializeComponent();
            mongoInit();
            groupComboFill();
            nameGen();
        }

        private void nameGen()
        {
            var firstName = Faker.Name.First();
            var lastName = Faker.Name.Last();

            fnameEdit.Text = firstName;
            lnameEdit.Text = lastName;
        }

        private static void mongoInit()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("rth_dev");
        }
        private void parentClose()
        {
            var parent = Parent.FindForm();
            parent?.Close();
            _parentForm.dockManager1.RemovePanel(_parentForm.dockManager1.ActivePanel);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            parentClose();
        }

        private async void groupComboFill()
        {
            enrollCombo.Properties.Items.Clear();
            var collection = _database.GetCollection<BsonDocument>("groups");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
            var sort = Builders<BsonDocument>.Sort.Ascending("groupno");
            var eq = collection.Find(new BsonDocument()).Project(projection).Sort(sort);
            await eq.ForEachAsync(doc => enrollCombo.Properties.Items.Add(doc.Values.Single()));
        }

        private async void okBtn_Click(object sender, EventArgs e)
        {
            var std = new BsonDocument
            {
                {"fname", fnameEdit.Text },
                {"lname", lnameEdit.Text },
                {"fullname", lnameEdit.Text + " " + fnameEdit.Text },
                {"phone", phoneEdit.Text },
                {"groupno", enrollCombo.Text }
            };

            var collection = _database.GetCollection<BsonDocument>("students");
            await collection.InsertOneAsync(std);

            var parent = Parent.FindForm(); parent?.Close();
            _parentForm.groupView_FocusedRowChanged(null, null);
        }
    }
}
