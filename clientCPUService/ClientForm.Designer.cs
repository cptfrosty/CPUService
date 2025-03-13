namespace clientCPUService
{
    partial class ClientForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelCPUInfo = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelStatusConnect = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownMaxAvCpu = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownMinutes = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownHour = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tbLaunchApp = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxAvCpu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHour)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Загрузка процессора";
            // 
            // labelCPUInfo
            // 
            this.labelCPUInfo.AutoSize = true;
            this.labelCPUInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCPUInfo.Location = new System.Drawing.Point(12, 32);
            this.labelCPUInfo.Name = "labelCPUInfo";
            this.labelCPUInfo.Size = new System.Drawing.Size(99, 46);
            this.labelCPUInfo.TabIndex = 1;
            this.labelCPUInfo.Text = "60%";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(363, 229);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel1.Controls.Add(this.labelStatusConnect);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.numericUpDownMaxAvCpu);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.numericUpDownMinutes);
            this.panel1.Controls.Add(this.numericUpDownHour);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.tbLaunchApp);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Location = new System.Drawing.Point(178, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(441, 255);
            this.panel1.TabIndex = 3;
            // 
            // labelStatusConnect
            // 
            this.labelStatusConnect.AutoSize = true;
            this.labelStatusConnect.Location = new System.Drawing.Point(180, 229);
            this.labelStatusConnect.Name = "labelStatusConnect";
            this.labelStatusConnect.Size = new System.Drawing.Size(85, 13);
            this.labelStatusConnect.TabIndex = 15;
            this.labelStatusConnect.Text = "Подключение...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 229);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(170, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Статус подключения к серверу: ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(70, 146);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(15, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "%";
            // 
            // numericUpDownMaxAvCpu
            // 
            this.numericUpDownMaxAvCpu.Location = new System.Drawing.Point(15, 141);
            this.numericUpDownMaxAvCpu.Name = "numericUpDownMaxAvCpu";
            this.numericUpDownMaxAvCpu.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownMaxAvCpu.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 125);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(181, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Порог загруженности процессора";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(70, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "час.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(158, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "мин.";
            // 
            // numericUpDownMinutes
            // 
            this.numericUpDownMinutes.Location = new System.Drawing.Point(103, 88);
            this.numericUpDownMinutes.Name = "numericUpDownMinutes";
            this.numericUpDownMinutes.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownMinutes.TabIndex = 7;
            // 
            // numericUpDownHour
            // 
            this.numericUpDownHour.Location = new System.Drawing.Point(15, 88);
            this.numericUpDownHour.Name = "numericUpDownHour";
            this.numericUpDownHour.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownHour.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(256, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Временный интервал запускаемого приложения";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Запускаемое приложение";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(363, 34);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Выбрать";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // tbLaunchApp
            // 
            this.tbLaunchApp.Location = new System.Drawing.Point(15, 36);
            this.tbLaunchApp.Name = "tbLaunchApp";
            this.tbLaunchApp.Size = new System.Drawing.Size(342, 20);
            this.tbLaunchApp.TabIndex = 3;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 276);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelCPUInfo);
            this.Controls.Add(this.label1);
            this.MaximumSize = new System.Drawing.Size(644, 315);
            this.MinimumSize = new System.Drawing.Size(644, 315);
            this.Name = "ClientForm";
            this.Text = "Клиентское приложение CPUService";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxAvCpu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHour)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCPUInfo;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbLaunchApp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxAvCpu;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownMinutes;
        private System.Windows.Forms.NumericUpDown numericUpDownHour;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelStatusConnect;
    }
}

