namespace Depressurizer.Dialogs
{
	partial class WarnDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarnDialog));
			this.LabelMessage = new MaterialSkin.Controls.MaterialLabel();
			this.ButtonOk = new MaterialSkin.Controls.MaterialRaisedButton();
			this.SuspendLayout();
			// 
			// LabelMessage
			// 
			this.LabelMessage.AutoSize = true;
			this.LabelMessage.BackColor = System.Drawing.Color.Transparent;
			this.LabelMessage.Depth = 0;
			this.LabelMessage.Font = new System.Drawing.Font("Roboto", 11F);
			this.LabelMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.LabelMessage.Location = new System.Drawing.Point(14, 74);
			this.LabelMessage.Margin = new System.Windows.Forms.Padding(5, 65, 5, 5);
			this.LabelMessage.MaximumSize = new System.Drawing.Size(416, 0);
			this.LabelMessage.MouseState = MaterialSkin.MouseState.HOVER;
			this.LabelMessage.Name = "LabelMessage";
			this.LabelMessage.Size = new System.Drawing.Size(63, 19);
			this.LabelMessage.TabIndex = 0;
			this.LabelMessage.Text = "Warning";
			// 
			// ButtonOk
			// 
			this.ButtonOk.Depth = 0;
			this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonOk.Location = new System.Drawing.Point(339, 140);
			this.ButtonOk.MouseState = MaterialSkin.MouseState.HOVER;
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.Primary = true;
			this.ButtonOk.Size = new System.Drawing.Size(75, 23);
			this.ButtonOk.TabIndex = 1;
			this.ButtonOk.Text = "Ok";
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// WarnDialog
			// 
			this.AcceptButton = this.ButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonOk;
			this.ClientSize = new System.Drawing.Size(426, 175);
			this.ControlBox = false;
			this.Controls.Add(this.ButtonOk);
			this.Controls.Add(this.LabelMessage);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(426, 175);
			this.MinimumSize = new System.Drawing.Size(426, 175);
			this.Name = "WarnDialog";
			this.Sizable = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "WarnDialog";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MaterialSkin.Controls.MaterialLabel LabelMessage;
		private MaterialSkin.Controls.MaterialRaisedButton ButtonOk;
	}
}