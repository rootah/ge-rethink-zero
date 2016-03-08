using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking.Helpers;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraGrid.Views.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ge_rethink_zero
{
    public partial class mainForm : RibbonForm
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        public mainForm()
        {
            InitializeComponent();
            mongoInit();
            groupGridFill();
            //DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        private static void mongoInit()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("rth_dev");
        }

        private void backstageExitBtn_ItemClick(object sender, BackstageViewItemEventArgs e)
        {
            Close();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            dashSwitch.IsOn = false;
        }

        public void stdGridFill(string groupno)
        {
            var stable = new DataTable();
            stable.Clear();
            stable.BeginInit();
            stable.Columns.Add("name", typeof(string));

            var collection = _database.GetCollection<BsonDocument>("students");
            var filter = Builders<BsonDocument>.Filter.Eq("groupno", groupno);
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
            var sort = Builders<BsonDocument>.Sort.Ascending("lname");
            var cursor = collection.Find(filter).Project(projection).Sort(sort).ToCursor();
            foreach (var document in cursor.ToEnumerable())
            {
                stable.Rows.Add(document.Values.Single().ToString());
            }
            stable.EndInit();

            realTimeStdSource.DataSource = null;
            realTimeStdSource.DataSource = stable;
            stdView.PopulateColumns();
        }

        private void stdFullgridFill()
        {
            var stable = new DataTable();
            stable.Clear();
            stable.BeginInit();
            stable.Columns.Add("name", typeof(string));

            var collection = _database.GetCollection<BsonDocument>("students");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
            var sort = Builders<BsonDocument>.Sort.Ascending("lname");
            var cursor = collection.Find(new BsonDocument()).Project(projection).Sort(sort).ToCursor();
            foreach (var document in cursor.ToEnumerable())
            {
                stable.Rows.Add(document.Values.Single().ToString());
            }
            stable.EndInit();

            realTimeStdSource.DataSource = null;
            realTimeStdSource.DataSource = stable;
            stdView.PopulateColumns();
        }

        public async void groupGridFill()
        {
            var gtable = new DataTable();
            var firstrow = BsonValue.Create("All");

            gtable.Clear();
            gtable.BeginInit();
            gtable.Columns.Add("num", typeof(BsonValue));
            gtable.Rows.Add(firstrow);
            
            var collection = _database.GetCollection<BsonDocument>("groups");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
            var sort = Builders<BsonDocument>.Sort.Ascending("groupno");
            await collection.Find(new BsonDocument()).Project(projection).Sort(sort).ForEachAsync(doc => gtable.Rows.Add(doc.Values.Single()));

            gtable.EndInit();
            realTimeGroupSource.DataSource = null;
            realTimeGroupSource.DataSource = gtable;
            groupView.PopulateColumns();
        }
        private void dashSwitch_Toggled(object sender, EventArgs e)
        {
            rPage1.Visible = dashSwitch.IsOn;
        }

        private static bool canCloseFunc(DialogResult parameter)
        {
            return parameter != DialogResult.Cancel;
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var action = new FlyoutAction { Caption = "Confirm", Description = "Close the application?" };
            Predicate<DialogResult> predicate = canCloseFunc;
            var command1 = new FlyoutCommand { Text = "Close", Result = DialogResult.Yes };
            var command2 = new FlyoutCommand { Text = "Cancel", Result = DialogResult.No };
            action.Commands.Add(command1);
            action.Commands.Add(command2);
            var properties = new FlyoutProperties
            {
                Style = FlyoutStyle.MessageBox,
                Alignment = ContentAlignment.MiddleCenter
            };
            e.Cancel = FlyoutDialog.Show(this, action, properties, predicate) != DialogResult.Yes;
        }

        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            var gpanel = dockManager1.AddPanel(DockingStyle.Float);
            gpanel.Controls.Add(new groupUC(this));
            gpanel.Options.ShowAutoHideButton = false;
            gpanel.Options.ShowMaximizeButton = false;
            gpanel.FloatSize = new Size(260, 232);
            gpanel.Text = @"group add in progress ..";
            gpanel.Options.ResizeDirection = ResizeDirection.None;
            gpanel.FloatLocation = new Point(Location.X + Width / 2 - 130, Location.Y + Height / 2 - 115);
        }

        private void dockManager1_ClosingPanel(object sender, DockPanelCancelEventArgs e)
        {
            dockManager1.RemovePanel(dockManager1.ActivePanel);
        }


        private void groupView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (groupView.GetFocusedRow() == null) return;
            var selectedgroupno = groupView.GetRowCellValue(groupView.FocusedRowHandle, "num").ToString();
            if (selectedgroupno == "All")
            {
                stdPanel.Text = @"Students All";
                stdFullgridFill();
            }
            else
            {
                stdPanel.Text = @"Students " + selectedgroupno;
                stdGridFill(selectedgroupno);
            }
        }

        private void newStdBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            var stdcontrol = new stdUC(this);
            var panel = dockManager1.AddPanel(DockingStyle.Float);
            panel.Controls.Add(stdcontrol);
            panel.Options.ShowAutoHideButton = false;
            panel.Options.ShowMaximizeButton = false;
            panel.FloatSize = new Size(341, 268); //249*191 11*41 330; 244
            panel.Text = @"student add in progress ..";
            panel.Options.ResizeDirection = ResizeDirection.None;
            panel.FloatLocation = new Point(Location.X + Width / 2 - 130, Location.Y + Height / 2 - 115);
            
            stdcontrol.mongoInit();
            stdcontrol.groupComboFill();
        }

        private void stdDel()
        {
            if (stdView.GetFocusedRow() == null) return;
            var selectedstd = stdView.GetRowCellValue(stdView.FocusedRowHandle, "name").ToString();
            var filter = Builders<BsonDocument>.Filter.Eq("fullname", selectedstd);
            var collection = _database.GetCollection<BsonDocument>("students");
            collection.DeleteOne(filter);
            
            groupView_FocusedRowChanged(null, null);
        }

        private void delBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (dockManager1.ActivePanel == stdPanel)
                stdDel();
            if (dockManager1.ActivePanel == groupPanel)
                MessageBox.Show(@"group");
        }
    }
}
