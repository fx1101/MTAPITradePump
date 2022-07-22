using MtApi;
using MtApi.Monitors;

namespace MTAPITradePump
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
            _apiClient.BeginDisconnect();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this._AxNSTOrders = new AxNSTOrdersAPI.AxNSTOrders();
            this.logListBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this._AxNSTOrders)).BeginInit();
            this.SuspendLayout();
            // 
            // _AxNSTOrders
            // 
            this._AxNSTOrders.Enabled = true;
            this._AxNSTOrders.Location = new System.Drawing.Point(0, 0);
            this._AxNSTOrders.Name = "_AxNSTOrders";
            this._AxNSTOrders.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_AxNSTOrders.OcxState")));
            this._AxNSTOrders.Size = new System.Drawing.Size(50, 38);
            this._AxNSTOrders.TabIndex = 0;
            this._AxNSTOrders.NewOrder += new AxNSTOrdersAPI.@__NSTOrders_NewOrderEventHandler(this._AxNSTOrders_NewOrder);
            this._AxNSTOrders.CancelOrder += new AxNSTOrdersAPI.@__NSTOrders_CancelOrderEventHandler(this._AxNSTOrders_CancelOrder);
            this._AxNSTOrders.ModifyOrder += new AxNSTOrdersAPI.@__NSTOrders_ModifyOrderEventHandler(this._AxNSTOrders_ModifyOrder);
            this._AxNSTOrders.OrderStatus += new AxNSTOrdersAPI.@__NSTOrders_OrderStatusEventHandler(this._AxNSTOrders_OrderStatus);
            this._AxNSTOrders.VerifyTicker += new AxNSTOrdersAPI.@__NSTOrders_VerifyTickerEventHandler(this._AxNSTOrders_VerifyTicker);
            this._AxNSTOrders.Logon += new AxNSTOrdersAPI.@__NSTOrders_LogonEventHandler(this._AxNSTOrders_Logon);
            // 
            // logListBox
            // 
            this.logListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logListBox.FormattingEnabled = true;
            this.logListBox.Location = new System.Drawing.Point(11, 41);
            this.logListBox.Margin = new System.Windows.Forms.Padding(2);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(464, 290);
            this.logListBox.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 347);
            this.Controls.Add(this.logListBox);
            this.Controls.Add(this._AxNSTOrders);
            this.Name = "Form1";
            this.Text = "MTAPI Trade Pump";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this._AxNSTOrders)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox logListBox;
    }
}

