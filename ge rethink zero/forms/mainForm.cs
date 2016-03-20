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
            // Handling the QueryControl event that will populate all automatically generated Documents
            this.tabbedView1.QueryControl += tabbedView1_QueryControl;
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

        //private async void fillnavBtn_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    treeList1.BeginUnboundLoad();
            
        //    var groupcollection = _database.GetCollection<BsonDocument>("groups");
        //    var groupprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("groupno");
        //    var groupsort = Builders<BsonDocument>.Sort.Ascending("groupno");
        //    var groupcursor = await groupcollection.Find(new BsonDocument()).Project(groupprojection).Sort(groupsort).ToCursorAsync();

        //    var stdcollection = _database.GetCollection<BsonDocument>("students");
        //    var stdprojection = Builders<BsonDocument>.Projection.Exclude("_id").Include("fullname");
        //    var stdsort = Builders<BsonDocument>.Sort.Ascending("lname");

        //    foreach (var document in groupcursor.ToEnumerable())
        //    {
        //        TreeListNode parentForRootNodes = null;
        //        var rootNode = treeList1.AppendNode( new object[] { document.Values.Single().ToString() }, parentForRootNodes);

        //        var stdfilter = Builders<BsonDocument>.Filter.Eq("groupno", document.Values.Single().ToString());
        //        var stdcursor = stdcollection.Find(stdfilter).Project(stdprojection).Sort(stdsort).ToCursor();

        //        foreach (var stddocument in stdcursor.ToEnumerable())
        //        {
        //            treeList1.AppendNode(new object[] { stddocument.Values.Single().ToString() }, rootNode);
        //        }
        //    }

        //    treeList1.EndUnboundLoad();
        //}

        // Assigning a required content for each auto generated Document
        void tabbedView1_QueryControl(object sender, DevExpress.XtraBars.Docking2010.Views.QueryControlEventArgs e)
        {
            if (e.Control == null)
                e.Control = new System.Windows.Forms.Control();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer dockingContainer1 = new DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer();
            this.documentGroup1 = new DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup(this.components);
            this.document2 = new DevExpress.XtraBars.Docking2010.Views.Tabbed.Document(this.components);
            this.formAssistant1 = new DevExpress.XtraBars.FormAssistant();
            this.tabFormDefaultManager1 = new DevExpress.XtraBars.TabFormDefaultManager();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.ribbonCtrl = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.backstageViewControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewControl();
            this.backstageViewClientControl2 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            this.backstageViewClientControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.backstageViewTabItem1 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.backstageViewTabItem2 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.backstageViewItemSeparator1 = new DevExpress.XtraBars.Ribbon.BackstageViewItemSeparator();
            this.backstageExitBtn = new DevExpress.XtraBars.Ribbon.BackstageViewButtonItem();
            this.newGroupBtn = new DevExpress.XtraBars.BarButtonItem();
            this.newStdBtn = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.editBtn = new DevExpress.XtraBars.BarButtonItem();
            this.delBtn = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticItem2 = new DevExpress.XtraBars.BarStaticItem();
            this.fillnavBtn = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticItem3 = new DevExpress.XtraBars.BarStaticItem();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem3 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem4 = new DevExpress.XtraBars.BarButtonItem();
            this.rPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.styleController1 = new DevExpress.XtraEditors.StyleController(this.components);
            this.backstageViewClientControl4 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            this.backstageViewClientControl5 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            this.backstageViewClientControl6 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            this.backstageViewTabItem3 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.backstageViewTabItem4 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.backstageViewTabItem5 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.backstageViewTabItem6 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.stdPanel = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel3_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.stdGrid = new DevExpress.XtraGrid.GridControl();
            this.realTimeStdSource = new DevExpress.Data.RealTimeSource();
            this.stdView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.groupPanel = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel2_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.groupGrid = new DevExpress.XtraGrid.GridControl();
            this.realTimeGroupSource = new DevExpress.Data.RealTimeSource();
            this.groupView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.detailsPanel = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.dockPanel1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.controlContainer1 = new DevExpress.XtraBars.Docking.ControlContainer();
            this.detailsLayoutControlGroup = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlGroup = new DevExpress.XtraLayout.LayoutControlGroup();
            this.emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.splitterItem1 = new DevExpress.XtraLayout.SplitterItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dataSet = new System.Data.DataSet();
            this.dataTable1 = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.documentManager1 = new DevExpress.XtraBars.Docking2010.DocumentManager(this.components);
            this.tabbedView1 = new DevExpress.XtraBars.Docking2010.Views.Tabbed.TabbedView(this.components);
            this.realTimeDetailSource = new DevExpress.Data.RealTimeSource();
            ((System.ComponentModel.ISupportInitialize)(this.documentGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.document2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabFormDefaultManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonCtrl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backstageViewControl1)).BeginInit();
            this.backstageViewControl1.SuspendLayout();
            this.backstageViewClientControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.styleController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.stdPanel.SuspendLayout();
            this.dockPanel3_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stdGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stdView)).BeginInit();
            this.groupPanel.SuspendLayout();
            this.dockPanel2_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupView)).BeginInit();
            this.detailsPanel.SuspendLayout();
            this.dockPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailsLayoutControlGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitterItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.documentManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabbedView1)).BeginInit();
            this.SuspendLayout();
            // 
            // documentGroup1
            // 
            this.documentGroup1.Items.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.Document[] {
            this.document2});
            // 
            // document2
            // 
            this.document2.Caption = "details";
            this.document2.ControlName = "detailsPanel";
            this.document2.FloatLocation = new System.Drawing.Point(0, 0);
            this.document2.FloatSize = new System.Drawing.Size(200, 200);
            this.document2.Properties.AllowClose = DevExpress.Utils.DefaultBoolean.True;
            this.document2.Properties.AllowFloat = DevExpress.Utils.DefaultBoolean.True;
            this.document2.Properties.AllowFloatOnDoubleClick = DevExpress.Utils.DefaultBoolean.True;
            // 
            // tabFormDefaultManager1
            // 
            this.tabFormDefaultManager1.DockControls.Add(this.barDockControlTop);
            this.tabFormDefaultManager1.DockControls.Add(this.barDockControlBottom);
            this.tabFormDefaultManager1.DockControls.Add(this.barDockControlLeft);
            this.tabFormDefaultManager1.DockControls.Add(this.barDockControlRight);
            this.tabFormDefaultManager1.Form = this;
            this.tabFormDefaultManager1.MaxItemId = 1;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Size = new System.Drawing.Size(1331, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 493);
            this.barDockControlBottom.Size = new System.Drawing.Size(1331, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 493);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1331, 0);
            this.barDockControlRight.Size = new System.Drawing.Size(0, 493);
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Office 2016 Colorful";
            // 
            // ribbonCtrl
            // 
            this.ribbonCtrl.ApplicationButtonDropDownControl = this.backstageViewControl1;
            this.ribbonCtrl.ApplicationButtonText = "...";
            this.ribbonCtrl.DrawGroupsBorder = false;
            this.ribbonCtrl.ExpandCollapseItem.Id = 0;
            this.ribbonCtrl.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonCtrl.ExpandCollapseItem,
            this.newGroupBtn,
            this.newStdBtn,
            this.barStaticItem1,
            this.editBtn,
            this.delBtn,
            this.barStaticItem2,
            this.fillnavBtn,
            this.barStaticItem3,
            this.barButtonItem1,
            this.barButtonItem2,
            this.barButtonItem3,
            this.barButtonItem4});
            this.ribbonCtrl.Location = new System.Drawing.Point(0, 0);
            this.ribbonCtrl.MaxItemId = 30;
            this.ribbonCtrl.Name = "ribbonCtrl";
            this.ribbonCtrl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.rPage1,
            this.rPage2,
            this.ribbonPage1});
            this.ribbonCtrl.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.OfficeUniversal;
            this.ribbonCtrl.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonCtrl.ShowFullScreenButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbonCtrl.ShowItemCaptionsInPageHeader = true;
            this.ribbonCtrl.ShowItemCaptionsInQAT = true;
            this.ribbonCtrl.Size = new System.Drawing.Size(1331, 80);
            this.ribbonCtrl.StatusBar = this.ribbonStatBar;
            this.ribbonCtrl.TransparentEditors = true;
            // 
            // backstageViewControl1
            // 
            this.backstageViewControl1.BackstageViewShowRibbonItems = DevExpress.XtraBars.Ribbon.BackstageViewShowRibbonItems.Title;
            this.backstageViewControl1.CaptionHorizontalAlignment = DevExpress.Utils.Drawing.ItemHorizontalAlignment.Left;
            this.backstageViewControl1.ColorScheme = DevExpress.XtraBars.Ribbon.RibbonControlColorScheme.Yellow;
            this.backstageViewControl1.Controls.Add(this.backstageViewClientControl2);
            this.backstageViewControl1.Controls.Add(this.backstageViewClientControl1);
            this.backstageViewControl1.GlyphHorizontalAlignment = DevExpress.Utils.Drawing.ItemHorizontalAlignment.Left;
            this.backstageViewControl1.GlyphLocation = DevExpress.Utils.Drawing.ItemLocation.Left;
            this.backstageViewControl1.GlyphToCaptionIndent = 10;
            this.backstageViewControl1.Items.Add(this.backstageViewTabItem1);
            this.backstageViewControl1.Items.Add(this.backstageViewTabItem2);
            this.backstageViewControl1.Items.Add(this.backstageViewItemSeparator1);
            this.backstageViewControl1.Items.Add(this.backstageExitBtn);
            this.backstageViewControl1.Location = new System.Drawing.Point(1020, 12);
            this.backstageViewControl1.Name = "backstageViewControl1";
            this.backstageViewControl1.PaintStyle = DevExpress.XtraBars.Ribbon.BackstageViewPaintStyle.Skinned;
            this.backstageViewControl1.Ribbon = this.ribbonCtrl;
            this.backstageViewControl1.SelectedTab = this.backstageViewTabItem2;
            this.backstageViewControl1.SelectedTabIndex = 1;
            this.backstageViewControl1.Size = new System.Drawing.Size(341, 54);
            this.backstageViewControl1.Style = DevExpress.XtraBars.Ribbon.BackstageViewStyle.Office2013;
            this.backstageViewControl1.TabIndex = 6;
            // 
            // backstageViewClientControl2
            // 
            this.backstageViewClientControl2.Location = new System.Drawing.Point(157, 63);
            this.backstageViewClientControl2.Name = "backstageViewClientControl2";
            this.backstageViewClientControl2.Size = new System.Drawing.Size(166, 0);
            this.backstageViewClientControl2.TabIndex = 2;
            // 
            // backstageViewClientControl1
            // 
            this.backstageViewClientControl1.Controls.Add(this.panelControl1);
            this.backstageViewClientControl1.Location = new System.Drawing.Point(157, 63);
            this.backstageViewClientControl1.Name = "backstageViewClientControl1";
            this.backstageViewClientControl1.Size = new System.Drawing.Size(166, 0);
            this.backstageViewClientControl1.TabIndex = 1;
            // 
            // panelControl1
            // 
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(166, 0);
            this.panelControl1.TabIndex = 1;
            // 
            // backstageViewTabItem1
            // 
            this.backstageViewTabItem1.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.backstageViewTabItem1.Caption = "   Settings";
            this.backstageViewTabItem1.CaptionHorizontalAlignment = DevExpress.Utils.Drawing.ItemHorizontalAlignment.Right;
            this.backstageViewTabItem1.ContentControl = this.backstageViewClientControl1;
            this.backstageViewTabItem1.Glyph = ((System.Drawing.Image)(resources.GetObject("backstageViewTabItem1.Glyph")));
            this.backstageViewTabItem1.GlyphHorizontalAlignment = DevExpress.Utils.Drawing.ItemHorizontalAlignment.Left;
            this.backstageViewTabItem1.GlyphLocation = DevExpress.Utils.Drawing.ItemLocation.Left;
            this.backstageViewTabItem1.Name = "backstageViewTabItem1";
            this.backstageViewTabItem1.Selected = false;
            // 
            // backstageViewTabItem2
            // 
            this.backstageViewTabItem2.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.backstageViewTabItem2.Caption = "   more tab";
            this.backstageViewTabItem2.ContentControl = this.backstageViewClientControl2;
            this.backstageViewTabItem2.Glyph = ((System.Drawing.Image)(resources.GetObject("backstageViewTabItem2.Glyph")));
            this.backstageViewTabItem2.GlyphHorizontalAlignment = DevExpress.Utils.Drawing.ItemHorizontalAlignment.Left;
            this.backstageViewTabItem2.GlyphLocation = DevExpress.Utils.Drawing.ItemLocation.Left;
            this.backstageViewTabItem2.Name = "backstageViewTabItem2";
            this.backstageViewTabItem2.Selected = true;
            // 
            // backstageViewItemSeparator1
            // 
            this.backstageViewItemSeparator1.Name = "backstageViewItemSeparator1";
            // 
            // backstageExitBtn
            // 
            this.backstageExitBtn.Caption = "Exit";
            this.backstageExitBtn.Name = "backstageExitBtn";
            this.backstageExitBtn.ItemClick += new DevExpress.XtraBars.Ribbon.BackstageViewItemEventHandler(this.backstageExitBtn_ItemClick);
            // 
            // newGroupBtn
            // 
            this.newGroupBtn.Caption = "group";
            this.newGroupBtn.Id = 14;
            this.newGroupBtn.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("newGroupBtn.LargeGlyph")));
            this.newGroupBtn.Name = "newGroupBtn";
            this.newGroupBtn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem4_ItemClick);
            // 
            // newStdBtn
            // 
            this.newStdBtn.Caption = "student";
            this.newStdBtn.Id = 16;
            this.newStdBtn.Name = "newStdBtn";
            this.newStdBtn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.newStdBtn_ItemClick);
            // 
            // barStaticItem1
            // 
            this.barStaticItem1.Caption = "Create";
            this.barStaticItem1.Enabled = false;
            this.barStaticItem1.Id = 17;
            this.barStaticItem1.Name = "barStaticItem1";
            this.barStaticItem1.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // editBtn
            // 
            this.editBtn.Caption = "edit";
            this.editBtn.Id = 18;
            this.editBtn.Name = "editBtn";
            // 
            // delBtn
            // 
            this.delBtn.Caption = "delete";
            this.delBtn.Id = 19;
            this.delBtn.Name = "delBtn";
            this.delBtn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.delBtn_ItemClick);
            // 
            // barStaticItem2
            // 
            this.barStaticItem2.Caption = "Selected op.";
            this.barStaticItem2.Enabled = false;
            this.barStaticItem2.Id = 20;
            this.barStaticItem2.Name = "barStaticItem2";
            this.barStaticItem2.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // fillnavBtn
            // 
            this.fillnavBtn.Caption = "fillnav";
            this.fillnavBtn.Id = 24;
            this.fillnavBtn.Name = "fillnavBtn";
            // 
            // barStaticItem3
            // 
            this.barStaticItem3.Caption = "Display";
            this.barStaticItem3.Enabled = false;
            this.barStaticItem3.Id = 25;
            this.barStaticItem3.Name = "barStaticItem3";
            this.barStaticItem3.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "Today";
            this.barButtonItem1.Id = 26;
            this.barButtonItem1.Name = "barButtonItem1";
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "Week";
            this.barButtonItem2.Id = 27;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // barButtonItem3
            // 
            this.barButtonItem3.Caption = "Month";
            this.barButtonItem3.Id = 28;
            this.barButtonItem3.Name = "barButtonItem3";
            // 
            // barButtonItem4
            // 
            this.barButtonItem4.ActAsDropDown = true;
            this.barButtonItem4.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.DropDown;
            this.barButtonItem4.Caption = "Custom";
            this.barButtonItem4.Id = 29;
            this.barButtonItem4.Name = "barButtonItem4";
            // 
            // rPage1
            // 
            this.rPage1.Name = "rPage1";
            this.rPage1.Text = "Dash";
            this.rPage1.Visible = false;
            // 
            // rPage2
            // 
            this.rPage2.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup2});
            this.rPage2.Name = "rPage2";
            this.rPage2.Text = "Groups / Peoples";
            // 
            // ribbonPageGroup2
            // 
            this.ribbonPageGroup2.ItemLinks.Add(this.barStaticItem1, true);
            this.ribbonPageGroup2.ItemLinks.Add(this.newGroupBtn);
            this.ribbonPageGroup2.ItemLinks.Add(this.newStdBtn);
            this.ribbonPageGroup2.ItemLinks.Add(this.barStaticItem2, true);
            this.ribbonPageGroup2.ItemLinks.Add(this.editBtn);
            this.ribbonPageGroup2.ItemLinks.Add(this.delBtn);
            this.ribbonPageGroup2.ItemLinks.Add(this.fillnavBtn);
            this.ribbonPageGroup2.Name = "ribbonPageGroup2";
            this.ribbonPageGroup2.Text = "ribbonPageGroup2";
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroup1});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Payments";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.barStaticItem3);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItem1);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItem2);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItem3);
            this.ribbonPageGroup1.ItemLinks.Add(this.barButtonItem4);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "ribbonPageGroup1";
            // 
            // ribbonStatBar
            // 
            this.ribbonStatBar.Location = new System.Drawing.Point(0, 472);
            this.ribbonStatBar.Name = "ribbonStatBar";
            this.ribbonStatBar.Ribbon = this.ribbonCtrl;
            this.ribbonStatBar.Size = new System.Drawing.Size(1331, 21);
            // 
            // styleController1
            // 
            this.styleController1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.styleController1.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.NoBorder;
            // 
            // backstageViewClientControl4
            // 
            this.backstageViewClientControl4.Location = new System.Drawing.Point(195, 0);
            this.backstageViewClientControl4.Name = "backstageViewClientControl4";
            this.backstageViewClientControl4.Size = new System.Drawing.Size(316, 469);
            this.backstageViewClientControl4.TabIndex = 2;
            // 
            // backstageViewClientControl5
            // 
            this.backstageViewClientControl5.Location = new System.Drawing.Point(195, 0);
            this.backstageViewClientControl5.Name = "backstageViewClientControl5";
            this.backstageViewClientControl5.Size = new System.Drawing.Size(316, 469);
            this.backstageViewClientControl5.TabIndex = 3;
            // 
            // backstageViewClientControl6
            // 
            this.backstageViewClientControl6.Location = new System.Drawing.Point(195, 0);
            this.backstageViewClientControl6.Name = "backstageViewClientControl6";
            this.backstageViewClientControl6.Size = new System.Drawing.Size(316, 469);
            this.backstageViewClientControl6.TabIndex = 4;
            // 
            // backstageViewTabItem3
            // 
            this.backstageViewTabItem3.Caption = "backstageViewTabItem3";
            this.backstageViewTabItem3.Name = "backstageViewTabItem3";
            this.backstageViewTabItem3.Selected = false;
            // 
            // backstageViewTabItem4
            // 
            this.backstageViewTabItem4.Caption = "backstageViewTabItem4";
            this.backstageViewTabItem4.ContentControl = this.backstageViewClientControl4;
            this.backstageViewTabItem4.Name = "backstageViewTabItem4";
            this.backstageViewTabItem4.Selected = false;
            // 
            // backstageViewTabItem5
            // 
            this.backstageViewTabItem5.Caption = "backstageViewTabItem5";
            this.backstageViewTabItem5.ContentControl = this.backstageViewClientControl5;
            this.backstageViewTabItem5.Name = "backstageViewTabItem5";
            this.backstageViewTabItem5.Selected = false;
            // 
            // backstageViewTabItem6
            // 
            this.backstageViewTabItem6.Caption = "backstageViewTabItem6";
            this.backstageViewTabItem6.ContentControl = this.backstageViewClientControl6;
            this.backstageViewTabItem6.Name = "backstageViewTabItem6";
            this.backstageViewTabItem6.Selected = false;
            // 
            // dockManager1
            // 
            this.dockManager1.AllowGlyphSkinning = true;
            this.dockManager1.Form = this;
            this.dockManager1.HiddenPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.stdPanel,
            this.groupPanel});
            this.dockManager1.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.detailsPanel,
            this.dockPanel1});
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane"});
            this.dockManager1.ClosingPanel += new DevExpress.XtraBars.Docking.DockPanelCancelEventHandler(this.dockManager1_ClosingPanel);
            // 
            // stdPanel
            // 
            this.stdPanel.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.stdPanel.Controls.Add(this.dockPanel3_Container);
            this.stdPanel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.stdPanel.ID = new System.Guid("0b086ae8-2315-487a-bde1-733501d911e4");
            this.stdPanel.Location = new System.Drawing.Point(219, 80);
            this.stdPanel.Name = "stdPanel";
            this.stdPanel.Options.ShowAutoHideButton = false;
            this.stdPanel.Options.ShowCloseButton = false;
            this.stdPanel.Options.ShowMaximizeButton = false;
            this.stdPanel.OriginalSize = new System.Drawing.Size(256, 200);
            this.stdPanel.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.stdPanel.SavedIndex = 1;
            this.stdPanel.Size = new System.Drawing.Size(256, 416);
            this.stdPanel.Text = "Students";
            this.stdPanel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            // 
            // dockPanel3_Container
            // 
            this.dockPanel3_Container.Controls.Add(this.stdGrid);
            this.dockPanel3_Container.Location = new System.Drawing.Point(4, 38);
            this.dockPanel3_Container.Name = "dockPanel3_Container";
            this.dockPanel3_Container.Size = new System.Drawing.Size(248, 374);
            this.dockPanel3_Container.TabIndex = 0;
            // 
            // stdGrid
            // 
            this.stdGrid.DataSource = this.realTimeStdSource;
            this.stdGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stdGrid.Location = new System.Drawing.Point(0, 0);
            this.stdGrid.MainView = this.stdView;
            this.stdGrid.MenuManager = this.tabFormDefaultManager1;
            this.stdGrid.Name = "stdGrid";
            this.stdGrid.Size = new System.Drawing.Size(248, 374);
            this.stdGrid.TabIndex = 0;
            this.stdGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.stdView});
            // 
            // realTimeStdSource
            // 
            this.realTimeStdSource.DisplayableProperties = null;
            this.realTimeStdSource.UseWeakEventHandler = true;
            // 
            // stdView
            // 
            this.stdView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.stdView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.None;
            this.stdView.GridControl = this.stdGrid;
            this.stdView.Name = "stdView";
            this.stdView.OptionsBehavior.Editable = false;
            this.stdView.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.stdView.OptionsView.ShowColumnHeaders = false;
            this.stdView.OptionsView.ShowGroupPanel = false;
            this.stdView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            this.stdView.OptionsView.ShowIndicator = false;
            this.stdView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            this.stdView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.stdView_FocusedRowChanged);
            // 
            // groupPanel
            // 
            this.groupPanel.Controls.Add(this.dockPanel2_Container);
            this.groupPanel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.groupPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.groupPanel.ID = new System.Guid("ae0d3af9-f675-4141-abf4-922ec195ef52");
            this.groupPanel.Location = new System.Drawing.Point(0, 80);
            this.groupPanel.Name = "groupPanel";
            this.groupPanel.Options.ShowAutoHideButton = false;
            this.groupPanel.Options.ShowCloseButton = false;
            this.groupPanel.Options.ShowMaximizeButton = false;
            this.groupPanel.OriginalSize = new System.Drawing.Size(219, 200);
            this.groupPanel.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.groupPanel.SavedIndex = 0;
            this.groupPanel.Size = new System.Drawing.Size(219, 416);
            this.groupPanel.Text = "Groups";
            this.groupPanel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            // 
            // dockPanel2_Container
            // 
            this.dockPanel2_Container.Controls.Add(this.groupGrid);
            this.dockPanel2_Container.Location = new System.Drawing.Point(4, 38);
            this.dockPanel2_Container.Name = "dockPanel2_Container";
            this.dockPanel2_Container.Size = new System.Drawing.Size(211, 374);
            this.dockPanel2_Container.TabIndex = 0;
            // 
            // groupGrid
            // 
            this.groupGrid.DataSource = this.realTimeGroupSource;
            this.groupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupGrid.Location = new System.Drawing.Point(0, 0);
            this.groupGrid.MainView = this.groupView;
            this.groupGrid.MenuManager = this.tabFormDefaultManager1;
            this.groupGrid.Name = "groupGrid";
            this.groupGrid.Size = new System.Drawing.Size(211, 374);
            this.groupGrid.TabIndex = 0;
            this.groupGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.groupView});
            // 
            // realTimeGroupSource
            // 
            this.realTimeGroupSource.DisplayableProperties = null;
            this.realTimeGroupSource.UseWeakEventHandler = true;
            // 
            // groupView
            // 
            this.groupView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.groupView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.None;
            this.groupView.GridControl = this.groupGrid;
            this.groupView.Name = "groupView";
            this.groupView.OptionsBehavior.Editable = false;
            this.groupView.OptionsFind.ShowClearButton = false;
            this.groupView.OptionsFind.ShowCloseButton = false;
            this.groupView.OptionsFind.ShowFindButton = false;
            this.groupView.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.groupView.OptionsSelection.MultiSelect = true;
            this.groupView.OptionsView.ShowColumnHeaders = false;
            this.groupView.OptionsView.ShowGroupPanel = false;
            this.groupView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            this.groupView.OptionsView.ShowIndicator = false;
            this.groupView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            this.groupView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.groupView_FocusedRowChanged);
            // 
            // detailsPanel
            // 
            this.detailsPanel.Controls.Add(this.dockPanel1_Container);
            this.detailsPanel.DockedAsTabbedDocument = true;
            this.detailsPanel.ID = new System.Guid("fdf7364a-93e7-4703-8f3b-2a6f48760aab");
            this.detailsPanel.Name = "detailsPanel";
            this.detailsPanel.OriginalSize = new System.Drawing.Size(200, 200);
            this.detailsPanel.Text = "details";
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Location = new System.Drawing.Point(0, 0);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1125, 362);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // dockPanel1
            // 
            this.dockPanel1.Controls.Add(this.controlContainer1);
            this.dockPanel1.Dock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.dockPanel1.ID = new System.Guid("92f88938-fcdf-40dc-b49a-1ef055610df8");
            this.dockPanel1.Location = new System.Drawing.Point(0, 80);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.OriginalSize = new System.Drawing.Size(200, 200);
            this.dockPanel1.Size = new System.Drawing.Size(200, 392);
            this.dockPanel1.Text = "dockPanel1";
            // 
            // controlContainer1
            // 
            this.controlContainer1.Location = new System.Drawing.Point(4, 38);
            this.controlContainer1.Name = "controlContainer1";
            this.controlContainer1.Size = new System.Drawing.Size(192, 350);
            this.controlContainer1.TabIndex = 0;
            // 
            // detailsLayoutControlGroup
            // 
            this.detailsLayoutControlGroup.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.detailsLayoutControlGroup.GroupBordersVisible = false;
            this.detailsLayoutControlGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlGroup,
            this.emptySpaceItem2,
            this.emptySpaceItem1,
            this.splitterItem1,
            this.layoutControlItem1});
            this.detailsLayoutControlGroup.Location = new System.Drawing.Point(0, 0);
            this.detailsLayoutControlGroup.Name = "Root";
            this.detailsLayoutControlGroup.Size = new System.Drawing.Size(915, 392);
            this.detailsLayoutControlGroup.TextVisible = false;
            // 
            // layoutControlGroup
            // 
            this.layoutControlGroup.GroupBordersVisible = false;
            this.layoutControlGroup.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup.Name = "layoutControlGroup";
            this.layoutControlGroup.Size = new System.Drawing.Size(223, 22);
            this.layoutControlGroup.TextVisible = false;
            // 
            // emptySpaceItem2
            // 
            this.emptySpaceItem2.AllowHotTrack = false;
            this.emptySpaceItem2.Location = new System.Drawing.Point(504, 0);
            this.emptySpaceItem2.Name = "emptySpaceItem2";
            this.emptySpaceItem2.Size = new System.Drawing.Size(391, 372);
            this.emptySpaceItem2.TextSize = new System.Drawing.Size(0, 0);
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 22);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(223, 350);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // splitterItem1
            // 
            this.splitterItem1.AllowHotTrack = true;
            this.splitterItem1.Location = new System.Drawing.Point(223, 0);
            this.splitterItem1.Name = "splitterItem1";
            this.splitterItem1.Size = new System.Drawing.Size(12, 372);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(50, 20);
            // 
            // dataSet
            // 
            this.dataSet.DataSetName = "NewDataSet";
            this.dataSet.Locale = new System.Globalization.CultureInfo("en-US");
            this.dataSet.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1});
            // 
            // dataTable1
            // 
            this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1});
            this.dataTable1.Locale = new System.Globalization.CultureInfo("en-US");
            this.dataTable1.TableName = "sourcetable";
            // 
            // dataColumn1
            // 
            this.dataColumn1.ColumnName = "groupno";
            // 
            // documentManager1
            // 
            this.documentManager1.ContainerControl = this;
            this.documentManager1.View = this.tabbedView1;
            this.documentManager1.ViewCollection.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseView[] {
            this.tabbedView1});
            // 
            // tabbedView1
            // 
            this.tabbedView1.DocumentGroups.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup[] {
            this.documentGroup1});
            this.tabbedView1.Documents.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseDocument[] {
            this.document2});
            dockingContainer1.Element = this.documentGroup1;
            dockingContainer1.Length.UnitValue = 1.4333333333333334D;
            this.tabbedView1.RootContainer.Nodes.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer[] {
            dockingContainer1});
            // 
            // realTimeDetailSource
            // 
            this.realTimeDetailSource.DisplayableProperties = null;
            this.realTimeDetailSource.UseWeakEventHandler = true;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1331, 493);
            this.Controls.Add(this.backstageViewControl1);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.ribbonStatBar);
            this.Controls.Add(this.ribbonCtrl);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "mainForm";
            this.Ribbon = this.ribbonCtrl;
            this.ShowIcon = false;
            this.StatusBar = this.ribbonStatBar;
            this.Text = "lynx\'s DB mongo pre 0.0.1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.documentGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.document2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabFormDefaultManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonCtrl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backstageViewControl1)).EndInit();
            this.backstageViewControl1.ResumeLayout(false);
            this.backstageViewClientControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.styleController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.stdPanel.ResumeLayout(false);
            this.dockPanel3_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.stdGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stdView)).EndInit();
            this.groupPanel.ResumeLayout(false);
            this.dockPanel2_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupView)).EndInit();
            this.detailsPanel.ResumeLayout(false);
            this.dockPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.detailsLayoutControlGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitterItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.documentManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabbedView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
