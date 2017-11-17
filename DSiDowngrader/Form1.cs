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
    }
    
}
