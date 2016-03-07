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

        public void groupGridFill()
        {
            var table = new DataTable();
            //var _id = new ObjectId();
            var firstrow = BsonValue.Create("All");

            table.Clear();
            table.BeginInit();
            table.Columns.Add("num", typeof(BsonValue));
            table.Rows.Add(firstrow);
            table.EndInit();

            var collection = _database.GetCollection<BsonDocument>("groups");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
            var sort = Builders<BsonDocument>.Sort.Ascending("groupno");
            var query = collection.Find(new BsonDocument()).Project(projection).Sort(sort).ForEachAsync(doc => table.Rows.Add(doc.Values.Single()));

            realTimeSource.DataSource = null;
            realTimeSource.DataSource = table; groupView.PopulateColumns();
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
            if (groupView.GetFocusedRow() != null)
            {
                var selectedgroupno = groupView.GetRowCellValue(groupView.FocusedRowHandle, "num").ToString();
                stdPanel.Text = @"Students [ " + selectedgroupno + @" ]";
            }
        }

        private void newStdBtn_ItemClick(object sender, ItemClickEventArgs e)
        {var gpanel = dockManager1.AddPanel(DockingStyle.Float);
            gpanel.Controls.Add(new stdUC(this));
            gpanel.Options.ShowAutoHideButton = false;
            gpanel.Options.ShowMaximizeButton = false;
            gpanel.FloatSize = new Size(341, 268); //249*191 11*41 330; 244
            gpanel.Text = @"student add in progress ..";
            gpanel.Options.ResizeDirection = ResizeDirection.None;
            gpanel.FloatLocation = new Point(Location.X + Width / 2 - 130, Location.Y + Height / 2 - 115);
        }
    }
}
