using System.Windows;
using System.Windows.Controls;

namespace Control_Panel.Views
{
    public partial class MembershipConfigWindow : Window
    {
        public short MembershipStatus { get; private set; }

        public MembershipConfigWindow()
        {
            InitializeComponent();
        }

        public void SetCurrentMembership(string membershipType)
        {
            switch (membershipType)
            {
                case "BuildersClub":
                    MembershipComboBox.SelectedIndex = 1;
                    break;
                case "TurboBuildersClub":
                    MembershipComboBox.SelectedIndex = 2;
                    break;
                case "OutrageousBuildersClub":
                    MembershipComboBox.SelectedIndex = 3;
                    break;
                default:
                    MembershipComboBox.SelectedIndex = 0;
                    break;
            }
        }

        private void SetMembershipButton_Click(object sender, RoutedEventArgs e)
        {
            MembershipStatus = (short)MembershipComboBox.SelectedIndex;
            DialogResult = true;
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
