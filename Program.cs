using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeMaven
{
    class Program
    {
        /// <summary>
        /// 1.在可连网机器上打包apk
        /// 如果Unity版本不一致，可修改Editor\Data\PlaybackEngines\AndroidPlayer\Tools\GradleTemplates\baseProjectTemplate.gradle
        /// 把classpath 'com.android.tools.build:gradle:3.6.0'改成目标版本号
        /// 2.执行MakeMaven
        /// 源目录：C:\Users\UserName\.gradle\caches\modules-2\files-2.1
        /// 目标目录：离线目录（自己创建，任一位置）
        /// 3.使用离线目录（假设离线目录为D:/mavens/）
        /// 修改
        /// repositories {**ARTIFACTORYREPOSITORY**
        ///     google()
        ///     jcenter()
        /// }
        /// 为
        /// repositories {**ARTIFACTORYREPOSITORY**
        ///     maven { url 'file:///D:/mavens/'}
        ///     //google()
        ///     //jcenter()
        /// }
        /// </summary>
        /// 后面的google()和jcenter()也改一下
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length < 2) {
                Console.WriteLine("Usage:MakeMavens <source dir> <dest dir>");
                return;
            }

            string srcDir = args[0];
            string dstDir = args[1];
            if (!srcDir.EndsWith("\\"))
                srcDir = srcDir + "\\";

            List<FileInfo> fileList = new List<FileInfo>();
            getAllFiles(ref fileList, srcDir);

            List<string> keepDir = new List<string>();
            keepDir.Add("javax.activation-api");

            foreach (FileInfo fi in fileList) {
                string manvenDir = ParseToMavenDir(srcDir, fi.FullName, keepDir);
                manvenDir = Path.Combine(dstDir, manvenDir);
                if (!Directory.Exists(manvenDir))
                    MakeDir(manvenDir);
                string dstFilename = Path.Combine(dstDir, manvenDir, fi.Name);
                if (!File.Exists(dstFilename))
                    File.Copy(fi.FullName, dstFilename);
            }
        }

        private static string ParseToMavenDir(string dirName, string fileName, List<string> keepDir)
        {
            // 暂时改成特殊字符
            bool needChange = false;
            for (int i = 0; i < keepDir.Count; i++) {
                if(fileName.Contains(keepDir[i])) {
                    needChange = true;
                    fileName = fileName.Replace(keepDir[i], keepDir[i].Replace('.', '#'));
                }
            }

            string relDirName0 = fileName.Substring(dirName.Length);
            int i2 = relDirName0.LastIndexOf('\\');
            int i1 = relDirName0.LastIndexOf('\\', i2 - 1);
            string relDirName = relDirName0.Substring(0, i1);
            int fl = relDirName.IndexOf('\\');
            string libDirName = relDirName.Substring(0, fl);
            string newLibDirName = libDirName.Replace('.', '\\');
            relDirName = relDirName.Replace(libDirName, newLibDirName);

            // 改回原.字符
            if (needChange) {
                for (int i = 0; i < keepDir.Count; i++) {
                    relDirName = relDirName.Replace(keepDir[i].Replace('.', '#'), keepDir[i]);
                }
            }

            return relDirName;
        }

        private static void MakeDir(string manvenDir)
        {
            Directory.CreateDirectory(manvenDir);
        }

        private static void getAllFiles(ref List<FileInfo> fileList, string dir)
        {
            DirectoryInfo root = new DirectoryInfo(dir);
            FileInfo[] files = root.GetFiles();
            foreach (FileInfo fi in files) {
                fileList.Add(fi);
            }
            foreach (DirectoryInfo di in root.GetDirectories()) {
                getAllFiles(ref fileList, di.FullName);
            }
        }
    }
}
