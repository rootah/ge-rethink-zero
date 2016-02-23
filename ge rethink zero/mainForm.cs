using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList.Nodes;
using RethinkDb.Driver;

namespace ge_rethink_zero
{
    public partial class mainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private static readonly RethinkDB R = RethinkDB.R;
        
        public mainForm()
        {
            InitializeComponent();
        }

        private void backstageExitBtn_ItemClick(object sender, DevExpress.XtraBars.Ribbon.BackstageViewItemEventArgs e)
        {
            Close();
        }

        private void connectBtn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var c = R.Connection()
                     .Hostname("127.0.0.1")
                     .Port(RethinkDBConstants.DefaultPort)
                     .Db("gerdb")
                     .Timeout(60)
                     .Connect();
        }

        private void mainForm_Load(object sender, System.EventArgs e)
        {
            dashSwitch.IsOn = false;
        }

        private void dashSwitch_Toggled(object sender, System.EventArgs e)
        {
            rPage1.Visible = dashSwitch.IsOn;
        }

        private static bool canCloseFunc(DialogResult parameter)
        {
            return parameter != DialogResult.Cancel;
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FlyoutAction action = new FlyoutAction { Caption = "Confirm", Description = "Close the application?" };
            Predicate<DialogResult> predicate = canCloseFunc;
            FlyoutCommand command1 = new FlyoutCommand { Text = "Close", Result = DialogResult.Yes };
            FlyoutCommand command2 = new FlyoutCommand { Text = "Cancel", Result = DialogResult.No };
            action.Commands.Add(command1);
            action.Commands.Add(command2);
            FlyoutProperties properties = new FlyoutProperties();
            //properties.ButtonSize = new Size(100, 40);
            properties.Style = FlyoutStyle.MessageBox;
            properties.Alignment = ContentAlignment.MiddleCenter;
            e.Cancel = FlyoutDialog.Show(this, action, properties, predicate) != DialogResult.Yes;
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            groupForm gForm = new groupForm();
            gForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            gForm.ShowDialog(owner: this);
            //DockPanel groupadd = dockManager1.AddPanel(DockingStyle.Float);
            //groupadd.Text = @"Adding group";
            //groupUC groupControl = new groupUC();
            //groupControl.Dock = DockStyle.Fill;
            //groupadd.ControlContainer.Controls.Add(groupControl);
            //groupadd.Options.ShowMaximizeButton = false;
            //groupadd.FloatSize = new Size(groupControl.layoutControl1.ClientWidth, groupControl.layoutControl1.ClientHeight);


            //groupadd.FloatForm.StartPosition = FormStartPosition.CenterParent;
        }
    }
}
