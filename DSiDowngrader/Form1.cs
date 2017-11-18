using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSiDowngrader
{
    public partial class Form1 : Form
    {
        private DSiTools dsicrypto = new DSiTools();
        public Form1()
        {
            InitializeComponent();
        }



         private void ConsoleID_Extract(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "DSiWare Exports|*.bin";
            openFileDialog1.Title = "Select a bin File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream dsiExport = null;

                if ((dsiExport = openFileDialog1.OpenFile()) != null)
                {
                    using (dsiExport)
                    {
                        byte[] footer = new byte[0x460];
                        dsiExport.Seek(DSiTools.EOFF_FOOTER, SeekOrigin.Begin);
                        dsiExport.Read(footer, 0, DSiTools.ESIZE_FOOTER);
   
                        this.ConsoleID.Text  = BitConverter.ToString(dsicrypto.GetCID(footer)).Replace("-", string.Empty); ;
                    }
                }
            }
        }


        private void CID_Extract(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CID |*.bin";
            openFileDialog1.Title = "Select a bin File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream dsiExport = null;

                if ((dsiExport = openFileDialog1.OpenFile()) != null)
                {
                    if (dsiExport.Length < 16)
                    {
                        MessageBox.Show("Incorrect size for CID.bin. Smaller than expected");
                        return;
                    }
                    if (dsiExport.Length > 16)
                    {
                        MessageBox.Show("Incorrect size for CID.bin. Larger than expected. ");
                        return; 
                    }
                    using (dsiExport)
                    {
                        byte[] CID = new byte[16];

                        dsiExport.Read(CID, 0, 16);
                        dsicrypto.CID = CID;
                        this.CID.Text = BitConverter.ToString(CID).Replace("-", string.Empty); ;
                    }
                }
            }
        }

        private void crypt_NAND(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "NAND |*.bin";
            openFileDialog1.Title = "Select a bin File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream nand = null;

                if ((nand = openFileDialog1.OpenFile()) != null)
                {
                    if (nand.Length < 0x0F000000)
                    {
                        MessageBox.Show("Invalid NAND size. Smaller than expected");
                        return;
                    }
                    if (nand.Length > 0x0F000000)
                    {
                        MessageBox.Show("Invalid NAND size. Larger than expected (NAND should have no footer)");
                        return;
                    }
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    saveFileDialog1.Filter = "NAND |*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;
                    saveFileDialog1.FileName = "NAND";
                    saveFileDialog1.DefaultExt = "bin";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        completed.Visible = false;
                        using (var fs = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                        {
                            using (nand)
                            {
                                dsicrypto.cryptNAND(nand, fs, this.progressBar1); 
                            }
                        }
                        completed.Visible = true;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var fs = new FileStream("config.bin", FileMode.OpenOrCreate, FileAccess.Read))
            {
                if (fs.Length > 0 )
                {
                    dsicrypto.CID = new byte[16];
                    fs.Read(dsicrypto.CID, 0, 16);
                    dsicrypto.ConsoleID = new byte[8];
                    fs.Read(dsicrypto.ConsoleID, 0, 8);
                    
                    this.CID.Text = BitConverter.ToString(dsicrypto.CID).Replace("-", string.Empty); ;
                    this.ConsoleID.Text = BitConverter.ToString(dsicrypto.ConsoleID).Replace("-", string.Empty); ;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (var fs = new FileStream("config.bin", FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (dsicrypto.CID == null)
                {
                    dsicrypto.CID = new byte[16];
                }
                fs.Write(dsicrypto.CID, 0, 16);

                if (dsicrypto.ConsoleID == null)
                {
                    dsicrypto.ConsoleID = new byte[8];
                }
                fs.Write(dsicrypto.ConsoleID, 0, 8);


            }
        }

        private void add_footer(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "NAND |*.bin";
            openFileDialog1.Title = "Select a bin File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream nand = null;

                if ((nand = openFileDialog1.OpenFile()) != null)
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    saveFileDialog1.Filter = "NAND |*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;
                    saveFileDialog1.FileName = "DSi-1";
                    saveFileDialog1.DefaultExt = "mmc";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {

                        using (var fs = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                        {
                            using (nand)
                            {
                                nand.CopyTo(fs);
                            }
                            fs.Write(Encoding.ASCII.GetBytes("DSi eMMC CID/CPU"), 0, 16);
                            if (dsicrypto.CID == null)
                            {
                                dsicrypto.CID = new byte[16];
                            }
                            fs.Write(dsicrypto.CID, 0, 16);
                            byte[] temp = new byte[8];
                            if (dsicrypto.ConsoleID != null)
                            {
                                 temp = dsicrypto.ConsoleID.Clone() as byte[];
                            }
                            Array.Reverse(temp);
                            fs.Write(temp, 0, 8);

                            fs.Write(new byte[24], 0, 24);
                        }

                    }
                }
            }
        }

    }

}
