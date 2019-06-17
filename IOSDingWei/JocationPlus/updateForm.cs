using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChengQByIOS
{
    public partial class updateForm : Form
    {
        public updateForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string zhanghao = textBox1.Text.Trim();
            string mima = textBox2.Text.Trim();
            string key = textBox3.Text.Trim();
            string date = dateTimePicker1.Text.Trim();
            if (string.IsNullOrWhiteSpace(zhanghao))
            {
                MessageBox.Show("账号不能为空");
                return;
            }
            if (string.IsNullOrWhiteSpace(mima))
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("秘钥不能为空");
                return;
            }
            if (string.IsNullOrWhiteSpace(date))
            {
                MessageBox.Show("时间不能为空");
                return;
            }
            if (!(zhanghao.Trim() == "admin" && mima.Trim() == "++++++"))
            {
                MessageBox.Show("账号密码不正确");
                return;
            }
            var model = new ChengQEntities().UrlTable.Where(x => x.url_code == key).FirstOrDefault();
            if (model == null)
            {
                MessageBox.Show("激活秘钥无效");
                return;
            }

            model.createtime = Convert.ToDateTime(date);

            var db = new ChengQEntities();
            db.Entry<UrlTable>(model).State = EntityState.Modified;
            MessageBox.Show(db.SaveChanges()>0?"成功":"失败"); db.SaveChanges();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string key = textBox3.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("秘钥不能为空");
                return;
            }
            var model = new ChengQEntities().UrlTable.Where(x => x.url_code == key).FirstOrDefault();
            if (model == null)
            {
                richTextBox1.AppendText("没查到此卡信息");
                return;
            }
            else
            {
                richTextBox1.Text = "";
                richTextBox1.AppendText("卡号:"+model.url_code+"   到期时间:"+model.createtime);
            }
        }
    }
}
