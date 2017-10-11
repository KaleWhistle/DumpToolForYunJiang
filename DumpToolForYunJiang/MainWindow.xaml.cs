using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DumpToolForYunJiang
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DataSource

        static Dictionary<User.EStatus, string> olist = new Dictionary<User.EStatus, string>
        {
            {1,"有钱" }
            ,{2,"有闲" }
            ,{3,"有料" }
            ,{4,"有鬼" }
        };
        public Dictionary<User.EStatus, string> StatusList
        {
            get
            {
                return olist;
            }
        }
        KeyValuePair<User.EStatus, string> _kvp;
        public KeyValuePair<User.EStatus, string> SelectedStatus
        {
            get
            {
                return this._kvp;
            }
            set
            {
                this._kvp = value;
                this.RaisePropertyChanged("SelectedStatus");
            }
        }
        DelegateCommand<ComboBox> _SelectionChangedCmd = null;
        public DelegateCommand<ComboBox> SelectionChangedCmd
        {
            get
            {
                if (this._SelectionChangedCmd == null)
                {
                    this._SelectionChangedCmd = new DelegateCommand<ComboBox>(SelectionChanged);
                }

                return this._SelectionChangedCmd;
            }
        }
        void SelectionChanged(ComboBox cb)
        {
            SelectedStatus = (KeyValuePair<User.EStatus, string>)cb.SelectedItem;
            Status = SelectedStatus.Key;
        }
        public User.EStatus Status
        {
            set
            {
                var query = olist.Where(n => n.Key == value);
                if (query != null && query.Count() > 0)
                {
                    this.SelectedStatus = query.First();
                }
            }
        }

        #endregion

        public int retCode, hContext, hCard, Protocol;

        public MainWindow()
        {
            InitializeComponent();
            bInit_Click();
        }

        private M1CardConvert convert;

        private ButtonResult.ConvertResult convertBegin()
        {
            byte[] card_num = new byte[4];
            card_num[0] = (byte)((Convert.ToInt32(textBox_card_num.Text, 16) >> 24) & 0xFF);
            card_num[1] = (byte)((Convert.ToInt32(textBox_card_num.Text, 16) >> 16) & 0xFF);
            card_num[2] = (byte)((Convert.ToInt32(textBox_card_num.Text, 16) >> 8) & 0xFF);
            card_num[3] = (byte)((Convert.ToInt32(textBox_card_num.Text, 16) >> 0) & 0xFF);
            convert = new M1CardConvert(card_num);
            if(convert != null)
            {
                System.Windows.MessageBox.Show("生成成功", "生成结果", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("生成失败", "生成结果", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return convert != null ? ButtonResult.ConvertResult.Success : ButtonResult.ConvertResult.FailNoReason;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            retCode = ModWinsCard.SCardConnect(hContext, cbReader.SelectedItem.ToString(), ModWinsCard.SCARD_SHARE_SHARED, ModWinsCard.SCARD_PROTOCOL_T0 | ModWinsCard.SCARD_PROTOCOL_T1, ref hCard, ref Protocol);

            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                displayOut(1, retCode, "");
            }
            else
            {
                displayOut(0, 0, "Successful connection to " + cbReader.Text);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            convertBegin();
            convert.ConvertOutput("F:\\CC.txt");
            System.Windows.MessageBox.Show("保存成功", "保存结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void pathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
                label_Path.Text = fbd.SelectedPath;
        }

        private void displayOut(int errType, int retVal, string PrintText)
        {
            switch (errType)
            {
                case 0:
                    break;
                case 1:
                    PrintText = ModWinsCard.GetScardErrMsg(retVal);
                    break;
                case 2:
                    PrintText = "<" + PrintText;
                    break;
                case 3:
                    PrintText = ">" + PrintText;
                    break;
                case 4:
                    break;
            }
            mMsg.AppendText(PrintText);
            mMsg.AppendText("\n");
            mMsg.Focus();
        }

        private void bInit_Click()
        {
            string ReaderList = "" + Convert.ToChar(0);
            int indx;
            int pcchReaders = 0;
            string rName = "";
            //Establish Context
            retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref hContext);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                displayOut(1, retCode, "");
                return;
            }

            // 2. List PC/SC card readers installed in the system
            retCode = ModWinsCard.SCardListReaders(hContext, null, null, ref pcchReaders);
            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                displayOut(1, retCode, "");
                return;
            }

            byte[] ReadersList = new byte[pcchReaders];

            // Fill reader list
            retCode = ModWinsCard.SCardListReaders(hContext, null, ReadersList, ref pcchReaders);

            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
            {
                mMsg.AppendText("SCardListReaders Error: " + ModWinsCard.GetScardErrMsg(retCode));
                return;
            }
            else
            {
                displayOut(0, 0, " ");
            }

            rName = "";
            indx = 0;

            //Convert reader buffer to string
            while (ReadersList[indx] != 0)
            {
                while (ReadersList[indx] != 0)
                {
                    rName = rName + (char)ReadersList[indx];
                    indx = indx + 1;
                }
                //Add reader name to list
                cbReader.Items.Add(rName);
                rName = "";
                indx = indx + 1;
            }

            if (cbReader.Items.Count > 0)
            {
                cbReader.SelectedIndex = 0;
            }
        }

        private void bInit_Click(object sender, EventArgs e)
        {
            bInit_Click();
        }
    }
}
