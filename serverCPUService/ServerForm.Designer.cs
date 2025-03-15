namespace serverCPUService
{
    partial class ServerForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOnService = new System.Windows.Forms.Button();
            this.btnOffService = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOnService
            // 
            this.btnOnService.Location = new System.Drawing.Point(12, 23);
            this.btnOnService.Name = "btnOnService";
            this.btnOnService.Size = new System.Drawing.Size(315, 23);
            this.btnOnService.TabIndex = 0;
            this.btnOnService.Text = "Включить";
            this.btnOnService.UseVisualStyleBackColor = true;
            this.btnOnService.Click += new System.EventHandler(this.btnOnService_Click);
            // 
            // btnOffService
            // 
            this.btnOffService.Location = new System.Drawing.Point(12, 52);
            this.btnOffService.Name = "btnOffService";
            this.btnOffService.Size = new System.Drawing.Size(315, 23);
            this.btnOffService.TabIndex = 1;
            this.btnOffService.Text = "Выключить";
            this.btnOffService.UseVisualStyleBackColor = true;
            this.btnOffService.Click += new System.EventHandler(this.btnOffService_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(154, 111);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(0, 13);
            this.labelInfo.TabIndex = 3;
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 133);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.btnOffService);
            this.Controls.Add(this.btnOnService);
            this.Name = "ServerForm";
            this.Text = "Серверное приложение CPUService";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOnService;
        private System.Windows.Forms.Button btnOffService;
        private System.Windows.Forms.Label labelInfo;
    }
}

