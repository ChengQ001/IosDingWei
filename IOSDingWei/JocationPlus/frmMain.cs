using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Service;
using TestSqlite.sq;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Jocation;
using System.IO;
using ChengQByIOS;
using System.Net;

namespace LocationCleaned
{
    [ComVisible(true)]
    public partial class frmMain : Form
    {
        frmMap map = new frmMap();
        LocationService service;
        double speed = 0.0002;
        bool keepMoving = false;
        private ChengQEntities db = new ChengQEntities();
        private string id = GetID.sFingerPrint;
        /// <summary>
        /// 经纬度坐标
        /// </summary>
        public new Location Location { get; set; } = new Location();
        public frmMain()
        {
            CreateLocationDB();
            InitializeComponent();
            ReadLocationFromDB();
            PrintMessage("不会使用联系作者QQ694340776！");
            frmMap_Load();
        }

        public void AddAuth()
        {
            var model = new ChengQEntities().UrlTable.Where(x => x.url_code == id).FirstOrDefault();
            if (model == null)
            {
                db = new ChengQEntities();
                db.UrlTable.Add(new UrlTable()
                {
                    use_count = 0,
                    total_count = 0,
                    url_code = id,
                    remark = "",
                    createtime = DateTime.Now.AddDays(2)
                });
                db.SaveChanges();
              
            }
            else
            {
                if (model.createtime < DateTime.Now)
                {
                    MessageBox.Show("体验期已经用完，联系QQ694340776");
                    btnReset.Enabled = false;
                    button7.Enabled = false;
                    button2.Enabled = false;
                    checkBox1.Enabled = false;
                    button5.Enabled = false;
                    button9.Enabled = false;
                    button8.Enabled = false;
                    button6.Enabled = false;
                    button3.Enabled = false;
                    button10.Enabled = false;
                    button4.Enabled = false;
                    button11.Enabled = false;
                }
                button12.Visible = true;
            }
            PrintMessage("到期时间为:" + model.createtime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            AddAuth();
            NativeLibraries.Load();
            service = LocationService.GetInstance();
            service.PrintMessageEvent = PrintMessage;
            service.ListeningDevice();

        }

        private void CreateLocationDB()
        {
            try
            {
                //创建名为table1的数据表
                map.locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private void InsertLocation(string name, string position)
        {
            try
            {
                //创建名为table1的数据表
                map.locationDB.InsertValues("location", new string[] { name, position });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private void ReadLocationFromDB()
        {
            txtLocation.Items.Clear();
            SQLiteDataReader reader = map.locationDB.ReadFullTable("location");
            try
            {
                while (reader.Read())
                {
                    //读取NAME与POSITION                    
                    txtLocation.Items.Add(reader.GetString(reader.GetOrdinal("NAME")) + "[" + reader.GetString(reader.GetOrdinal("POSITION")) + "]");
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                reader.Close();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                reader.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            frmMap_Load();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string locStr = txtLocation.Text;
            if (locStr.Contains("[") & locStr.Contains("]"))
            {
                int start = locStr.LastIndexOf("[");
                int end = locStr.LastIndexOf("]");
                locStr = locStr.Substring(start + 1, end - start - 1);
            }
            else if (locStr.Contains(","))
            {
                locStr = locStr.Replace(",", ":");
            }
            string[] loc = locStr.Split(new char[] { ':' });
            if (loc.Length == 2)
            {
                map.Location.Longitude = System.Convert.ToDouble(loc[0].Trim());
                map.Location.Latitude = System.Convert.ToDouble(loc[1].Trim());
                service.UpdateLocation(map.Location);
            }
            else
            {
                PrintMessage($"位置格式为：经度:纬度 或 经度,纬度，请确认格式");
            }

        }

        public void PrintMessage(string msg)
        {
            if (rtbxLog.InvokeRequired)
            {
                this.Invoke(new Action<string>(PrintMessage), msg);
            }
            else
            {
                rtbxLog.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}： {msg}\r\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            service.ClearLocation();
            MessageBox.Show("建议是重启手机即可恢复,使用一键还原可能会出现定位偏移,谢谢.");
        }



        //↑
        private void button5_Click(object sender, EventArgs e)
        {

            PrintMessage($"向↑移动.");
            do
            {
                map.Location.Latitude += speed;
                //map.Location.Longitude += 0.0005;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        // ←
        private void button3_Click(object sender, EventArgs e)
        {
            PrintMessage($"向←移动.");
            do
            {
                map.Location.Longitude -= speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }

        //↓
        private void button4_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↓移动.");
            do
            {
                map.Location.Latitude -= speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }

        //→
        private void button6_Click(object sender, EventArgs e)
        {
            PrintMessage($"向→移动.");
            do
            {
                map.Location.Longitude += speed;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }
        public void frmMap_Load()
        {
            HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.Shift, Keys.S); //注册Shift+S
            this.webBrowser1.ScriptErrorsSuppressed = true;
            var text = @"<html xmlns='http://www.w3.org/1999/xhtml\'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title>站点地图</title>
    <style type='text/css'>
        body, html, #allmap
        {
            width: 100%;
            height: 100%;
            overflow: hidden;
            margin: 0;
        }
        #l-map
        {
            height: 100%;
            width: 78%;
            float: left;
            border-right: 2px solid #bcbcbc;
        }
        #r-result
        {
            height: 100%;
            width: 20%;
            float: left;
        }
    </style>
    <script type='text/javascript' src='http://api.map.baidu.com/api?v=3.0&ak=qsgSGFbqwGGzeQZU1ahGqLN3xQaHGrHf'></script>
</head>
<body>
    <div id='allmap'>
    </div>
    <div style='float:right;position: absolute;
    top: 1px;
    left: 60px;background-color:;font-size: 12px;
    line-height: 23px;'>
        <div style='float:left'>请输入关键字搜索</div>
        <div id='r-result' style='float:left'><input type='text' id='suggestId' size='20' value=''  style='width:300px;' /></div>
        <div id='searchResultPanel' style='border:1px solid #C0C0C0;width:300px;height:auto; display:none;'></div>
    </div>
</body>
</html>
<script type='text/javascript'>
    document.oncontextmenu=new Function('event.returnValue=false;'); document.onselectstart=new Function('event.returnValue=false;'); 
    var marker;
    //alert(window.external.GetLongitude());
    //alert(window.external.GetLatitude());
    var map = new BMap.Map('allmap');               // 创建Map实例
    var point = new BMap.Point(114.381692, 30.573998);    // 创建点坐标(经度,纬度)
    //var point = new BMap.Point(window.external.GetLongitude(), window.external.GetLatitude());    // 创建点坐标(经度,纬度)
    map.centerAndZoom(point, 8);                   // 初始化地图,设置中心点坐标和地图大小级别
    //map.addOverlay(new BMap.Marker(point));         // 给该坐标加一个红点标记
    map.addControl(new BMap.NavigationControl());   // 添加平移缩放控件
    map.addControl(new BMap.ScaleControl());        // 添加比例尺控件
    map.addControl(new BMap.OverviewMapControl());  //添加缩略地图控件
    map.addControl(new BMap.MapTypeControl());      //添加地图类型控件
    map.enableScrollWheelZoom();                    //启用滚轮放大缩小
    var geoc = new BMap.Geocoder();  
    map.addEventListener('click', function (e) {
        checkMaker();
        point = new BMap.Point(e.point.lng, e.point.lat);
        marker = new BMap.Marker(point);
        map.addOverlay(marker);
        var pt = e.point;
        geoc.getLocation(pt, function (rs) {
            var addComp = rs.addressComponents;
            var address = [];
            if (addComp.province.length > 0) {
                address.push(addComp.province);
            }
            if (addComp.city.length > 0) {
                address.push(addComp.city);
            }
            if (addComp.district.length > 0) {
                address.push(addComp.district);
            }
            if (addComp.street.length > 0) {
                address.push(addComp.street);
            }
            if (addComp.streetNumber.length > 0) {
                address.push(addComp.streetNumber);
            }
            window.external.position(e.point.lat, e.point.lng, address.join(','));
        });
    });
    function G(id) {
        return document.getElementById(id);
    }
    var ac = new BMap.Autocomplete(    //建立一个自动完成的对象
       {
           'input': 'suggestId'
          , 'location': map
       });
    ac.addEventListener('onhighlight', function (e) {  //鼠标放在下拉列表上的事件
        var str = '';
        var _value = e.fromitem.value;
        var value = '';
        if (e.fromitem.index > -1) {
            value = _value.province + _value.city + _value.district + _value.street + _value.business;
        }
        str = 'FromItem<br />index = ' + e.fromitem.index + '<br />value = ' + value;
        value = '';
        if (e.toitem.index > -1) {
            _value = e.toitem.value;
            value = _value.province + _value.city + _value.district + _value.street + _value.business;
        }
        str += '<br />ToItem<br />index = ' + e.toitem.index + '<br />value = ' + value;
        G('searchResultPanel').innerHTML = str;
    });
    var myValue;
    ac.addEventListener('onconfirm', function (e) {    //鼠标点击下拉列表后的事件
        var _value = e.item.value;
        myValue = _value.province + _value.city + _value.district + _value.street + _value.business;
        G('searchResultPanel').innerHTML = 'onconfirm<br />index = ' + e.item.index + '<br />myValue = ' + myValue;
        setPlace();
    });
    function setPlace() {
        map.clearOverlays();    //清除地图上所有覆盖物
        function myFun() {
            var pp = local.getResults().getPoi(0).point;    //获取第一个智能搜索的结果
            checkMaker();
            map.centerAndZoom(pp, 15);
            marker=new BMap.Marker(pp);
            map.addOverlay(marker);    //添加标注
         var pt = pp;
            geoc.getLocation(pt, function (rs) {
                var addComp = rs.addressComponents;
                var address = [];
                if (addComp.province.length > 0) {
                    address.push(addComp.province);
                }
                if (addComp.city.length > 0) {
                    address.push(addComp.city);
                }
                if (addComp.district.length > 0) {
                    address.push(addComp.district);
                }
                if (addComp.street.length > 0) {
                    address.push(addComp.street);
                }
                if (addComp.streetNumber.length > 0) {
                    address.push(addComp.streetNumber);
                }
                window.external.position(pp.lat, pp.lng, address.join(','));
            });
        }
        var local = new BMap.LocalSearch(map, { //智能搜索
            onSearchComplete: myFun
        });
        local.search(myValue);
    }
    function checkMaker() {
        if (marker != null)
            map.removeOverlay(marker);
    };
    function evaluatepoint(log,lat){
        checkMaker();
        point = new BMap.Point(log,lat);
        map.centerAndZoom(point, 15);
        marker = new BMap.Marker(point);
        map.addOverlay(marker);
        var pt = point;
        geoc.getLocation(pt, function (rs) {
            var addComp = rs.addressComponents;
            var address = [];
            if (addComp.province.length > 0) {
                address.push(addComp.province);
            }
            if (addComp.city.length > 0) {
                address.push(addComp.city);
            }
            if (addComp.district.length > 0) {
                address.push(addComp.district);
            }
            if (addComp.street.length > 0) {
                address.push(addComp.street);
            }
            if (addComp.streetNumber.length > 0) {
                address.push(addComp.streetNumber);
            }
            window.external.position(pt.lat,pt.lng, address.join(','));
        });
    };</script>";
            this.webBrowser1.DocumentText = text;
            this.webBrowser1.ObjectForScripting = this;
        }

        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }


        public void position(string a_0, string a_1, string b_0)
        {
            PrintMessage(double.Parse(a_1) + "," + double.Parse(a_0));
            txtLocation.Items.Add(double.Parse(a_1) + "," + double.Parse(a_0));
            txtLocation.Text = double.Parse(a_1) + "," + double.Parse(a_0);
            label5.Text = b_0;
        }
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //switch (e.KeyCode)
            //{
            //    case Keys.Left:
            //    case Keys.NumPad4:
            //        button3.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.Right:
            //    case Keys.NumPad6:
            //        button6.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.Up:
            //    case Keys.NumPad8:
            //        button5.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.Down:
            //    case Keys.NumPad2:
            //        button4.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.NumPad7:
            //        button9.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.NumPad9:
            //        button8.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.NumPad1:
            //        button10.PerformClick();
            //        e.Handled = true;
            //        break;
            //    case Keys.NumPad3:
            //        button11.PerformClick();
            //        e.Handled = true;
            //        break;
            //    default:
            //        break;
            //}
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            speed = System.Convert.ToDouble(textBox1.Text);
            PrintMessage($"速度修改为：{speed}");
        }




        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
            {
                PrintMessage("开启持续移动，关闭请取消勾选!");
                keepMoving = true;
            }
            else
            {
                PrintMessage("已取消持续移动!");
                keepMoving = false;
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            String locName = Interaction.InputBox("", "输入坐标别名", "", -1, -1);
            if (locName != "")
            {
                //MessageBox.Show(locName);
                InsertLocation(locName, map.Location.Longitude + ":" + map.Location.Latitude);
                ReadLocationFromDB();
                map.ReadNameFromDB();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↖移动.");
            do
            {
                map.Location.Latitude += speed * Math.Sqrt(2) / 2;
                map.Location.Longitude -= speed * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↗移动.");
            do
            {
                map.Location.Latitude += speed * Math.Sqrt(2) / 2;
                map.Location.Longitude += speed * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↘移动.");
            do
            {
                map.Location.Latitude -= speed * Math.Sqrt(2) / 2;
                map.Location.Longitude += speed * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↙移动.");
            do
            {
                map.Location.Latitude -= speed * Math.Sqrt(2) / 2;
                map.Location.Longitude -= speed * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            PrintMessage($"❤ ❤ ❤ 哇哦居然被发现了 ❤ ❤ ❤");
            PrintMessage($"❤ ❤ ❤ 联系作者QQ694340776 ❤ ❤ ❤");
            updateForm form = new updateForm();
            form.ShowDialog();

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Text.StringBuilder p = new System.Text.StringBuilder();
            p.AppendLine(" ");
            p.AppendLine(" ");
            p.AppendLine("本软件适用于任何IOS虚拟定位，基于钉钉打卡，企业微信打卡，一起来抓妖游戏的基于修改IOS定位的软件。");
            p.AppendLine("1.首先需要电脑下载苹果爱思助手:http://url.i4.cn/faIfqyaa");
            p.AppendLine("2.启动连接手机成功即可。");
            p.AppendLine("3. 解压软件安装包，运行JocationPlus.exe");
            p.AppendLine("4.软件执行完成之后左边会有加载驱动成功提示。");
            p.AppendLine("5.右边地图点击获取坐标，点击修改定位即可。");
            p.AppendLine("6.里面有模拟走路的时时定位,只需要设置偏移量");
            p.AppendLine("注意:");
            p.AppendLine("1.软件报错，定位异常联系我:qq 694340776");
            p.AppendLine("2.游戏定位需注意:建议先改修定位成功，用手机的导航软件确认看是否定位完成，在开启游戏，防止被检查异常定位，封号(千万不要一会在武汉，一会在深圳，一会在重庆，这样一起来抓妖的游戏很容易被检测出，封号。切记)。");
            p.AppendLine("3.使用苹果8以上的手机软件的,软件报错,需要在爱思助手-更多功能-虚拟定位 随便点击一个地方修改一下虚拟定位，然后在使用就可以解决软件报错问题");
            PrintMessage(p.ToString());
        }



        private void button12_Click(object sender, EventArgs e)
        {
            new keyForm().ShowDialog();
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            ////注册热键Alt+D，Id号为102。HotKey.KeyModifiers.Alt也可以直接使用数字1来表示。
            //HotKey.RegisterHotKey(Handle, 102, HotKey.KeyModifiers.Alt, Keys.D);
            HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.None, Keys.Left);
            HotKey.RegisterHotKey(Handle, 104, HotKey.KeyModifiers.None, Keys.Right);
            HotKey.RegisterHotKey(Handle, 105, HotKey.KeyModifiers.None, Keys.Down);
            HotKey.RegisterHotKey(Handle, 106, HotKey.KeyModifiers.None, Keys.Up);
        }

        private void frmMain_Leave(object sender, EventArgs e)
        {
            //注销Id号为100的热键设定
            //HotKey.UnregisterHotKey(Handle, 100);
           
            HotKey.UnregisterHotKey(Handle, 103);
            HotKey.UnregisterHotKey(Handle, 104);
            HotKey.UnregisterHotKey(Handle, 105);
            HotKey.UnregisterHotKey(Handle, 106);
        }


 
    protected override void WndProc(ref Message m)
        {

            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                       
                        case 103:    //左
                            PrintMessage("按下的是左");           //此处填写快捷键响应代码
                        button3.PerformClick();
                        break;
                        case 104:    //右
                            PrintMessage("按下的是右");           //此处填写快捷键响应代码
                        button6.PerformClick();
                        break;
                        case 105:    //下
                            PrintMessage("按下的是Down");           //此处填写快捷键响应代码
                        button4.PerformClick();
                        break;
                        case 106:    //上
                            PrintMessage("按下的是Down");           //此处填写快捷键响应代码
                        button5.PerformClick();
                        break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void label4_Click(object sender, EventArgs e)
        {
          
        }
    }

}
