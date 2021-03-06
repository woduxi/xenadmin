﻿/* Copyright (c) Citrix Systems Inc. 
 * All rights reserved. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using XenAdmin.Actions;
using XenAdmin.Controls;
using XenAdmin.Core;
using XenAdmin.Properties;
using XenAPI;


namespace XenAdmin.SettingsPanels
{
    public partial class GpuEditPage : XenTabPage, IEditPage
    {
        public VM vm;
        private GpuTuple currentGpuTuple;
        private GPU_group[] gpu_groups;
        private bool gpusAvailable;

        public GpuEditPage()
        {
            InitializeComponent();
        }

        public GPU_group GpuGroup
        {
            get
            {
                GpuTuple tuple = comboBoxGpus.SelectedItem as GpuTuple;
                return tuple == null ? null : tuple.GpuGroup;
            }
        }

        public VGPU_type VgpuType
        {
            get
            {
                GpuTuple tuple = comboBoxGpus.SelectedItem as GpuTuple;
                if (tuple == null || tuple.VgpuTypes == null || tuple.VgpuTypes.Length == 0)
                    return null;

                return tuple.VgpuTypes[0];
            }
        }

        #region IEditPage Members

        public AsyncAction SaveSettings()
        {
            return new GpuAssignAction(vm, GpuGroup, VgpuType);
        }

        public void SetXenObjects(IXenObject orig, IXenObject clone)
        {
            Trace.Assert(clone is VM);  // only VMs should show this page
            Trace.Assert(Helpers.BostonOrGreater(clone.Connection));  // If not Boston or greater, we shouldn't see this page
            Trace.Assert(!Helpers.FeatureForbidden(clone, Host.RestrictGpu));  // If license insufficient, we show upsell page instead

            vm = (VM)clone;

            if (Connection == null) // on the PropertiesDialog
                Connection = vm.Connection;

            PopulatePage();
        }

        public bool ValidToSave
        {
            get { return true; }
        }

        public void ShowLocalValidationMessages()
        {
        }

        public void Cleanup()
        {
        }

        public bool HasChanged
        {
            get
            {
                var tuple = comboBoxGpus.SelectedItem as GpuTuple;
                if (tuple == null)
                    return false;
                return !tuple.Equals(currentGpuTuple);
            }
        }

        #region VerticalTab Members

        public string SubText
        {
            get
            {
                var tuple = comboBoxGpus.SelectedItem as GpuTuple;
                return tuple == null ? "" : tuple.ToString();
            }
        }

        public Image Image
        {
            get { return Resources._000_GetMemoryInfo_h32bit_16; }
        }

        #endregion

        #endregion

        #region XenTabPage overrides

        public override string Text
        {
            get { return Messages.GPU; }
        }

        public override string PageTitle
        {
            get { return Messages.NEWVMWIZARD_VGPUPAGE_TITLE; }
        }

        public override string HelpID
        {
            get { return "GPU"; }
        }

        public override List<KeyValuePair<string, string>> PageSummary
        {
            get
            {
                var summ = new List<KeyValuePair<string, string>>();

                GpuTuple tuple = comboBoxGpus.SelectedItem as GpuTuple;
                if (tuple != null)
                    summ.Add(new KeyValuePair<string, string>(Messages.GPU, tuple.ToString()));

                return summ;
            }
        }
        
        public override void PopulatePage()
        {
            currentGpuTuple = new GpuTuple(null, null);

            if (vm.VGPUs.Count != 0)
            {
                VGPU vgpu = Connection.Resolve(vm.VGPUs[0]);
                if (vgpu != null)
                {
                    var vgpuGroup = Connection.Resolve(vgpu.GPU_group);

                    if (Helpers.FeatureForbidden(Connection, Host.RestrictVgpu))
                        currentGpuTuple = new GpuTuple(vgpuGroup, null);
                    else
                    {
                        VGPU_type vgpuType = Connection.Resolve(vgpu.type);
                        currentGpuTuple = new GpuTuple(vgpuGroup, new[] { vgpuType });
                    }
                }
            }

            gpu_groups = Connection.Cache.GPU_groups;
            gpusAvailable = gpu_groups.Length > 0;

            if (!gpusAvailable)
            {
                labelGpuType.Visible = comboBoxGpus.Visible = false;
                labelRubric.Text = Helpers.GetPool(Connection) == null
                                       ? Messages.GPU_RUBRIC_NO_GPUS_SERVER
                                       : Messages.GPU_RUBRIC_NO_GPUS_POOL;
            }
            else
            {
                var noneItem = new GpuTuple(null, null);
                comboBoxGpus.Items.Add(noneItem);

                Array.Sort(gpu_groups);
                foreach (GPU_group gpu_group in gpu_groups)
                {
                    if (Helpers.FeatureForbidden(Connection, Host.RestrictVgpu))
                    {
                        comboBoxGpus.Items.Add(new GpuTuple(gpu_group, null));
                    }
                    else
                    {
                        var enabledRefs = GPU_group.get_enabled_VGPU_types(Connection.Session, gpu_group.opaque_ref);
                        var enabledTypes = Connection.ResolveAll(enabledRefs);

                        if (enabledTypes.Count > 1)
                        {
                            enabledTypes.Sort((t1, t2) =>
                            {
                                int result = t1.max_heads.CompareTo(t2.max_heads);
                                if (result != 0)
                                    return result;
                                return t1.Name.CompareTo(t2.Name);
                            });

                            comboBoxGpus.Items.Add(new GpuTuple(gpu_group, enabledTypes.ToArray()));

                            foreach (var vgpuType in enabledTypes)
                                comboBoxGpus.Items.Add(new GpuTuple(gpu_group, new[] { vgpuType }));
                        }
                        else
                            comboBoxGpus.Items.Add(new GpuTuple(gpu_group, null));
                    }
                }

                foreach (var item in comboBoxGpus.Items)
                {
                    var tuple = item as GpuTuple;
                    if (tuple == null)
                        continue;

                    if (tuple.Equals(currentGpuTuple))
                    {
                        comboBoxGpus.SelectedItem = item;
                        break;
                    }
                }
            }

            ShowHideWarnings();
        }

        #endregion

        private void ShowHideWarnings()
        {
            if (!gpusAvailable)
            {
                imgRDP.Visible = labelRDP.Visible =
                imgNeedDriver.Visible = labelNeedDriver.Visible =
                imgNeedGpu.Visible = labelNeedGpu.Visible =
                imgStopVM.Visible = labelStopVM.Visible =
                    false;
                return;
            }

            if (vm.power_state != vm_power_state.Halted)
            {
                imgRDP.Visible = labelRDP.Visible =
                imgNeedDriver.Visible = labelNeedDriver.Visible =
                imgNeedGpu.Visible = labelNeedGpu.Visible =
                    false;

                imgStopVM.Visible = labelStopVM.Visible = true;

                labelGpuType.Enabled = comboBoxGpus.Enabled = false;
                return;
            }

            GpuTuple tuple = comboBoxGpus.SelectedItem as GpuTuple;

            imgStopVM.Visible = labelStopVM.Visible = false;
            imgRDP.Visible = labelRDP.Visible =
                HasChanged && currentGpuTuple.GpuGroup == null &&
                tuple != null && !tuple.IsFractionalVgpu;

            imgNeedGpu.Visible = labelNeedGpu.Visible =
            labelNeedDriver.Visible = imgNeedDriver.Visible =
                tuple != null && tuple.GpuGroup != null;
        }

        private void comboBoxGpus_SelectedIndexChanged(object sender, EventArgs e)
        {
            warningsTable.SuspendLayout();
            ShowHideWarnings();
            warningsTable.ResumeLayout();
        }

        private void warningsTable_SizeChanged(object sender, EventArgs e)
        {
            int[] columnsWidth = warningsTable.GetColumnWidths();
            int textColumnWidth = columnsWidth.Length > 1 ? columnsWidth[1] : 1;

            labelRDP.MaximumSize = labelStopVM.MaximumSize = new Size(textColumnWidth, 999);
        }
    }
}
