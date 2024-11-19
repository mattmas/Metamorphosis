﻿namespace Metamorphosis.UI
{
    partial class CompareForm
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.tbPrevious = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btnStart = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbDateTime = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbDocumentChoice = new System.Windows.Forms.ComboBox();
            this.cbSelectionSets = new System.Windows.Forms.ComboBox();
            this.btnSaveCategories = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cbUseDocumentGuid = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 262);
            this.label1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(652, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Compare a model against a previous snapshot file:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // tbPrevious
            // 
            this.tbPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPrevious.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbPrevious.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbPrevious.Location = new System.Drawing.Point(32, 327);
            this.tbPrevious.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tbPrevious.Name = "tbPrevious";
            this.tbPrevious.Size = new System.Drawing.Size(897, 38);
            this.tbPrevious.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(973, 322);
            this.button1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 55);
            this.button1.TabIndex = 2;
            this.button1.Text = "Browse";
            this.toolTip1.SetToolTip(this.button1, "Browse for the previous snapshot.");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Snapshot Database|*.sdb|All Files|*.*";
            this.openFileDialog1.Title = "Select an existing snapshot file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 410);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(460, 32);
            this.label2.TabIndex = 3;
            this.label2.Text = "Choose the Categories to compare:";
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.CheckBoxes = true;
            this.treeView1.Location = new System.Drawing.Point(40, 494);
            this.treeView1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(1129, 498);
            this.treeView1.TabIndex = 4;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.onAfterCheck);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(976, 1042);
            this.btnStart.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(200, 55);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(704, 1042);
            this.button2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(200, 55);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(5, 19);
            this.label4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(1176, 119);
            this.label4.TabIndex = 9;
            this.label4.Text = "DISCLAIMER: This application checks parameter values and some aspects of graphic " +
    "changes - but it is not absolute. It is intended to assist you, not a replacemen" +
    "t for professional judgement.";
            // 
            // cbDateTime
            // 
            this.cbDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDateTime.AutoSize = true;
            this.cbDateTime.Location = new System.Drawing.Point(82, 1027);
            this.cbDateTime.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbDateTime.Name = "cbDateTime";
            this.cbDateTime.Size = new System.Drawing.Size(290, 36);
            this.cbDateTime.TabIndex = 10;
            this.cbDateTime.Text = "Date  stamp output";
            this.toolTip1.SetToolTip(this.cbDateTime, "Add a date stamp to the output results filename.");
            this.cbDateTime.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 148);
            this.label3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 32);
            this.label3.TabIndex = 11;
            this.label3.Text = "Model:";
            // 
            // cbDocumentChoice
            // 
            this.cbDocumentChoice.DisplayMember = "Title";
            this.cbDocumentChoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDocumentChoice.FormattingEnabled = true;
            this.cbDocumentChoice.Location = new System.Drawing.Point(32, 188);
            this.cbDocumentChoice.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbDocumentChoice.Name = "cbDocumentChoice";
            this.cbDocumentChoice.Size = new System.Drawing.Size(897, 39);
            this.cbDocumentChoice.TabIndex = 12;
            this.toolTip1.SetToolTip(this.cbDocumentChoice, "Model to compare against previous results.");
            this.cbDocumentChoice.SelectedIndexChanged += new System.EventHandler(this.onSelectedModelChange);
            // 
            // cbSelectionSets
            // 
            this.cbSelectionSets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectionSets.FormattingEnabled = true;
            this.cbSelectionSets.Location = new System.Drawing.Point(541, 403);
            this.cbSelectionSets.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbSelectionSets.Name = "cbSelectionSets";
            this.cbSelectionSets.Size = new System.Drawing.Size(388, 39);
            this.cbSelectionSets.TabIndex = 13;
            this.toolTip1.SetToolTip(this.cbSelectionSets, "Previous category configurations");
            this.cbSelectionSets.SelectedIndexChanged += new System.EventHandler(this.onCategorySettingChanged);
            // 
            // btnSaveCategories
            // 
            this.btnSaveCategories.Location = new System.Drawing.Point(973, 403);
            this.btnSaveCategories.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnSaveCategories.Name = "btnSaveCategories";
            this.btnSaveCategories.Size = new System.Drawing.Size(200, 55);
            this.btnSaveCategories.TabIndex = 14;
            this.btnSaveCategories.Text = "Save";
            this.toolTip1.SetToolTip(this.btnSaveCategories, "Save the currently selected categories for future use.");
            this.btnSaveCategories.UseVisualStyleBackColor = true;
            this.btnSaveCategories.Click += new System.EventHandler(this.btnSaveCategories_Click);
            // 
            // cbUseDocumentGuid
            // 
            this.cbUseDocumentGuid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbUseDocumentGuid.AutoSize = true;
            this.cbUseDocumentGuid.Location = new System.Drawing.Point(82, 1088);
            this.cbUseDocumentGuid.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbUseDocumentGuid.Name = "cbUseDocumentGuid";
            this.cbUseDocumentGuid.Size = new System.Drawing.Size(419, 36);
            this.cbUseDocumentGuid.TabIndex = 15;
            this.cbUseDocumentGuid.Text = "Use EpisodeGUID Difference";
            this.toolTip1.SetToolTip(this.cbUseDocumentGuid, "Uses a new API mechanism for checking differences available in Revit 2023+");
            this.cbUseDocumentGuid.UseVisualStyleBackColor = true;
            this.cbUseDocumentGuid.CheckedChanged += new System.EventHandler(this.onEpisodeGUIDChecked);
            // 
            // CompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1208, 1185);
            this.Controls.Add(this.cbUseDocumentGuid);
            this.Controls.Add(this.btnSaveCategories);
            this.Controls.Add(this.cbSelectionSets);
            this.Controls.Add(this.cbDocumentChoice);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbDateTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbPrevious);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompareForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Revit Metamorphosis Compare";
            this.Shown += new System.EventHandler(this.onShown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPrevious;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbDateTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbDocumentChoice;
        private System.Windows.Forms.ComboBox cbSelectionSets;
        private System.Windows.Forms.Button btnSaveCategories;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox cbUseDocumentGuid;
    }
}