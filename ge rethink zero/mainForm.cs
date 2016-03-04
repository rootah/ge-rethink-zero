using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;

namespace ge_rethink_zero
{
    public partial class mainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        
        public mainForm()
        {
            InitializeComponent();
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

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            groupForm gForm = new groupForm {FormBorderStyle = FormBorderStyle.FixedToolWindow};
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

        private void createBtn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }
    }
}
