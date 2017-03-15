using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestfulTest
{
    public partial class TestResult : Form
    {
        List<SendRecord> dtList = null;
        public TestResult(List<SendRecord> dtList)
        {
            InitializeComponent();
            this.dtList = dtList;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dtList;
        }

        private void dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                DataGridView grid = sender as DataGridView;
                if (grid == null)
                    return;
                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
                    e.RowBounds.Location.Y,
                    grid.RowHeadersWidth - 4,
                    e.RowBounds.Height);

                TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                    grid.RowHeadersDefaultCellStyle.Font,
                    rectangle,
                    grid.RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                if (grid.Columns.Count > 0)
                    grid.Columns[grid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "dgv_RowPostPaint", "", exp);
            }
        }
    }
}
