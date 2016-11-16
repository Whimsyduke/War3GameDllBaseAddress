using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace War3GameDllBaseAddress
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private string GameAddressReplace(Match func)
        {
            string str = func.ToString().Substring(TextBox_Name.Text.Length + 1);
            int address = Int32.Parse(str, System.Globalization.NumberStyles.HexNumber) - baseAddress;
            StringBuilder sb = new StringBuilder();
            sb.Append(TextBox_Name.Text + ".");
            sb.AppendFormat("{0:X8}", address);
            return sb.ToString();
        }

        private int baseAddress = 0;
        private Regex addressRegex = new Regex("[0123456789ABCDEF]{8}");

        private void Button_Gen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Regex regex = null;
                bool isModifyFunc = false;
                if (!string.IsNullOrEmpty(TextBox_Name.Text))
                {
                    regex = new Regex(TextBox_Name.Text + ".[0123456789ABCDEF]{8}");
                    isModifyFunc = true;
                }
                FileInfo file;
                if (File.Exists(TextBox_Path.Text))
                {
                    file = new FileInfo(TextBox_Path.Text);
                }
                else
                {
                    MessageBox.Show("文件地址：" + TextBox_Path.Text + "无效");
                    return;
                }
                baseAddress = 0;
                if (TextBox_Path.Text == TextBox_Output.Text)
                {
                    StreamReader sr = new StreamReader(file.FullName);
                    string output = "";
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (CheckBox_DeleteNotDllText.IsChecked == true && !line.Contains(TextBox_Name.Text)) continue;
                        if (line.Length < 8)
                        {
                            output += line;
                            continue;
                        }
                        string addressStr = line.Substring(0, 8);
                        if (addressRegex.IsMatch(addressStr))
                        {
                            int address = Int32.Parse(addressStr, System.Globalization.NumberStyles.HexNumber);
                            line = line.Substring(8);
                            if (baseAddress == 0)
                            {
                                if (CheckBox_UseSepcialBaseAddress.IsChecked == true)
                                {
                                    baseAddress = Int32.Parse(TextBox_BaseAddress.Text, System.Globalization.NumberStyles.HexNumber) << 16;
                                    address = address - baseAddress;
                                }
                                else
                                {
                                    baseAddress = address - 0x1000;
                                    address = 0x1000;
                                }
                            }
                            else
                            {
                                address = address - baseAddress;
                            }
                            if (isModifyFunc)
                            {
                                line = regex.Replace(line, new MatchEvaluator(GameAddressReplace));
                            }
                            output += address.ToString("{0:X8}") + line;
                        }
                        else
                        {
                            output += line;
                        }
                    }
                    sr.Close();
                    if (File.Exists(TextBox_Output.Text))
                    {
                        if (MessageBox.Show("是否删除已经存在的" + TextBox_Output.Text + "文件", "确认删除", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            File.Delete(TextBox_Output.Text);
                        }
                        else
                        {
                            return;
                        }
                    }
                    StreamWriter sw = new StreamWriter(TextBox_Output.Text);
                    sw.Write(output);
                    sw.Close();
                }
                else
                {
                    if (File.Exists(TextBox_Output.Text))
                    {
                        if (MessageBox.Show("是否删除已经存在的" + TextBox_Output.Text + "文件", "确认删除", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            File.Delete(TextBox_Output.Text);
                        }
                        else
                        {
                            return;
                        }
                    }
                    StreamReader sr = new StreamReader(file.FullName);
                    StreamWriter sw = new StreamWriter(TextBox_Output.Text);
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (CheckBox_DeleteNotDllText.IsChecked == true && !line.Contains(TextBox_Name.Text)) continue;
                        if (line.Length < 8)
                        {
                            sw.WriteLine(line);
                            continue;
                        }
                        string addressStr = line.Substring(0, 8);
                        if (addressRegex.IsMatch(addressStr))
                        {
                            int address = Int32.Parse(line.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
                            line = line.Substring(8);
                            if (baseAddress == 0)
                            {
                                if (CheckBox_UseSepcialBaseAddress.IsChecked == true)
                                {
                                    baseAddress = Int32.Parse(TextBox_BaseAddress.Text, System.Globalization.NumberStyles.HexNumber) << 16;
                                    address = address - baseAddress;
                                }
                                else
                                {
                                    baseAddress = address - 0x1000;
                                    address = 0x1000;
                                }
                            }
                            else
                            {
                                address = address - baseAddress;
                            }
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0:X8}", address);
                            if (isModifyFunc)
                            {
                                line = regex.Replace(line, new MatchEvaluator(GameAddressReplace));
                            }
                            sw.WriteLine(sb.ToString() + line);
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    sr.Close();
                    sw.Close();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return;
            }
            MessageBox.Show("生成完成！");
        }

        private void Button_Path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            if (File.Exists(TextBox_Path.Text))
            {
                fileDialog.InitialDirectory = new FileInfo(TextBox_Path.Text).DirectoryName;
            }
            else
            {
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            fileDialog.Filter = "文本文件|*.txt";
            fileDialog.Multiselect = false;
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = "选择Game.dll汇编代码文件";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_Path.Text = fileDialog.FileName;
            }
            else
            {
                TextBox_Path.Text = "";
            }
        }

        private void Button_Output_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            FileInfo saveFile = null;
            bool isPathOK = true;
            try
            {
                saveFile = new FileInfo(TextBox_Output.Text);
            }
            catch
            {
                isPathOK = false;
            }
            if (isPathOK)
            {
                fileDialog.InitialDirectory = saveFile.DirectoryName;
            }
            else
            {
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            fileDialog.Filter = "文本文件|*.txt";
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = "选择输出基地址版本Game.dll汇编代码文件";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_Output.Text = fileDialog.FileName;
            }
            else
            {
                TextBox_Output.Text = "";
            }
        }
    }
}
