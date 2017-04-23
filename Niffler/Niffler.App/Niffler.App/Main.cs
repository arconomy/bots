using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Niffler.App
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        public event ButtonSellClickedEventHandler ButtonSellClicked;
        public delegate void ButtonSellClickedEventHandler();
        public event ButtonBuyClickedEventHandler ButtonBuyClicked;
        public delegate void ButtonBuyClickedEventHandler();

        public delegate void dlSetButtonText(string cTxt, double dPrice);

        private void ButtonSell_Click(object sender, EventArgs e)
        {
            if (sender != null)
            {
                ButtonSellClicked();
            }
        }

        private void ButtonBuy_Click(object sender, EventArgs e)
        {
            if (ButtonBuyClicked != null)
            {
                ButtonBuyClicked();
            }
        }

        public void SetButtonText(string cTxt, double dPrice)
        {

            Control[] myCont = null;
            //external application (cAlgo) tries to call the frame application. invocation is required
            myCont = this.Controls.Find(cTxt, true);
            if (myCont.Length == 0)
                return;
            if (myCont[0].InvokeRequired)
            {
                myCont[0].Invoke(new dlSetButtonText(SetButtonText), cTxt, dPrice);
            }
            else
            {
                //after invocation and 'recall' of the sub the button text can be set
                myCont[0].Text = dPrice.ToString();
            }

        }

    }
}
