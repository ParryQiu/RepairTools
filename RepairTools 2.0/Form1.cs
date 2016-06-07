using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RepairTools_2._0;
using JCMLib;
using Dolinay;
using RegistryUtils;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Diagnostics;

namespace RepairTools_2._
{
    public partial class Form1 : Form
    {
        #region variable declare
        private NotifyIconEx notifyIcon;
        private Icon myIcon;
        string TipText = "";
        public int IsSearchAutorunFiles = 0;
        public int IsCheckRegistry = 0;
        private DriveDetector driveDetector = null;
        private RegistryMonitor registryMonitorRun;
        #endregion

        public Form1()
        {
            InitializeComponent();
            this.pBoxRegistryState.Image = RepairTools_2._0.Properties.Resources.checknot;
            this.lblRegistryState.Text = "状态未检测";
            this.btnGoFileDelete.Enabled = false;
            this.ultraProgressBar1.Maximum = 100;
            this.pboxAutorunFileState.Image = RepairTools_2._0.Properties.Resources.checknot;
            myIcon = new Icon("app.ico");
            if (notifyIcon == null)
            {
                notifyIcon = new NotifyIconEx();
                notifyIcon.Text = TipText;
                notifyIcon.Icon = this.myIcon;
                notifyIcon.Visible = true;
                notifyIcon.Click += new EventHandler(OnClickIcon);
                notifyIcon.BalloonClick += new EventHandler(OnClickBalloon);
            }
            MenuItem showMainForm = new MenuItem("显示主界面");
            MenuItem close = new MenuItem("关闭程序");
            MenuItem[] menuitems = { showMainForm, close };
            notifyIcon.ContextMenu = new ContextMenu(menuitems);
            notifyIcon.ContextMenu.MenuItems[0].Click += new EventHandler(showMainFormMethod);
            notifyIcon.ContextMenu.MenuItems[1].Click += new EventHandler(closeMainFormMethod);
            foreach (string str in searchMethod.getDrivers())
            {
                if(str!=null)
                comboBoxDrivers.Items.Add(str);
            }
            driveDetector = new DriveDetector(this);
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnRemovableDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(removeRemovableDrive);
        }

        private void showMainFormMethod(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void closeMainFormMethod(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnClickIcon(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void OnClickBalloon(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.ShowBalloon("RepairTools 安全工具", "我在这儿，单击我就能还原界面！", NotifyIconEx.NotifyInfoFlags.Info,0);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.lviewAutorun.BeginUpdate();
            int i = 0;
            int t = 0;
            this.lviewAutorun.Items.Clear();
            this.lboxExeFiles.Items.Clear();
            this.lvDeleteAutorunFiles.Items.Clear();
            for (t = 0; t <= 100; t++)
            {
                this.ultraProgressBar1.Value = t;
            }
            try
            {
                for (i = 0, t = 0; i < searchMethod.getDrivers().Length; i++)
                {
                    if (searchMethod.getDrivers()[i] != null)
                    {
                        DirectoryInfo dis = new DirectoryInfo(searchMethod.getDrivers()[i]);
                        foreach (FileInfo autorunFiles in dis.GetFiles("autorun.inf"))
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = autorunFiles.FullName;
                            lvi.ImageIndex = 0;
                            this.lviewAutorun.Items.Add(lvi);
                        }
                        foreach (FileInfo exeFiles in dis.GetFiles("*.exe"))
                        {
                            this.lboxExeFiles.Items.Add(exeFiles.FullName);
                        }
                        foreach (FileInfo exeFiles in dis.GetFiles("*.bat"))
                        {
                            this.lboxExeFiles.Items.Add(exeFiles.FullName);
                        }
                        foreach (FileInfo exeFiles in dis.GetFiles("*.cmd"))
                        {
                            this.lboxExeFiles.Items.Add(exeFiles.FullName);
                        }
                        foreach (FileInfo exeFiles in dis.GetFiles("*.js"))
                        {
                            this.lboxExeFiles.Items.Add(exeFiles.FullName);
                        }
                        foreach (FileInfo exeFiles in dis.GetFiles("*.vbs"))
                        {
                            this.lboxExeFiles.Items.Add(exeFiles.FullName);
                        }
                    }
                }
                if (this.lviewAutorun.Items.Count == 0)
                {
                    this.lboxExeFiles.Items.Clear();
                    MessageBox.Show("恭喜！你的系统未检测到病毒传染文件！", "RepairTools 病毒传染文件检测", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.pboxAutorunFileState.Image = RepairTools_2._0.Properties.Resources.empty;
                    this.btnClearAutorunFiles.Enabled = false;
                    this.btnGoFileDelete.Enabled = false;
                    ListViewItem empty = new ListViewItem();
                    empty.Text = "没有检测到autorun.inf文件！";
                    lvDeleteAutorunFiles.Items.Add(empty);
                }
                else
                {
                    this.btnClearAutorunFiles.Enabled = true;
                    this.pboxAutorunFileState.Image = RepairTools_2._0.Properties.Resources.full;
                    this.lvDeleteAutorunFiles.BeginUpdate();
                    ListViewItem[] newLvi = new ListViewItem[lviewAutorun.Items.Count];
                    int index = 0;
                    foreach (ListViewItem lvi in lviewAutorun.Items)
                    {
                        newLvi[index++] = (ListViewItem)lvi.Clone();
                    }
                    lvDeleteAutorunFiles.Items.AddRange(newLvi);
                    this.btnGoFileDelete.Enabled = true;
                    this.lvDeleteAutorunFiles.EndUpdate();
                }
                this.lviewAutorun.EndUpdate();
            }
            catch
            {

            }
            IsSearchAutorunFiles = 1;
        }

        private void lviewAutorun_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.lviewAutorun.FocusedItem.Text);
            }
            catch
            {
               
            }
        }

        private void btnCheckRegistry_Click(object sender, EventArgs e)
        {
            IsCheckRegistry = 1;
            if (RegistryOperate.registryRead() == true)
            {
                this.pBoxRegistryState.Image = RepairTools_2._0.Properties.Resources.checkok;
                this.lblRegistryState.Text = "注册表状态正常";
                this.lblRegistryState.ForeColor = Color.Green;
                this.pboxRepairHidenRegistry.Image = RepairTools_2._0.Properties.Resources.checkok;
                this.lblRepairHidenFileRegistryState.Text = "注册表状态正常";
                this.lblRepairHidenFileRegistryState.ForeColor = Color.Green;
                this.btnRepairHidenRegistry.Enabled = false;
                this.lblRepairHidenRegistryTip.Visible = false;
                if (btnEnableRegMonitor.Enabled == true)
                    notifyIcon.ShowBalloon("RepairTools 安全工具", "注册表正常，您可以打开注册表实时监控来保护关键位置！", NotifyIconEx.NotifyInfoFlags.Info, 2);

            }
            else
                if (RegistryOperate.registryRead() == false)
                {
                    this.pBoxRegistryState.Image = RepairTools_2._0.Properties.Resources.checkno;
                    this.lblRegistryState.Text = "注册表状态异常，请即时修复";
                    this.lblRegistryState.ForeColor = Color.Red;
                    this.pboxRepairHidenRegistry.Image = RepairTools_2._0.Properties.Resources.checkno;
                    this.lblRepairHidenFileRegistryState.Text = "注册表状态异常，请即时修复";
                    this.lblRepairHidenFileRegistryState.ForeColor = Color.Red;
                    this.btnRepairHidenRegistry.Enabled = true;
                    this.btnGoHiddenRegistryRepair.Enabled = true;
                }
        }

        private void btnReadImageHijack_Click(object sender, EventArgs e)
        {
            ListViewItem lvi;
            ListViewItem.ListViewSubItem lvsi;
            try
            {
                lvImageHijacks.BeginUpdate();
                lvImageHiijkClear.BeginUpdate();
                lvImageHijacks.Items.Clear();
                lvImageHiijkClear.Items.Clear();
                foreach (string str in RegistryOperate.strRegistry())
                {
                    if (RegistryOperate.checkImageValue(str))
                    {
                        RegistryKey temp = RegistryOperate.getImage().OpenSubKey(str);
                        string sourceApp = temp.GetValue("Debugger").ToString();
                        if (sourceApp == null)
                        {
                            sourceApp = temp.GetValue("debugger").ToString();
                        }
                        lvi = new ListViewItem();
                        lvsi = new ListViewItem.ListViewSubItem();
                        lvi.Text = str;
                        lvsi.Text = sourceApp;
                        lvsi.Tag = sourceApp;
                        lvi.SubItems.Add(lvsi);
                        lvImageHijacks.Items.Add(lvi);
                        lvImageHiijkClear.Items.Add((ListViewItem)lvi.Clone());
                    }
                }
                lvImageHijacks.EndUpdate();
                lvImageHiijkClear.EndUpdate();
            }
            catch
            {

            }
            if (lvImageHijacks.Items.Count == 0)
            {
                MessageBox.Show("没有检测到映像劫持项！\n如果杀毒软件还不能打开，请重新安装！", "RepairTools 映像劫持检测", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnGoImageHiijacksClear.Enabled = false;
                return;
            }
            this.btnGoImageHiijacksClear.Enabled = true;
        }

        private void btnRunSearch_Click(object sender, EventArgs e)
        {
            lvClearStartupFiles.Items.Clear();
            try
            {
                ListViewItem lvi;
                ListViewItem.ListViewSubItem lvsi;
                showSystemIcon showIcon = new showSystemIcon();
                this.lvStartupFileList.Items.Clear();
                lvStartupFileList.BeginUpdate();
                lvClearStartupFiles.BeginUpdate();
                foreach (string str in RegistryOperate.strRunRegistry())
                {
                    string str1 = RegistryOperate.getRun().GetValue(str).ToString();
                    string str2 = stringChecker.stringReplacer(str1);
                    showIcon.ListViewSysImages(lvStartupFileList);
                    lvi = new ListViewItem();
                    lvi.Text = str;
                    int systemIconIndex = showIcon.FileIconIndex(@str2);
                    if (systemIconIndex != 0)
                    {
                        lvi.ImageIndex = systemIconIndex;
                    }
                    else
                        lvi.ImageIndex = 3;
                    lvsi = new ListViewItem.ListViewSubItem();
                    lvsi.Text = str2;
                    lvi.SubItems.Add(lvsi);
                    lvStartupFileList.Items.Add(lvi);
                    lvClearStartupFiles.Items.Add((ListViewItem)lvi.Clone());
                }
                lvStartupFileList.EndUpdate();
                lvClearStartupFiles.EndUpdate();
            }
            catch
            {
                
            }
            this.btnGoStartupClear.Enabled = true;
        }

        private void btnRepairHidenRegistry_Click(object sender, EventArgs e)
        {
            if (IsCheckRegistry == 0)
            {
                MessageBox.Show("您还未检测注册表状态，请先检测！", "RepairTools 注册表修复", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                RegistryOperate.registrySetValue("CheckedValue", 1);
                this.pboxRepairHidenRegistry.Image = RepairTools_2._0.Properties.Resources.checkok;
                this.lblRepairHidenFileRegistryState.Text = "注册表修复成功！";
                this.btnRepairHidenRegistry.Enabled = false;
                this.pBoxRegistryState.Image = RepairTools_2._0.Properties.Resources.checknot;
                this.lblRegistryState.Text = "状态未检测";
                notifyIcon.ShowBalloon("RepairTools 安全工具", "修复注册表后，您可以打开注册表实时监控来保护关键位置！", NotifyIconEx.NotifyInfoFlags.Info, 2);

            }
        }

        private void btnGoFileDelete_Click(object sender, EventArgs e)
        {
            this.ultraTabControl1.Tabs[1].Selected = true;
            this.ultraTabControl3.Tabs[0].Selected = true;
        }

        private void btnSaveRunState_Click(object sender, EventArgs e)
        {
            savefdStartupState.ShowDialog();

            try
            {
                StreamWriter sw = new StreamWriter(savefdStartupState.FileName);
                sw.WriteLine(DateTime.Now.ToLongDateString());
                foreach (ListViewItem lvi in lvStartupFileList.Items)
                {
                    string strWriter = lvi.Text + "               " + lvi.SubItems[1].Text;
                    sw.WriteLine(strWriter);
                }
                sw.Close();
            }
            catch
            {

            }
        }

        private void linkLblSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvDeleteAutorunFiles.Items)
            {
                lvi.Checked = true;
            }
        }

        private void linkLblNotSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvDeleteAutorunFiles.Items)
            {
                lvi.Checked = false;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnClearAutorunFiles_Click(object sender, EventArgs e)
        {
            if (IsSearchAutorunFiles == 0)
            {
                MessageBox.Show("您还未检测病毒感染文件状态，请先检测！", "RepairTools 病毒传染文件清理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (this.lviewAutorun.Items.Count > 0)
            {
                if (lvDeleteAutorunFiles.CheckedItems.Count == 0)
                {
                    MessageBox.Show("您未选定任何文件，无法执行清理工作！", "RepairTools 病毒传染文件清理", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    DialogResult response = MessageBox.Show("确定要清除选定的文件？", "RepairTools 文件清理", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (response == DialogResult.OK)
                    {
                        foreach (ListViewItem lvi in lvDeleteAutorunFiles.CheckedItems)
                        {
                            FileInfo file = new FileInfo(lvi.Text);
                            file.Attributes = FileAttributes.Normal;
                            File.Delete(lvi.Text);
                            lvi.Remove();
                        }
                        if (lvDeleteAutorunFiles.Items.Count == 0)
                        {
                            pboxAutorunFileState.Image = RepairTools_2._0.Properties.Resources.empty;
                        }
                        lviewAutorun.Items.Clear();
                        this.ultraProgressBar1.Value = 0;
                        MessageBox.Show("已成功清除您选定的文件！", "RepairTools 文件清理", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void savefdStartupState_FileOk(object sender, CancelEventArgs e)
        {
            MessageBox.Show("启动文件列表保存成功！", "RepairTools 启动项扫描", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLblSelectAllImageHiijk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvImageHiijkClear.Items)
            {
                lvi.Checked = true;
            }
        }

        private void linkLblNotSelectAllImageHiijk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvImageHiijkClear.Items)
            {
                lvi.Checked = false;
            }
        }

        private void btnImageHiijkClear_Click(object sender, EventArgs e)
        {
            if (lvImageHiijkClear.Items.Count == 0)
            {
                MessageBox.Show("您还未进行映像劫持状态扫描，请先扫描状态！", "RepairTools 映像劫持修复", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (lvImageHiijkClear.CheckedItems.Count == 0)
                {
                    MessageBox.Show("您未选定任何映像劫持项，无法执行修复工作", "RepairTools 映像劫持修复", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    DialogResult response = MessageBox.Show("确定要清除选定的项目？", "RepairTools 映像劫持修复", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    {
                        if (response == DialogResult.OK)
                        {
                            foreach (ListViewItem lvi in lvImageHiijkClear.CheckedItems)
                            {
                                RegistryOperate.deleteSubKey(lvi.Text);
                                lvi.Remove();
                            }
                            lvImageHijacks.Items.Clear();
                            MessageBox.Show("已成功修复您选定的映像劫持项！", "RepairTools 映像劫持修复", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void btnGoImageHiijacksClear_Click(object sender, EventArgs e)
        {
            this.ultraTabControl1.Tabs[1].Selected = true;
            this.ultraTabControl3.Tabs[2].Selected = true;
        }

        private void btnGoHiddenRegistryRepair_Click(object sender, EventArgs e)
        {
            this.ultraTabControl1.Tabs[1].Selected = true;
            this.ultraTabControl3.Tabs[1].Selected = true;
        }

        private void btnGoStartupClear_Click(object sender, EventArgs e)
        {
            this.ultraTabControl1.Tabs[1].Selected = true;
            this.ultraTabControl3.Tabs[3].Selected = true;
        }

        private void linkLblSelectAllStartupFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvClearStartupFiles.Items)
            {
                lvi.Checked = true;
            }
        }

        private void linkLblNotSelectAllStartupFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in lvClearStartupFiles.Items)
            {
                lvi.Checked = false;
            }
        }

        private void btnClearStartupFiles_Click(object sender, EventArgs e)
        {
            if (lvClearStartupFiles.Items.Count == 0)
            {
                MessageBox.Show("您还未进行启动项扫描，请先扫描！", "RepairTools 启动项修复", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (lvClearStartupFiles.CheckedItems.Count == 0)
                {
                    MessageBox.Show("您未选定任何启动项，无法执行修复工作", "RepairTools 启动项修复", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
                else
                {
                    DialogResult response = MessageBox.Show("确定清除选定的启动项？", "RepairTools 启动项修复", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    {
                        if (response == DialogResult.OK)
                        {
                            foreach (ListViewItem lvi in lvClearStartupFiles.CheckedItems)
                            {
                                RegistryOperate.deleteStartupFiles(lvi.Text);
                                lvi.Remove();
                            }
                            MessageBox.Show("已成功修复您选定的启动项！", "RepairTools 启动项修复", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            lvStartupFileList.Items.Clear();
                        }
                    }
                }
            }
        }

        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            string s = e.Drive;
            string filePath = s + "autorun.inf";
            FileInfo test = new FileInfo(filePath);
            if (test.Exists)
            {
                notifyIcon.ShowBalloon("RepairTools 安全工具", "检测到" + filePath + "存在，请先不要打开此移动设备！\n建议立即执行【病毒疫情扫描】清除感染文件！", NotifyIconEx.NotifyInfoFlags.Error, 2);
                new USBMointorOperate().Show();
            }
            else
            {
                notifyIcon.ShowBalloon("RepairTools 安全工具", s + "没有检测到病毒传染文件，您可以放心使用此设备！", NotifyIconEx.NotifyInfoFlags.Info, 2);
            }
        }

        private void OnRemovableDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            string s = e.Drive;
            comboBoxDrivers.Items.Add(s);
        }

        private void removeRemovableDrive(object sender, DriveDetectorEventArgs e)
        {
            string s = e.Drive;
            comboBoxDrivers.Items.Remove(s);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (driveDetector != null)
            {
                driveDetector.WndProc(ref m);
            }
        }

        private void btnEnableUSBMonitor_Click(object sender, EventArgs e)
        {
            pboxUSB.Image = RepairTools_2._0.Properties.Resources.usb;
            btnEnableUSBMonitor.Enabled = false;
            btnDisableUSBMonitor.Enabled = true;
            notifyIcon.ShowBalloon("RepairTools 安全工具", "您开启了移动设备病毒文件监控，实时保护您的计算机安全！", NotifyIconEx.NotifyInfoFlags.Info, 2);
            driveDetector = new DriveDetector(this);
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
        }

        private void btnDisableUSBMonitor_Click(object sender, EventArgs e)
        {
            pboxUSB.Image = RepairTools_2._0.Properties.Resources.BW_usb;
            btnEnableUSBMonitor.Enabled = true;
            btnDisableUSBMonitor.Enabled = false;
            notifyIcon.ShowBalloon("RepairTools 安全工具", "您关闭了移动设备病毒文件监控，外来移动设备可能会危害您的计算机！", NotifyIconEx.NotifyInfoFlags.Error, 2);
            driveDetector = null;
        }

        private void btnEnableRegMonitor_Click(object sender, EventArgs e)
        {
            pboxRegMonitor.Image = RepairTools_2._0.Properties.Resources.reg;
            btnEnableRegMonitor.Enabled = false;
            btnDisableRegMonitor.Enabled = true;
            notifyIcon.ShowBalloon("RepairTools 安全工具", "您开启了关键位置注册表实时监控，实时保护您的计算机安全！", NotifyIconEx.NotifyInfoFlags.Info, 2);

            registryMonitorRun = new RegistryMonitor(RegistryOperate.getRun());
            registryMonitorRun.RegChanged += new EventHandler(OnRegChanged);
            registryMonitorRun.Error += new System.IO.ErrorEventHandler(OnError);
            registryMonitorRun.Start();
        }

        private void btnDisableRegMonitor_Click(object sender, EventArgs e)
        {
            pboxRegMonitor.Image = RepairTools_2._0.Properties.Resources.BW_reg;
            btnEnableRegMonitor.Enabled = true;
            btnDisableRegMonitor.Enabled = false;
            notifyIcon.ShowBalloon("RepairTools 安全工具", "您关闭了关键位置注册表实时监控，病毒可能会危害您的计算机安全！", NotifyIconEx.NotifyInfoFlags.Error, 2);
        }

        public void StopRegistryMonitor()
        {
            if (registryMonitorRun != null)
            {
                registryMonitorRun.Stop();
                registryMonitorRun.RegChanged -= new EventHandler(OnRegChanged);
                registryMonitorRun.Error -= new System.IO.ErrorEventHandler(OnError);
                registryMonitorRun = null;
            }
        }

        public void OnRegChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(OnRegChanged), new object[] { sender, e });
                return;
            }
            notifyIcon.ShowBalloon("RepairTools 安全工具", "注册表中添加了未知的启动项，请立即执行【启动项扫描】进行清理！", NotifyIconEx.NotifyInfoFlags.Error, 2);

        }

        public void OnError(object sender, ErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ErrorEventHandler(OnError), new object[] { sender, e });
                return;
            }
            StopRegistryMonitor();
        }

        public static int getRemovableNO()
        {
            int i = 0;
            foreach (string letter in searchMethod.getRemovableDrivers())
            {
                if (letter != null)
                {
                    i++;
                }
            }
            return i;
        }

        private void btnGetRemovableDrivers_Click(object sender, EventArgs e)
        {
            lvArrestVirusFiles.Items.Clear();
            string existFolder = "";
            ListViewItem lvi;
            ListViewItem.ListViewSubItem lvsi;
            if (getRemovableNO() == 0)
            {
                lvi = new ListViewItem();
                lvi.Text = "没有移动设备";
                lvArrestVirusFiles.Items.Add(lvi);
            }
            else
            {
                foreach (string letter in searchMethod.getRemovableDrivers())
                {
                    if (letter != null)
                    {
                        DirectoryInfo dir = new DirectoryInfo(letter + "autorun.inf");
                        if (dir.Exists)
                        {
                            existFolder = "已免疫";
                        }
                        else
                        {
                            existFolder = "未免疫";
                        }
                        lvi = new ListViewItem();
                        lvsi = new ListViewItem.ListViewSubItem();
                        lvi.Text = letter;
                        lvsi.Text = existFolder;
                        lvi.SubItems.Add(lvsi);
                        lvArrestVirusFiles.Items.Add(lvi);
                    }
                }
            }
        }

        private void btnEnableArrestVirus_Click(object sender, EventArgs e)
        {
            string path = "";
            if (lvArrestVirusFiles.CheckedItems.Count == 0)
            {
                MessageBox.Show("您没有选定任何项目，无法完成处理！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {
                if (lvArrestVirusFiles.Items[0].Text == "没有移动设备")
                {
                    MessageBox.Show("没有移动设备连接，请先连接您的移动设备！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    foreach (ListViewItem lvi in lvArrestVirusFiles.CheckedItems)
                    {
                        if (lvi.SubItems[1].Text == "已免疫")
                        {
                            MessageBox.Show("您选择的移动设备" + lvi.Text + "已经存在免疫文件，不需要再免疫！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            try
                                {
                                    DirectoryInfo dir = Directory.CreateDirectory(lvi.Text + "autorun.inf");
                                    File.SetAttributes(lvi.Text + "autorun.inf", FileAttributes.Hidden);
                                    lvi.SubItems[1].Text = "已免疫";
                                    path += lvi.Text;
                                }
                                catch
                                {

                                }
                        }
                    }
                    if(path != "")
                    {
                        MessageBox.Show("移动设备" + path + "免疫成功！您的设备将摆脱蠕虫病毒的侵害！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnDisableArrestVirus_Click(object sender, EventArgs e)
        {
            string path = "";
            if (lvArrestVirusFiles.CheckedItems.Count == 0)
            {
                MessageBox.Show("您没有选定任何项目，无法完成处理！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (lvArrestVirusFiles.Items[0].Text == "没有移动设备")
                    {
                        MessageBox.Show("没有移动设备连接，请先连接您的移动设备！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                else
                {
                    foreach (ListViewItem lvi in lvArrestVirusFiles.CheckedItems)
                        {
                            if (lvi.SubItems[1].Text == "未免疫")
                            {
                                MessageBox.Show("您选择的移动设备" + lvi.Text + "不存在免疫文件，无法进行取消免疫操作！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        else
                        {
                            try
                            {
                                DirectoryInfo dir = new DirectoryInfo(lvi.Text + "autorun.inf");
                                File.SetAttributes(lvi.Text + "autorun.inf", FileAttributes.Normal);
                                Directory.Delete(dir.ToString(), true);
                                lvi.SubItems[1].Text = "未免疫";
                                path += lvi.Text;
                            }
                            catch
                            {

                            }
                        }
                    }
                    if (path != "")
                    {
                        MessageBox.Show("移动设备" + path + "取消免疫成功，这样您的移动设备会很容易感染蠕虫病毒！", "RepairTools 移动设备病毒免疫", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        notifyIcon.ShowBalloon("RepairTools 安全工具", "移动设备" + path + "取消免疫成功，这样您的移动设备会很容易感染蠕虫病毒！", NotifyIconEx.NotifyInfoFlags.Error, 2);
                    }
                }
            }
        }

        private void btnSelectPic_Click(object sender, EventArgs e)
        {
            openFileDialog_SelectPic.ShowDialog();
            tboxPicPath.Text = openFileDialog_SelectPic.FileName;
        }

        private void btnUsingPic_Click(object sender, EventArgs e)
        {
            string iniFileContent=@"[ExtShellFolderViews]
                                    {BE098140-A513-11D0-A3A4-00C04FD706EC}={BE098140-A513-11D0-A3A4-00C04FD706EC}
                                    [{BE098140-A513-11D0-A3A4-00C04FD706EC}]
                                    Attributes=1
                                    IconArea_Image=backgroundPic\bg.jpg
                                    IconArea_Text=0x00000000";
            if (comboBoxDrivers.SelectedItem == null && tboxPicPath.Text == "")
            {
                MessageBox.Show("盘符和图片路径未指定,请先选择盘符和图片路径！", "RepairTools 分区背景添加", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (comboBoxDrivers.SelectedItem == null)
            {
                MessageBox.Show("盘符未指定，请先选择盘符！", "RepairTools 分区背景添加", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tboxPicPath.Text == "")
            {
                MessageBox.Show("图片路径未指定,请先选择图片路径！", "RepairTools 分区背景添加", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    string drivePath = comboBoxDrivers.SelectedItem.ToString();
                    Directory.CreateDirectory(drivePath + "backgroundPic");
                    File.Copy(tboxPicPath.Text, drivePath + "backgroundPic\\bg.jpg", true);
                    if (File.Exists(drivePath + "desktop.ini"))
                        File.Delete(drivePath + "desktop.ini");
                    StreamWriter sw = new StreamWriter(drivePath + "desktop.ini");
                    sw.Write(iniFileContent);
                    sw.Close();
                    File.SetAttributes(drivePath + "backgroundPic", FileAttributes.Hidden);
                    File.SetAttributes(drivePath + "desktop.ini", FileAttributes.Hidden);
                    MessageBox.Show("分区背景添加成功，进入分区刷新就可以看到背景！", "RepairTools 分区背景添加", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
 
                }
            }
        }

        private void btnDisUsingPic_Click(object sender, EventArgs e)
        {
            if (comboBoxDrivers.SelectedItem == null)
            {
                MessageBox.Show("盘符未指定，请先选择盘符！", "RepairTools 分区背景清除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string drivePath = comboBoxDrivers.SelectedItem.ToString();
                if (!Directory.Exists(drivePath + "backgroundPic"))
                {
                    MessageBox.Show("您选定的盘符没有添加背景，不能完成背景清除工作！", "RepairTools 分区背景清除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    File.SetAttributes(drivePath + "backgroundPic", FileAttributes.Normal);
                    File.SetAttributes(drivePath + "desktop.ini", FileAttributes.Normal);
                    Directory.Delete(drivePath + "backgroundPic", true);
                    File.Delete(drivePath + "desktop.ini");
                    MessageBox.Show("分区背景清除成功！", "RepairTools 分区背景添加", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnINUsingUSB_Click(object sender, EventArgs e)
        {
            int i = 0;
            try
            {
                for (i = 0; i < searchMethod.getDrivers().Length; i++)
                {
                    if (searchMethod.getDrivers()[i] != null)
                    {
                        DirectoryInfo dis = new DirectoryInfo(searchMethod.getDrivers()[i]);
                        foreach (FileInfo autorunFiles in dis.GetFiles("autorun.inf"))
                        {
                            autorunFiles.Attributes = FileAttributes.Normal;
                            autorunFiles.Delete();
                        }
                    }
                }
                Process[] p = Process.GetProcessesByName("explorer");
                p[0].Kill();
                MessageBox.Show("清理成功，您现在可以双击打开移动设备了！", "RepairTools 移动设备立即使用", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
 
            }
        }

        private void btnAddAutorun_Click(object sender, EventArgs e)
        {
            RegistryOperate.setRunKey(Application.StartupPath + "\\RepairTools 2.0.exe");
            MessageBox.Show("添加启动项成功，软件在您下次启动机器时自动运行！", "RepairTools 安全工具", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnNotAddAutorun_Click(object sender, EventArgs e)
        {
            RegistryOperate.deleteRunKey();
            MessageBox.Show("删除启动项成功，软件在您下次开机时不再自动运行！", "RepairTools 安全工具", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}