using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RepairTools_2._;

namespace RepairTools_2._0
{
    public partial class USBMointorOperate : Form
    {
        public USBMointorOperate()
        {
            InitializeComponent();
        }

        private void USBMointorOperate_FormClosed(object sender, FormClosedEventArgs e)
        {
            new Form1().Show();
        }
    }
}