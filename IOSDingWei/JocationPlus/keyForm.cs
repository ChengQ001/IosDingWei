using Jocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChengQByIOS
{
    public partial class keyForm : Form
    {
        public keyForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox1.AppendText(GetID.sFingerPrint);
        }

        private void keyForm_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox1.AppendText(GetID.sFingerPrint);
        }
    }
}
