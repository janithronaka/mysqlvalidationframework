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
    public partial class Form1 : Form
    {
        int[] mandetory_flag;
        int[] unique_flag;
        DataGridViewComboBoxCell tblList = new DataGridViewComboBoxCell();
        public DBConnect server;
        public string[][][] foreignRefs;
        public int refCount = 0;
        public string [] sign;
        public string[] refTbl;
        String [] sqlCode;
        int rowselected = -1;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbSchema.Items.Clear();
           String attrib =  server.GetAllSchema();
           string[] schemas = attrib.Split('|');
           foreach (string db in schemas)
           {
               cmbSchema.Items.Add(db);
           }
           cmbSchema.Items.RemoveAt(cmbSchema.Items.Count-1);
        }

        private void btnViewCode_Click(object sender, EventArgs e)
        {            
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cmbSchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbTable.Items.Clear();
            server.SetDatabase(cmbSchema.SelectedItem.ToString());
            String attrib = server.GetAllTables();
            string[] tbls = attrib.Split('|');
            foreach (string tbl in tbls)
            {
                cmbTable.Items.Add(tbl);
            }
            cmbTable.Items.RemoveAt(cmbTable.Items.Count - 1);
        }

        private void btnGenarate_Click(object sender, EventArgs e)
        {
            if(Validations())
            {
                sqlCode = new String[4+tblRelation.RowCount*6];
                
                MessageBox.Show(GenarateInsertMethod());
                MessageBox.Show(GenarateValidationsMethod());
                MessageBox.Show(GenerateCheckReferenceMethod());
                MessageBox.Show(GenarateMandatoryInfoMethod());
                MessageBox.Show(GenarateCheckFormatInfoMethod());
            }
        }

        private String GenarateInsertMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();
            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Insert_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + " (";
            sql += "\r\n\tIN attribute_string VARCHAR(5000),\r\n\tOUT rslt_ VARCHAR(1000)\t)";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            sql += "\r\n\tDECLARE return_ VARCHAR (500);";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tDECLARE " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ";";
                
            }
            sql += "\r\n\tCALL Extract_" + tbl.Substring(0,1).ToUpper() + tbl.Substring(1) + "_Attributes ("; //need to implement
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_, ";
            }
            sql += " return_ );";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";
            sql += "\r\n\tCALL Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Validations(";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_, ";
            }
            sql += " return_ );";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";
            sql += "\r\n\tINSERT INTO " + "`" + db + "`.`" + tbl + "` (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "`" + tblRelation.Rows[i].Cells[0].Value.ToString() + "` ";
                if (i != (tblRelation.RowCount - 1))
                    sql += ",";
            }
            sql += ") VALUES (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += tblRelation.Rows[i].Cells[0].Value.ToString() + "_ ";
                if (i != (tblRelation.RowCount - 1))
                    sql += ",";
            }
            sql += "\r\n\tSET rslt_ = 'TRUE|Successfully Saved the Record.';";
            sql += "\r\nEND";//"`uid`, `username`, `password`, `email`, `reputation`, `registered_date`, `last_login_time`, `last_post_name`, `postcount`, `avatar`, `last_ip`) VALUES (NULL, '', '', '', '', '', '', '', '', '/avatar/def.jpg', '');";
            return sql;
        }
        private bool Validations()
        {
            bool valid = true;
            if(String.IsNullOrEmpty(cmbSchema.SelectedItem.ToString()))
            {
                valid = false;
                MessageBox.Show("Error. Invalid Database Name.");
            }
            else if (String.IsNullOrEmpty(cmbTable.SelectedItem.ToString()))
            {
                valid = false;
                MessageBox.Show("Error. Invalid Table Name.");
            }
            return valid;
        }

        private String GenarateValidationsMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();

            String [][] foreign_key = new String[tblRelation.RowCount][];

            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Validations (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ",";
            }
            sql += "\r\n\tDECLARE rslt_ OUT VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            sql += "\r\n\tDECLARE return_ VARCHAR (500);";
            sql += "\r\n\tCALL " + "Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Mandatory_Info (" ;
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (tblRelation.Rows[i].Cells[3].Value.ToString().Equals("True"))
                    sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " return_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";
            sql += "\r\n\tCALL " + "Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Format_Info (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " return_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";

            sql += "\r\n\tCALL Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Primary_Key (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (tblRelation.Rows[i].Cells[4].Value.ToString().Equals("True"))
                    sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " retrun_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";

            sql += "\r\n\tCALL Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_References (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " retrun_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";
            sql += "\r\n\tSET rslt_ = 'TRUE|';";
            sql += "\r\nEND";
            return sql;
        }

        private string GenerateCheckReferenceMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();

            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_References (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ",";
            }
            sql += "\r\n\tDECLARE rslt_ OUT VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ',  MESSAGE_TEXT);";
            sql += "\r\n\tDECLARE return_ VARCHAR (500);";
            for (int i = 0; i < refCount; i++)
            {
                string[] tbl_stat = sign[i].Split('|');
                sql += "\r\n\tCALL Check_" + tbl_stat[0].Substring(0, 1).ToUpper() + tbl_stat[0].Substring(1) + "_Exists (";
                for (int j = 1; j < tbl_stat.Length; j++)
                {
                    sql += " " + tbl_stat[j] + "_,";
                }
                sql += " retrun_);";
                sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'FALSE' THEN";
                sql += "\r\n\t\tSET rslt_ = return_;";
                sql += "\r\n\t\tLEAVE proc;";
                sql += "\r\n\tEND IF;";
            }
            sql += "\r\n\tSET rslt_ = 'TRUE';";
            sql += "\r\nEND";
            return sql;
        }

        private string GenarateMandatoryInfoMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();

            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Mandatory_Info (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if(tblRelation.Rows[i].Cells[3].Value.ToString().Equals("True"))
                {
                    sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                    if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                        sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                    sql += ",";
                }
            }

            sql += "\r\n\tDECLARE rslt_ OUT VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (tblRelation.Rows[i].Cells[3].Value.ToString().Equals("True"))
                {
                    sql += "\r\n\tIF " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ IS NULL OR LTRIM(" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_) = '' THEN";
                    sql += "\r\n\t\tSET rslt_ = 'FALSE|Error: [" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_] field is mandatory.'";
                    sql += "\r\n\t\tLEAVE proc;";
                    sql += "\r\n\tEND IF;";
                }
            }
            sql += "\r\nEND";
            return sql;
        }

        private String GenarateCheckFormatInfoMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();

            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Format_Info (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ",";
            }
            sql += "\r\n\tDECLARE rslt_ OUT VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (!String.IsNullOrEmpty(tblRelation.Rows[i].Cells[2].Value.ToString()))
                {
                    sql += "\r\n\tIF LENGTH(" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_) > " + tblRelation.Rows[i].Cells[2].Value.ToString() + " THEN";
                    sql += "\r\n\t\tSET rslt_ = 'FALSE|Error: [" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_] field exceeds its defined size limit.'";
                    sql += "\r\n\t\tLEAVE proc;";
                    sql += "\r\n\tEND IF;";
                }
            }
            sql += "END";
            return sql;
        }

        private String GenarateCheckPrimaryKeyMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();

            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Primary_Key (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if(tblRelation.Rows[i].Cells[4].Value.ToString().Equals("True"))
                {
                    sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                    if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                        sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                    sql += ",";
                }
            }
            sql += "\r\n\tDECLARE rslt_ OUT VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            sql += "\r\n\tCALL Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Exists (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (tblRelation.Rows[i].Cells[4].Value.ToString().Equals("True"))
                    sql += " " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " retrun_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'TRUE' OR LENGTH(return_) > 8 THEN";
            sql += "\r\n\t\tIF SUBSTRING_INDEX(return_,\"|\",1) = 'TRUE' THEN";
            sql += "\r\n\t\t\tSET rslt_ = 'FALSE|';";
            sql += "\r\n\t\tELSE";
            sql += "\r\n\t\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tEND IF;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tEND IF;";
            sql += "END";
            return sql;
        }

        private String GenarateExtractAttributesMethod()
        {
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();
            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Insert_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + " (";
            sql += "\r\n\tIN attribute_string VARCHAR(5000),\r\n\tOUT rslt_ VARCHAR(1000)\t)";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE EXIT HANDLER FOR SQLEXCEPTION SET rslt_= CONCAT('FALSE|Error: ', MESSAGE_TEXT);";
            sql += "\r\n\tDECLARE return_ VARCHAR (500);";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tDECLARE " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ";";

            }
            sql += "\r\n\tCALL Extract_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Attributes ("; //need to implement
            return sql;
        }

        private void cmbTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            tblRelation.Rows.Clear();
            //requesting all the attributes of the table along with the information
            String attrib = server.GetAllAttributes(cmbTable.SelectedItem.ToString());
            int i = 0;
            string[] attributes = attrib.Split('|');
            attributes = attributes.Take(attributes.Count() - 1).ToArray();
            string[][] attribute = new string[attributes.Length][];

            //get each attribute's properties of the selected table
            foreach (string atr in attributes)
            {
                attribute[i] = atr.Split('^');
                i++;
            }
            mandetory_flag = new int[i-1];
            unique_flag = new int[i - 1];

            //loop through the number of attributes
            for(int step = 0; step < attributes.Length - 1; step++)
            {
                Object[] tmp_ = new Object [10];
                int index = 0;

                //ading each attribute's information to the table
                foreach (string atr in attribute[step])
                {
                    tmp_[index] = atr;
                    index++;
                }
                if (tmp_[3].ToString().Equals("NO")) //identifying the mandatory fields
                {
                    tmp_[3] = true;
                    mandetory_flag[step] = 0;
                }
                else
                {
                    tmp_[3] = false;
                    mandetory_flag[step] = 1;
                }
                if (tmp_[4].ToString().Equals("UNI")) //identifying the unique fields
                {
                    tmp_[4] = false;
                    tmp_[5] = true;
                    unique_flag[step] = 0;
                }
                else if (tmp_[4].ToString().Equals("PRI")) //identifying the primary keys
                {
                    tmp_[4] = true;
                    tmp_[3] = true;
                    tmp_[5] = false;
                    unique_flag[step] = 1;
                }
                else
                {
                    unique_flag[step] = 1;
                    tmp_[4] = false;
                    tmp_[5] = false;
                }
                tblRelation.Rows.Add(tmp_); //adding a row to the table
            }
            foreignRefs = new String[tblRelation.RowCount][][];
            for (int index = 0; i < tblRelation.RowCount;i++)
            {
                DataGridViewButtonCell btn = (DataGridViewButtonCell) tblRelation.Rows[index].Cells[8];
                btn.Value = "Show References";
            }
            refTbl = new String[tblRelation.RowCount];
            sign = new string[tblRelation.RowCount];
            refCount = 0;

        }
        
        private void tblRelation_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (!Object.ReferenceEquals(tblRelation.Rows[e.RowIndex].Cells[e.ColumnIndex], null))
            {
                if (e.ColumnIndex == 3)
                {
                    if (tblRelation.Rows.Count > 0 && !Object.ReferenceEquals(mandetory_flag, null))
                    {
                        if (tblRelation.Rows[e.RowIndex].Cells[3].Value.ToString().Equals("False") && mandetory_flag[e.RowIndex] == 0)
                        {
                            MessageBox.Show("Error: You cannot set as not mandatory on the columns which have already defined as mandatory in the database.");
                            tblRelation.Rows[e.RowIndex].Cells[3].Value = true;
                        }
                    }
                }
                else if (e.ColumnIndex == 5)
                {
                    if (tblRelation.Rows[e.RowIndex].Cells[5].Value.ToString().Equals("False") && unique_flag[e.RowIndex] == 0)
                    {
                        MessageBox.Show("Error: You cannot set as not uniuqe on the columns which have already defined as uniuqe in the database.");
                        tblRelation.Rows[e.RowIndex].Cells[5].Value = true;
                    }
                }
            }
            
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void RmbOptions_Click(object sender, EventArgs e)
        {
            String tbl = cmbTable.SelectedItem.ToString();
            string db = cmbSchema.SelectedItem.ToString();
            if(tblRelation.RowCount>0 && rowselected > -1 && rowselected < tblRelation.RowCount)
            {
                frmForeignKey frmRef = new frmForeignKey();
                frmRef.localTblName = tbl;
                frmRef.server = server;
                frmRef.localCols = new String[tblRelation.RowCount];
                for (int i = 0; i < tblRelation.RowCount; i++)
                    frmRef.localCols[i] = tblRelation.Rows[i].Cells[0].Value.ToString();
                frmRef.refs = foreignRefs[rowselected];
                frmRef.refTblName = refTbl[rowselected];
                frmRef.parentRow = rowselected;
                frmRef.parent = this;
                if (!String.IsNullOrEmpty(refTbl[rowselected]))
                    frmRef.exist = 1;
                frmRef.Visible = true;
            }
        }

        private void tblRelation_Click(object sender, EventArgs e)
        {
            
        }

        private void tblRelation_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            rowselected = e.RowIndex;
        }
    }
}
