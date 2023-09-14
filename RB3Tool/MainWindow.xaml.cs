using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System;
using RB3Tool.x360;
using Xceed.Wpf.Toolkit;

namespace RB3Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		List<int> charBasesA = new List<int>();
		List<int> charBasesB = new List<int>();

		public MainWindow()
		{
			InitializeComponent();

			charBasesA.Add(0x4B21B);
			charBasesB.Add(0x4B21B);
			charBasesA.Add(0x6C111);
			charBasesB.Add(0x6C111);
			charBasesA.Add(0x8D007);
			charBasesB.Add(0x8D007);
			charBasesA.Add(0xADEFD);
			charBasesB.Add(0xADEFD);
			charBasesA.Add(0xCEDF3);
			charBasesB.Add(0xCEDF3);
			charBasesA.Add(0xEFCE9);
			charBasesB.Add(0xEFCE9);
			charBasesA.Add(0x110BDF);
			charBasesB.Add(0x110BDF);
			charBasesA.Add(0x131AD5);
			charBasesB.Add(0x131AD5);
			charBasesA.Add(0x1529CB);
			charBasesB.Add(0x1529CB);
			charBasesA.Add(0x1738C1);
			charBasesB.Add(0x1738C1);
		}

		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			ProcessSaveFile(files[0]);
		}

        /// <summary>
        /// Signs STFS file as CON for use in retail Xboxes
        /// </summary>
        /// <param name="file_path">Full path to the STFS file to sign</param>
        /// <returns></returns>
        public bool SignSave(string file_path)
        {
            var backup = file_path + "_backup";
            DeleteFile(backup);
            File.Copy(file_path, backup);

            var xPackage = new STFSPackage(file_path);
            if (!xPackage.ParseSuccess)
            {
                DeleteFile(backup);
                return false;
            }
            try
            {               
                var kv = new RSAParams(AppDomain.CurrentDomain.BaseDirectory + "KV.bin");
                if (kv.Valid)
                {
                    xPackage.FlushPackage(kv);
                    xPackage.UpdateHeader(kv);
                    xPackage.CloseIO();
                    DeleteFile(backup);
                    return true;
                }
                xPackage.CloseIO();
                MoveFile(backup, file_path);                
                return false;
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show(ex.Message);
                xPackage.CloseIO();
                return false;
            }
        }

        /// <summary>
        /// Will safely try to move, and if fails, copy/delete a file
        /// </summary>
        /// <param name="input">Full starting path of the file</param>
        /// <param name="output">Full destination path of the file</param>
        public bool MoveFile(string input, string output)
        {
            try
            {
                DeleteFile(output);
                File.Move(input, output);
            }
            catch (Exception)
            {
                try
                {
                    File.Copy(input, output);
                    DeleteFile(input);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return File.Exists(output);
        }

        /// <summary>
        /// Simple function to safely delete files
        /// </summary>
        /// <param name="file">Full path of file to be deleted</param>
        public void DeleteFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return;
            if (!File.Exists(file)) return;
            try
            {
                File.Delete(file);
            }
            catch (Exception)
            { }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("RB3 Character Editor\n\nOriginal version © Lord Zedd\\TehBanStick, 2015\n\nRefreshed version © TrojanNemo, 2023\n\nEnjoy!", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
		{
        //    if (txtFile.Text == "No file loaded")
        //   {
        //        Environment.Exit(0);
        //    }
		//	var result = System.Windows.MessageBox.Show("Do you want to resign the save file before exiting?\nClick Yes to resign\nClick No to just exit the program", "Character Editor", MessageBoxButton.YesNo, MessageBoxImage.Question);
		//	if (result == MessageBoxResult.No)
		//	{
        //        Environment.Exit(0);
        //    }
		//	if (SignSave(txtFile.Text))
        //    {
        //        System.Windows.MessageBox.Show("Resigned save file successfully", "Character Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        System.Windows.MessageBox.Show("Resigning save file failed, backup restored", "Character Editor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
			Environment.Exit(0);			
		}
            private void btnOpen_Click(object sender, RoutedEventArgs e)
		{ 
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.RestoreDirectory = true;
            ofd.Title = "Open Save";
            ofd.Filter = "RB3 PS3 Save|SAVE*.DAT";
			//if (ofd.ShowDialog() == DialogResult)
			if ((bool)ofd.ShowDialog())
			{
				ProcessSaveFile(ofd.FileName);
			}
		}

		private void ProcessSaveFile(string save)
		{
            FileStream fs = new FileStream(save, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 0;

            //Make sure this is a con file
            var fileMagic = br.ReadInt32();
        //    if (fileMagic != 542003011)
        //    {
        //        fs.Close();
        //        br.Close();
        //        System.Windows.MessageBox.Show("This file is not a CON container", "Character Editor", MessageBoxButton.OK, MessageBoxImage.Exclamation); ;
        //        return;
        //    }
            charTabs.Items.Clear();
            txtFile.Text = save;
            for (int i = 0; i < 10; i++)
            {
                fs.Position = charBasesB[i];
                int namesize = br.ReadInt32();
                if (namesize > 0)
                {
                    string name = Encoding.ASCII.GetString(br.ReadBytes(namesize));

                    charTabs.Items.Add(new TabItem
                    {
                        Header = name,
                        Content = new Character(save, fs, br, i, charBasesA, charBasesB),
                    });
                }
            }
            fs.Close();
            br.Close();
            if (charTabs.Items.Count <= 0)
                System.Windows.MessageBox.Show("This save does not appear to contain any characters. You may need to modify them in-game first.", "Character Editor", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			charTabs.Items.Clear();
			txtFile.Text = "No file loaded";
		}
	}
}
