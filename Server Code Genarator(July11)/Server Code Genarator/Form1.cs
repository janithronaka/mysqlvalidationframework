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
            // TODO: This line of code loads data into the 'primarydbDataSet.customer' table. You can move, or remove it, as needed.
            this.customerTableAdapter.Fill(this.primarydbDataSet.customer);
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
                MessageBox.Show(GenarateCheckExistMethod());
                MessageBox.Show(GenarateGetMethods());
                MessageBox.Show(GenarateValidationsMethod());
                MessageBox.Show(GenerateCheckReferenceMethod());
                MessageBox.Show(GenarateMandatoryInfoMethod());
                MessageBox.Show(GenarateCheckFormatInfoMethod());
                textBox1.Text = GenarateExtractAttributesMethod();
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
            sql += "\r\n\tCALL Extract_" + tbl.Substring(0,1).ToUpper() + tbl.Substring(1) + "_Attributes (";
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
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                {
                    sql += "\r\n\tIN " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
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
            sql = "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Extract_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Attributes (";
            sql += "\r\n\tIN attr_string_ VARCHAR(10000),";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tOUT " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                sql += ";";

            }
            sql += "\r\n\tOUT rslt_ VARCHAR(500) )";
            sql += "\r\nNOT DETERMINISTIC";
            sql += "\r\nproc: BEGIN";
            sql += "\r\n\tDECLARE tmp_ VARCHAR(1000);";
            sql += "\r\n\tDECLARE result_ VARCHAR(1000);";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tDECLARE tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("varchar") || tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("int")
                    || tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("double") || tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("decimal"))
                {
                    if (tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("varchar"))
                        sql += " (1000)";
                    else if (tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("int"))
                        sql += " (255)";
                    else if (tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("double"))
                        sql += " (255,4)";
                    else if (tblRelation.Rows[i].Cells[1].Value.ToString().ToLower().Equals("decimal"))
                        sql += " (65,4)";
                }
                sql += ";";

            }
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\tSET tmp_ = Get_Attributes(attr_string_,'" + tblRelation.Rows[i].Cells[0].Value.ToString() + "');";
                sql += "\r\n\tIF tmp_ = 'Attribute not exists.' THEN";
                sql += "\r\n\t\tSET tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ = NULL;";
                sql += "\r\n\tELSE";
                sql += "\r\n\t\tIF " + tblRelation.Rows[i].Cells[1].Value.ToString() + " = INT THEN";
                sql += "\r\n\t\t\tCALL ToInt(tmp_,tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,result_);";
                sql += "\r\n\t\tELSEIF " + tblRelation.Rows[i].Cells[1].Value.ToString() + " = DATE THEN";
                sql += "\r\n\t\t\tCALL ToDate(tmp_,tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,result_);";
                sql += "\r\n\t\tELSEIF " + tblRelation.Rows[i].Cells[1].Value.ToString() + " = DOUBLE THEN";
                sql += "\r\n\t\t\tCALL ToDouble(tmp_,tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,result_);";
                sql += "\r\n\t\tELSEIF " + tblRelation.Rows[i].Cells[1].Value.ToString() + " = VARCHAR THEN";
                sql += "\r\n\t\t\tSET tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ = tmp_;";
                sql += "\r\n\t\tEND IF;";
                sql += "\r\n\t\t\tIF result_ = 'FALSE' THEN";
                sql += "\r\n\t\t\t\tSET rslt_ = 'FALSE|Error: Invalid value format in [" + tblRelation.Rows[i].Cells[0].Value.ToString() + "]';";
                sql += "\r\n\t\t\t\tLEAVE proc;";
                sql += "\r\n\t\t\tEND IF;";
                sql += "\r\n\tEND IF;";
            }
            sql += "\r\n\tCALL " + "Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Format_Info (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                    sql += " tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_,";
            }
            sql += " result_);";
            sql += "\r\n\tIF SUBSTRING_INDEX(result_,\"|\",1) = 'FALSE' THEN";
            sql += "\r\n\t\tSET rslt_ = return_;";
            sql += "\r\n\t\tLEAVE proc;";
            sql += "\r\n\tELSE";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                sql += "\r\n\t\tSET " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ = tmp_" + tblRelation.Rows[i].Cells[0].Value.ToString() + "_;";
            }
            sql += "\r\n\tEND IF;";
            sql += "\r\nEND";
            return sql;
        }

        private String GenarateCheckExistMethod()
        {
            int keyCount = 0; 
            String sql = "";
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();
            sql = "DELIMITER $\r\nCREATE FUNCTION " + db + ".Check_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_Exists (";
            for (int i = 0; i < tblRelation.RowCount; i++)
            {
                if (tblRelation.Rows[i].Cells[4].Value.ToString().Equals("True"))
                {
                    keyCount++;
                    sql += "\r\n\t " + tblRelation.Rows[i].Cells[0].Value.ToString() + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                    if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                        sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                    sql += ",";
                }
            }
            String []keys = new String[keyCount];
            for (int i = 0; i < tblRelation.RowCount; i++)
                if (tblRelation.Rows[i].Cells[4].Value.ToString().Equals("True"))
                    keys[i] = tblRelation.Rows[i].Cells[0].Value.ToString();
            sql = sql.Substring(0,sql.Length-1) + ") RETURNS VARCHAR(500)";
            sql += "\r\n\tDECLARE return_ VARCHAR(500);";
            sql += "\r\n\tDECLARE CONTINUE HANDLER FOR SQLEXCEPTION SET return_= 'FALSE|Error: ' + MESSAGE_TEXT;";
            sql += "\r\n\tDECLARE dummy_ INT DEFAULT 0;";
            sql += "\r\n\tDEClARE get_rec CURSOR FOR";
            sql += "\r\n\t   SELECT 1 FROM `" + db + "`.`" + tbl + "`";
            if (keyCount > 0)
            {
                sql += "\r\n\t   WHERE " + keys[0]+ " = " + keys[0] + "_";
                int j = 1;
                while (j < keyCount)
                {
                    sql += "\r\n\t      AND " + keys[j] + " = " + keys[j] + "_";
                    j++;
                }
                sql += ";";
            }
            else
                return "Error: Invalid primary keys";
            sql += "\r\n\tOPEN get_rec;";
            sql += "\r\n\tFETCH get_rec INTO dummy_;";
            sql += "\r\n\tCLOSE get_rec;";
            sql += "\r\n\tIF dummy_ = 1 THEN";
            sql += "\r\n\t\tSET return_ = 'TRUE|';";
            sql += "\r\n\tELSE";
            sql += "\r\n\t\tSET return_ = 'FALSE|Error: " + tbl + " object does not exist for the given information.';";
            sql += "\r\n\tEND IF;";
            sql += "\r\n\tRETURN return_;";
            sql += "\r\nEND";
            return sql;
        }

        private String GenarateGetMethods()
        {
            
            String db = cmbSchema.SelectedItem.ToString();
            String tbl = cmbTable.SelectedItem.ToString();
            String sql = "", tmp_sql = "";
            int keyCount = 0;
            String where = "\r\n\t   WHERE ";
            String tmp_keys = "";
            for (int x = 0; x < tblRelation.RowCount; x++)
            {
                if (tblRelation.Rows[x].Cells[4].Value.ToString().Equals("True"))
                {
                    tmp_keys += tblRelation.Rows[x].Cells[0].Value.ToString() + "|";
                    keyCount++;
                }
            }

            String[] keys = tmp_keys.Split('|');
            for (int i = 0; i < keyCount; i++)
            {
                tmp_sql += "\r\n\tIN " + keys[i] + "_ " + tblRelation.Rows[i].Cells[1].Value.ToString().ToUpper();
                if (!tblRelation.Rows[i].Cells[2].Value.ToString().Equals(""))
                    tmp_sql += " (" + tblRelation.Rows[i].Cells[2].Value.ToString() + ")";
                tmp_sql += ",";
                where += keys[i] + " = " + keys[i] + "_";
                if (i < (keyCount - 1))
                    where += "\r\n\t   AND";
                else
                    where += ";";
            }
            tmp_sql += "\r\n\tOUT rslt_ VARCHAR(500),"; ;
            for (int x = keyCount; x < tblRelation.RowCount; x++)
                {
                    String column = tblRelation.Rows[x].Cells[0].Value.ToString();
                    sql += "DELIMITER $\r\nCREATE PROCEDURE " + db + ".Get_" + tbl.Substring(0, 1).ToUpper() + tbl.Substring(1) + "_" + column.Substring(0, 1).ToUpper() + column.Substring(1) + " (";
                    sql += tmp_sql;
                    sql += "\r\n\tOUT value_ " + tblRelation.Rows[x].Cells[1].Value.ToString();
                    if (!tblRelation.Rows[x].Cells[2].Value.ToString().Equals(""))
                        sql += " (" + tblRelation.Rows[x].Cells[2].Value.ToString() + ") )";
                    sql += "\r\nBEGIN";
                    sql += "\r\n\tDECLARE return_ VARCHAR(500);";
                    sql += "\r\n\tDECLARE CONTINUE HANDLER FOR SQLEXCEPTION SET return_= 'FALSE|Error: ' + MESSAGE_TEXT;";
                    sql += "\r\n\tDECLARE dummy_ INT DEFAULT 0;";
                    sql += "\r\n\tDEClARE get_" + column + " CURSOR FOR";
                    sql += "\r\n\t   SELECT " + column + ",1 FROM `" + db + "`.`" + tbl + "`";
                    if (keyCount > 0)
                    {
                        sql += where;
                    }
                    else
                        return "Error: Invalid primary keys";
                    sql += "\r\n\tOPEN get_" + column  + ";";
                    sql += "\r\n\tFETCH get_" + column + " INTO value_,dummy_;";
                    sql += "\r\n\tCLOSE get_" + column + ";";
                    sql += "\r\n\tIF dummy_ = 1 THEN";
                    sql += "\r\n\t\tSET return_ = 'TRUE|';";
                    sql += "\r\n\tELSE";
                    sql += "\r\n\t\tSET return_ = 'FALSE|Error: " + tbl + " object does not exist for the given information.';";
                    sql += "\r\n\tEND IF;";
                    sql += "\r\nEND";
                    sql += "\r\n\r\n";
                }
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
                    tmp_[7] = true;
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

        private void customerBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.customerBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.primarydbDataSet);

        }
    }
}
