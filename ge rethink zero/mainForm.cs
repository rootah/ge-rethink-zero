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
    }
}
