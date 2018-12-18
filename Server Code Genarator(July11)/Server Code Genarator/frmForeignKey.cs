using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server_Code_Genarator
{
    public partial class frmForeignKey : Form
    {
        public Form1 parent;
        public DBConnect server;
        public string refTblName, localTblName;
        public string[][] refs;
        public string[] localCols;
        public int parentRow;
        public int exist = 0;
        public frmForeignKey()
        {
            InitializeComponent();
        }

        private void frmForeignKey_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(refTblName))
                button1.Text = "Change";
            cmbTable.Items.Clear();
            String attrib = server.GetAllTables();
            string[] tbls = attrib.Split('|');
            foreach (string tbl in tbls)
            {
                if(!localTblName.Equals(tbl))
                    cmbTable.Items.Add(tbl);
            }
            cmbTable.Items.RemoveAt(cmbTable.Items.Count - 1);
                
            if (!String.IsNullOrEmpty(refTblName))
            {
                for(int i=0; i < cmbTable.Items.Count; i++)
                {
                    if (cmbTable.Items[i].ToString().Equals(refTblName))
                    {
                        cmbTable.SelectedIndex = i;
                        break;
                    }
                }
                for(int i = 0; i < refs.Length; i++)
                {
                    DataGridViewComboBoxCell cmb;
                    Object []row_ = new Object[2];
                    row_[0] = refs[i][0];
                    row_[1] = null;
                    tblRelation.Rows.Add(row_);
                    cmb = (DataGridViewComboBoxCell) tblRelation.Rows[i].Cells[1];
                    for (int j = 0; j < localCols.Length; j++)
                        cmb.Items.Add(localCols[j]);
                    for(int j = 0; j < cmb.Items.Count; j++)
                    {
                        if(cmb.Items[j].Equals(refs[i][1]))
                        {
                            cmb.Value = refs[i][1];
                            break;
                        }
                    }
                }
            }
        }

        private void cmbTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (exist==0)
            {
                tblRelation.Rows.Clear();
                refTblName = cmbTable.SelectedItem.ToString();
                int row = -1;
                DataGridViewComboBoxCell cmb;
                //requesting all the attributes of the table along with the information
                String attrib = server.GetAllAttributes(cmbTable.SelectedItem.ToString());
                string[] attributes = attrib.Split('|');
                attributes = attributes.Take(attributes.Count() - 1).ToArray();
                string[] attribute = new string[attributes.Length];
                Object[] tmp_ = new Object[2];
                //get key attributes of the selected table and add it to the datagrid view
                foreach (string atr in attributes)
                {
                    attribute = atr.Split('^');
                    if (attribute[4].Equals("PRI"))
                    {
                        tmp_[0] = attribute[0];
                        tmp_[1] = null;
                        tblRelation.Rows.Add(tmp_);
                        row++;
                        cmb = (DataGridViewComboBoxCell)tblRelation.Rows[row].Cells[1];
                        for (int j = 0; j < localCols.Length; j++)
                            cmb.Items.Add(localCols[j]);
                    }
                }
            }
            else
                exist=0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String [][]tmp_ref = new string[tblRelation.RowCount][];
            int error_ = 0;
            string error_msg_ = "";
            if (tblRelation.RowCount > 0 && String.IsNullOrEmpty(refTblName))
                refTblName = cmbTable.SelectedItem.ToString();
            String tmp_sign = refTblName;
            for (int i = 0; i < tblRelation.RowCount;  i++)
            {
                tmp_ref[i] = new String[2];
                tmp_ref[i][0] = tblRelation.Rows[i].Cells[0].Value.ToString();
                if (!Object.ReferenceEquals(tblRelation.Rows[i].Cells[1].Value, null))
                    tmp_ref[i][1] = tblRelation.Rows[i].Cells[1].Value.ToString();
                else
                    tmp_ref[i][1] = "";
                if(!CheckReferences(tmp_ref[i][0], tmp_ref[i][1]).Equals("TRUE"))
                {
                    error_ = 1;
                    error_msg_ = CheckReferences(tmp_ref[i][0], tmp_ref[i][1]);
                    break;
                }
                tmp_sign += "|" + tblRelation.Rows[i].Cells[1].Value.ToString();
            }
            if(error_ == 0)
            {
                for (int i = 0; i < tmp_ref.Length; i++)
                {
                    string sameCol = tmp_ref[i][1];
                    for (int j = 0; j < tmp_ref.Length; j++)
                    {
                        if (i != j && tmp_ref[j][1].Equals(sameCol))
                        {
                            error_ = 1;
                            error_msg_ = "Error: Local columns which are referring [" + tmp_ref[i][0] + "] and [" + tmp_ref[j][0] + "] are same.";
                        }
                    }
                }
            }

            if(error_ == 0)
            {
                for (int i = 0; i < parent.refCount; i++)
                {
                    string sameCol = parent.sign[i];
                    if (sameCol.Equals(tmp_sign))
                    {
                        error_ = 2;
                        break;
                    }
                }
            }

            if (error_ == 0)
            {
                parent.foreignRefs[parentRow] = tmp_ref;
                parent.sign[parent.refCount] = tmp_sign;
                parent.refCount++;
                parent.refTbl[parentRow] = refTblName;
                this.Visible = false;
            }
            else if(error_ == 2)
                MessageBox.Show("Information: Same reference added on a different attribute.");
            else
            {
                MessageBox.Show(error_msg_);
            }
        }

        private string CheckReferences(string refCol, string localCol)
        {
            if (String.IsNullOrEmpty(refCol) || String.IsNullOrEmpty(localCol))
                return "Error: Invalid reference found.";
            String attrib = server.GetAllAttributes(refTblName);
            string[] attributes = attrib.Split('|');
            attributes = attributes.Take(attributes.Count() - 1).ToArray();
            string[] attribute = new string[attributes.Length];
            String[] refInfo = new string[2];
            
            foreach (string atr in attributes)
            {
                attribute = atr.Split('^');
                if (attribute[0].Equals(refCol))
                {
                    refInfo[0] = attribute[1];
                    refInfo[1] = attribute[2];
                    break;
                }
            }
            attrib = server.GetAllAttributes(localTblName);
            attributes = attrib.Split('|');
            attributes = attributes.Take(attributes.Count() - 1).ToArray();
            attribute = new string[attributes.Length];
            String[] localInfo = new string[2];
            foreach (string atr in attributes)
            {
                attribute = atr.Split('^');
                if (attribute[0].Equals(localCol))
                {
                    localInfo[0] = attribute[1];
                    localInfo[1] = attribute[2];
                    break;
                }
            }
            if (!refInfo[0].Equals(localInfo[0]))
                return "Error: Referenced column type mismatched on " + refCol + ".";
            else if(!refInfo[1].Equals(localInfo[1]))
                return "Error: Referenced column feild size mismatched on " + refCol + ".";
            return "TRUE";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tblRelation.Rows.Clear();
            cmbTable.SelectedItem = "";
            parent.refCount--;
            parent.foreignRefs[parentRow] = null;
            parent.refTbl[parentRow] = null;
        }
    }
}
