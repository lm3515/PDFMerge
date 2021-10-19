using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrintPDFs.PDFFuns;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace PrintPDFs
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShowData showDataUsed = new ShowData();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = showDataUsed;
        }

        // 打开文件夹
        private void OpenFolderButtonClick(object sender, RoutedEventArgs e)
        {
            showDataUsed.RootFolder = "";

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "选择ArcCheck验证文件存放主目录";
            folderBrowser.ShowNewFolderButton = false;

            folderBrowser.ShowDialog();
            
            showDataUsed.RootFolder = folderBrowser.SelectedPath;

            if (showDataUsed.RootFolder == "")
            {
                MessageBox.Show("请选择一个有效的文件夹", "Warning");
                showDataUsed.AddLog($"{folderBrowser.SelectedPath} 不是一个有效的文件夹");
            }
            else
            {
                showDataUsed.AddLog($"{folderBrowser.SelectedPath} 是当前选定的ArcCheck验证文件存放主目录");
            }
        }

        // 开始合并操作
        private void DoCombineButtonClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(showDataUsed.RootFolder))
            {
                MessageBox.Show("请选择一个有效的文件夹", "Warning");
                showDataUsed.AddLog($"{showDataUsed.RootFolder} 不是一个有效的文件夹");
                return;
            }

            List<string> leafDir = getLeafDirList(showDataUsed.RootFolder);
            var pdfEmptyLeaf = leafDir.Where(p => !isSpecifiedFileExisit(p, ".pdf"));
            var emptyLeaf = pdfEmptyLeaf.ToList();
            if (emptyLeaf.Any())
            {
                showDataUsed.AddLog($"有 {emptyLeaf.Count()} 个文件夹没有PDF文件，需要进行检查，列表如下");
                emptyLeaf.ForEach(p => showDataUsed.AddLog(p));
            }
            else
            {
                showDataUsed.AddLog($"未发现没有PDF文件的文件夹");
            }

            List<string> allPdfFilePaths = getFilePaths(showDataUsed.RootFolder, ".pdf");

            // 显示找到的PDF文件
            showDataUsed.AddLog($"找到 {allPdfFilePaths.Count} 个有效的PDF文件，列表如下");
            allPdfFilePaths.ForEach(p => showDataUsed.AddLog(p));

            try
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.Description = "选择合并的PDF存放主目录";
                folderBrowser.ShowNewFolderButton = true;

                folderBrowser.ShowDialog();

                if (Directory.Exists(folderBrowser.SelectedPath))
                {
                    showDataUsed.AddLog($"选择输出的文件夹为 {folderBrowser.SelectedPath}");
                    showDataUsed.AddLog($"开始进行合并操作");

                    string outputPath = Path.Combine(folderBrowser.SelectedPath,
                        $"Merge_{allPdfFilePaths.Count}_Count_{DateTime.Today.Millisecond}.pdf");

                    MergePdfs.MergePdfFun(
                        outputPath,
                        allPdfFilePaths);

                    showDataUsed.AddLog($"输出到 {outputPath} 成功");

                    showDataUsed.AddLog("==================================");
                }
                else
                {
                    showDataUsed.AddLog("选择一个有效的输出文件夹，{folderBrowser.SelectedPath} 无效");
                }
            }
            catch (Exception ee)
            {
                showDataUsed.AddLog("发生意料之外错误，错误代码为");
                showDataUsed.AddLog($"{ee.ToString()}");
            }
        }

        // 获取当前目录下面的所有叶子目录（没有子文件夹的目录）
        private List<string> getLeafDirList(string workDir)
        {
            List<string> leafs = new List<string>();
            DirectoryInfo di = new DirectoryInfo(workDir);
            if (!di.Exists)
            {
                return leafs;
            }

            var currentDirSubDirs = di.GetDirectories().ToList(); //获取子目录

            if (currentDirSubDirs.Count == 0) // 没有子目录就是最深的路径
            {
                leafs.Add(workDir);
                return leafs;
            }
            else // 递归深挖
            {
                foreach (DirectoryInfo currentDirSubDir in currentDirSubDirs)
                {
                    leafs.AddRange(getLeafDirList(currentDirSubDir.FullName));
                }

                return leafs;
            }
        }

        // 获取当前目录下面的所有某一类型的文件
        private List<string> getFilePaths(string workDir, string fileExt = ".pdf")
        {
            List<string> specifiedList = new List<string>();

            DirectoryInfo di = new DirectoryInfo(workDir);
            if (!di.Exists)
            {
                return specifiedList;
            }

            var currentDirFiles = di.GetFiles().Where(p => (new FileInfo(p.FullName)).Extension.ToLower().Equals(fileExt));
            specifiedList.AddRange(currentDirFiles.Select(p=>p.FullName));

            var currentDirSubDirs = di.GetDirectories().ToList();//获取子目录
            currentDirSubDirs.ForEach(p => specifiedList.AddRange(getFilePaths(p.FullName, fileExt)));

            return specifiedList;
        }

        // 判断某一路径下是否有某一类型的文件，不遍历子文件夹
        private bool isSpecifiedFileExisit(string workDir, string fileExt = ".pdf")
        {
            DirectoryInfo di = new DirectoryInfo(workDir);
            if (!di.Exists)
            {
                return false;
            }

            var currentDirFiles = di.GetFiles().Where(p => (new FileInfo(p.FullName)).Extension.ToLower().Equals(fileExt));
            if (!currentDirFiles.Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            showDataUsed.LogInfo = $"Welcome ~ ~{Environment.NewLine}" +
                                   $"==================================" +
                                   $"{Environment.NewLine}";
        }
    }

    public class ShowData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        // 选择的根目录
        private string _rootFolder = "";

        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (Directory.Exists(value))
                {
                    _rootFolder = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("RootFolder"));
                }
                else // 不做改变
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("RootFolder"));
                }
            }
        }

        // 选择的输出目录
        private string _outputFolder = "";

        public string OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                if (Directory.Exists(value))
                {
                    _outputFolder = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("OutputFolder"));
                }
                else // 不做改变
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("OutputFolder"));
                }
            }
        }

        // 程序运行过程中的提醒消息
        private string _logInfo = $"Welcome ~ ~{Environment.NewLine}" +
                                  $"==================================" +
                                  $"{Environment.NewLine}";

        public string LogInfo
        {
            get { return _logInfo; }
            set
            {
                _logInfo = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("LogInfo"));
            }
        }

        // 提醒消息添加
        public int AddLog(string logInfoToAdd)
        {
            LogInfo = LogInfo + (logInfoToAdd + Environment.NewLine);

            return 0;
        }
    }
}
