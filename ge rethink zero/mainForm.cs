using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking.Helpers;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraEditors.Filtering;
using DevExpress.XtraTreeList.Nodes;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ge_rethink_zero
{
    public partial class mainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        public static DataTable table;

        public mainForm()
        {
            InitializeComponent();
            mongoInit();
            //DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
        }

        private static void mongoInit()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("rth_dev");
        }

        private void backstageExitBtn_ItemClick(object sender, DevExpress.XtraBars.Ribbon.BackstageViewItemEventArgs e)
        {
            Close();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            dashSwitch.IsOn = false;
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

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var gpanel = dockManager1.AddPanel(DockingStyle.Float);
            gpanel.Controls.Add(new groupUC());
            gpanel.Options.ShowAutoHideButton = false;
            gpanel.Options.ShowMaximizeButton = false;
            gpanel.FloatSize = new Size(260, 232);
            gpanel.Text = @"group add in progress ..";
            gpanel.Options.ResizeDirection = ResizeDirection.None;
            gpanel.FloatLocation = new Point(Location.X + Width / 2 - 130, Location.Y + Height / 2 - 115);
        }

        private void dockManager1_ClosingPanel(object sender, DockPanelCancelEventArgs e)
        {
            dockManager1.RemovePanel(dockManager1.ActivePanel);}

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            table.Clear();
            table.BeginInit();
            table.Columns.Add("num", typeof(BsonValue));
            table.EndInit();

            var collection = _database.GetCollection<BsonDocument>("groups");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
            var query = collection.Find(new BsonDocument()).Project(projection).ForEachAsync(doc => table.Rows.Add(new object[] { doc.Values.Single()} ));

            realTimeSource.DataSource = table;
            gridView1.PopulateColumns();
        }

        delegate void UpdateDataSourceDelegate();
    }
}
