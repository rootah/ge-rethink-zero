﻿using System;
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
using DevExpress.XtraLayout;
using DevExpress.XtraNavBar;
using DevExpress.XtraTreeList.Nodes;
using ge_rethink_zero.controls;
using MongoDB.Bson;
using MongoDB.Driver;

/*
    td  1.  add parsing db field names
    td  2.  edit forms

*/

namespace ge_rethink_zero.forms
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
            stdFullgridFill();
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

        private void stdView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (stdView.GetFocusedRow() == null) return;

            var selectedstd = stdView.GetRowCellValue(stdView.FocusedRowHandle, "name").ToString();
            var filter = Builders<BsonDocument>.Filter.Eq("fullname", selectedstd);
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
            var collection = _database.GetCollection<BsonDocument>("students");
            var eq = collection.Find(filter).Project(projection).ToCursor();
            foreach (var document in eq.ToEnumerable())
            {
                detailsPanel.Text = document.Values.Single().ToString();
            }

            layoutControlGroup.Items.Clear();
            detailsFill(selectedstd);
        }

        private void detailsFill(string selectedstd)
        {
            if (stdView.GetFocusedRow() == null) return;

            var filter = Builders<BsonDocument>.Filter.Eq("fullname", selectedstd);
            var collection = _database.GetCollection<BsonDocument>("students");
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("fname").Exclude("lname");
            var eq = collection.Find(filter).Project(projection).First();

            foreach (var document in eq)
            {
                layoutControlGroup.AddItem(new SimpleLabelItem() { Text = document.Name + @": " + document.Value, ControlAlignment = ContentAlignment.TopLeft});
            }
        }

        private void stdGridFill(string groupno)
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


        public void groupView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
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
            panel.FloatSize = new Size(341, 268);
            panel.Text = @"student add in progress ..";
            panel.Options.ResizeDirection = ResizeDirection.None;
            panel.FloatLocation = new Point(Location.X + Width / 2 - 130, Location.Y + Height / 2 - 115);
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

        private async void fillnavBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            treeList1.BeginUnboundLoad();
            
            var groupcollection = _database.GetCollection<BsonDocument>("groups");
            var groupprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
            var groupsort = Builders<BsonDocument>.Sort.Ascending("groupno");
            var groupcursor = await groupcollection.Find(new BsonDocument()).Project(groupprojection).Sort(groupsort).ToCursorAsync();

            var stdcollection = _database.GetCollection<BsonDocument>("students");
            var stdprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
            var stdsort = Builders<BsonDocument>.Sort.Ascending("lname");

            foreach (var document in groupcursor.ToEnumerable())
            {
                TreeListNode parentForRootNodes = null;
                var rootNode = treeList1.AppendNode( new object[] { document.Values.Single().ToString() }, parentForRootNodes);

                var stdfilter = Builders<BsonDocument>.Filter.Eq("groupno", document.Values.Single().ToString());
                var stdcursor = stdcollection.Find(stdfilter).Project(stdprojection).Sort(stdsort).ToCursor();

                foreach (var stddocument in stdcursor.ToEnumerable())
                {
                    treeList1.AppendNode(new object[] { stddocument.Values.Single().ToString() }, rootNode);
                }
            }

            treeList1.EndUnboundLoad();
        }
    }
}