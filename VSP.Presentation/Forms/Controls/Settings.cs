﻿using DataIntegrationHub.Business.Entities;
using DataIntegrationHub.Business.Components;

using VSP;
using VSP.Business.Components;
using VSP.Business.Entities;
using VSP.Security;
using VSP.Presentation;
using VSP.Presentation.Forms;
using VSP.Presentation.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSP.Forms.Controls
{
    public partial class Settings : UserControl
    {
        public frmMain frmMainParent;

        public Settings()
        {
            InitializeComponent();
        }

        public void Initialize(frmMain _frmMain)
        {
            frmMainParent = _frmMain;

            cboSmTable.Items.Clear();

            foreach (DataRow dr in StringMap.GetDistinctTable().Rows)
            {
                cboSmTable.Items.Add(dr["RegardingTable"].ToString());
            }

            cboSmTable.SelectedIndex = 0;
        }

        private void cboSmTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            string table = cboSmTable.Text.ToString();

            cboSmColumn.Items.Clear();

            txtSmValue.Text = null;
            //lstStringMapValues.SelectedIndex = -1;

            foreach (DataRow dr in VSP.Business.Entities.StringMap.GetDistinctColumnFromTable(table).Rows)
            {
                cboSmColumn.Items.Add(dr["RegardingColumn"].ToString());
            }

            if (cboSmColumn.Items.Count > 0)
                cboSmColumn.SelectedIndex = 0;
        }

        private void cboSmColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            string table = cboSmTable.Text;
            string column = cboSmColumn.Text;

            lstStringMapValues.Items.Clear();

            txtSmValue.Text = null;
            lstStringMapValues.SelectedIndex = -1;

            foreach (DataRow dr in VSP.Business.Entities.StringMap.GetAssociatedFromTableColumn(table, column).Rows)
            {
                lstStringMapValues.Items.Add(new ListItem(dr["Name"].ToString(), dr["StringMapId"].ToString()));
            }
        }

        private void lstStringMapValues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstStringMapValues.SelectedItem == null)
                return;

            if (lstStringMapValues.Items.Count > 0 && lstStringMapValues.SelectedIndex != -1)
                txtSmValue.Text = lstStringMapValues.SelectedItem.ToString();
        }

        private void btnSmUpdate_Click(object sender, EventArgs e)
        {
            if (lstStringMapValues.SelectedIndex == -1 || lstStringMapValues.Items.Count == 0)
            {
                MessageBox.Show("Error: No String Map value is selected. Please resolve and try again.", "Error!", MessageBoxButtons.OK);
                return;
            }

            Guid stringMapId = new Guid(((ListItem)lstStringMapValues.SelectedItem).HiddenValue);
            string table = cboSmTable.SelectedItem.ToString();
            string column = cboSmColumn.SelectedItem.ToString();
            string value = txtSmValue.Text;

            lstStringMapValues.Items.Clear();
            txtSmValue.Text = null;

            try
            {
                StringMap stringMap = new StringMap(stringMapId);
                stringMap.Value = value;
                stringMap.SaveRecordToDatabase(frmMainParent.CurrentUser.UserId);

                foreach (DataRow dr in VSP.Business.Entities.StringMap.GetAssociatedFromTableColumn(table, column).Rows)
                {
                    lstStringMapValues.Items.Add(new ListItem(dr["Name"].ToString(), dr["StringMapId"].ToString()));
                }
            }
            catch (Exception ex)
            {
                frmError _frmError = new frmError(frmMainParent, ex);
            }
        }

        private void btnSmInsert_Click(object sender, EventArgs e)
        {
            string table = cboSmTable.SelectedItem.ToString();
            string column = cboSmColumn.SelectedItem.ToString();
            string value = txtSmValue.Text;

            lstStringMapValues.Items.Clear();
            txtSmValue.Text = null;

            try
            {
                StringMap stringMap = new StringMap();
                stringMap.Value = value;
                stringMap.RegardingColumn = column;
                stringMap.RegardingTable = table;
                stringMap.SaveRecordToDatabase(frmMainParent.CurrentUser.UserId);

                foreach (DataRow dr in VSP.Business.Entities.StringMap.GetAssociatedFromTableColumn(table, column).Rows)
                {
                    lstStringMapValues.Items.Add(new ListItem(dr["Name"].ToString(), dr["StringMapId"].ToString()));
                }
            }
            catch (Exception ex)
            {
                frmError _frmError = new frmError(frmMainParent, ex);
            }
        }

        private void btnSmDelete_Click(object sender, EventArgs e)
        {
            string table = cboSmTable.SelectedItem.ToString();
            string column = cboSmColumn.SelectedItem.ToString();
            string value = txtSmValue.Text;

            Guid StringMapId = new Guid(((ListItem)lstStringMapValues.SelectedItem).HiddenValue);

            DialogResult result = MessageBox.Show("Are you sure you wish to delete " + value + "?", "Attention", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                try
                {
                    lstStringMapValues.Items.Clear();

                    StringMap stringMap = new StringMap(StringMapId);
                    stringMap.DeleteRecordFromDatabase();

                    foreach (DataRow dr in StringMap.GetAssociatedFromTableColumn(table, column).Rows)
                    {
                        lstStringMapValues.Items.Add(new ListItem(dr["Name"].ToString(), dr["StringMapId"].ToString()));
                    }

                    txtSmValue.Text = null;
                }
                catch (Exception ex)
                {
                    frmError _frmError = new frmError(frmMainParent, ex);
                }
            }
        }
    }
}
