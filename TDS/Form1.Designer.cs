namespace TDS
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblTotal1 = new System.Windows.Forms.Label();
            this.Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Accesstoken = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IDAcc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CookieFacebook = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Social = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Fields = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Submit = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvMain
            // 
            this.dgvMain.AllowUserToAddRows = false;
            this.dgvMain.AllowUserToDeleteRows = false;
            this.dgvMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Name,
            this.Accesstoken,
            this.IDAcc,
            this.CookieFacebook,
            this.Social,
            this.Fields,
            this.Submit});
            this.dgvMain.Location = new System.Drawing.Point(7, 44);
            this.dgvMain.Margin = new System.Windows.Forms.Padding(4);
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.ReadOnly = true;
            this.dgvMain.RowHeadersWidth = 51;
            this.dgvMain.Size = new System.Drawing.Size(1283, 246);
            this.dgvMain.TabIndex = 2;
            this.dgvMain.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dgvMain.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMain_CellValueChanged);
            this.dgvMain.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvMain_CurrentCellDirtyStateChanged);
            // 
            // lblTotal
            // 
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(12, 9);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(419, 23);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Text = "100xu";
            // 
            // lblTotal1
            // 
            this.lblTotal1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal1.Location = new System.Drawing.Point(540, 11);
            this.lblTotal1.Name = "lblTotal1";
            this.lblTotal1.Size = new System.Drawing.Size(419, 23);
            this.lblTotal1.TabIndex = 6;
            this.lblTotal1.Text = "100xu";
            // 
            // Name
            // 
            this.Name.HeaderText = "Name";
            this.Name.MinimumWidth = 6;
            this.Name.Name = "Name";
            this.Name.ReadOnly = true;
            // 
            // Accesstoken
            // 
            this.Accesstoken.HeaderText = "Accesstoken";
            this.Accesstoken.MinimumWidth = 6;
            this.Accesstoken.Name = "Accesstoken";
            this.Accesstoken.ReadOnly = true;
            // 
            // IDAcc
            // 
            this.IDAcc.HeaderText = "IDAcc";
            this.IDAcc.MinimumWidth = 6;
            this.IDAcc.Name = "IDAcc";
            this.IDAcc.ReadOnly = true;
            // 
            // CookieFacebook
            // 
            this.CookieFacebook.HeaderText = "CookieFacebook";
            this.CookieFacebook.MinimumWidth = 6;
            this.CookieFacebook.Name = "CookieFacebook";
            this.CookieFacebook.ReadOnly = true;
            // 
            // Social
            // 
            this.Social.HeaderText = "Social";
            this.Social.MinimumWidth = 6;
            this.Social.Name = "Social";
            this.Social.ReadOnly = true;
            // 
            // Fields
            // 
            this.Fields.HeaderText = "Fields";
            this.Fields.MinimumWidth = 6;
            this.Fields.Name = "Fields";
            this.Fields.ReadOnly = true;
            // 
            // Submit
            // 
            this.Submit.HeaderText = "Submit";
            this.Submit.MinimumWidth = 6;
            this.Submit.Name = "Submit";
            this.Submit.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1902, 1033);
            this.Controls.Add(this.lblTotal1);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.dgvMain);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            //this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvMain;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotal1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Accesstoken;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDAcc;
        private System.Windows.Forms.DataGridViewTextBoxColumn CookieFacebook;
        private System.Windows.Forms.DataGridViewComboBoxColumn Social;
        private System.Windows.Forms.DataGridViewComboBoxColumn Fields;
        private System.Windows.Forms.DataGridViewButtonColumn Submit;
    }
}

