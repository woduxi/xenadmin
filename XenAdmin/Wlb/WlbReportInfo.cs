/* Copyright (c) Citrix Systems Inc. 
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
using System.Text;
using System.Xml;
using XenAPI;
using XenAdmin.Core;
using XenAdmin.Wlb;
using XenAdmin.Dialogs;
using XenAdmin.Actions;
using XenAdmin.Help;
using System.Windows.Forms;

namespace XenAdmin.Wlb
{

    /// <summary>
    /// ReportInfo describes the essential pieces required to successfully render a report
    /// within the ReportView control.  
    /// </summary>
    public class WlbReportInfo
    {

        #region Variables

        private string _reportName;
        private string _reportFile;
        private string _reportDefinition;
        private bool _displayHosts;
        private bool _displayFilter;
        private List<string> _reportQueryParameterNames;

        #endregion


        #region Properties

        /// <summary>
        /// ReportInfo constructor
        /// </summary>
        /// <param name="reportName">The name of the report to be rendered</param>
        /// <param name="reportFile">The RDLC file name for the report</param>
        /// <param name="DisplayHosts">Whether or not the report requires a host parameter value</param>
        /// <param name="reportQueryParameterNames">List of parameter names whose values are required for the SQL query</param>
        public WlbReportInfo(string reportName, 
                             string reportFile, 
                             string reportDefinition, 
                             bool DisplayHosts, 
                             bool DisplayFilter,
                             List<string> reportQueryParameterNames)
        {
            this._reportName = reportName;
            this._reportFile = reportFile;
            this._reportDefinition = reportDefinition;
            this._displayHosts = DisplayHosts;
            this._displayFilter = DisplayFilter;
            this._reportQueryParameterNames = reportQueryParameterNames;
        }


        /// <summary>
        /// The name of the report to be rendered
        /// </summary>
        public string ReportName
        {
            get { return _reportName; }
        }


        /// <summary>
        /// The RDLC file name for the report
        /// </summary>
        public string ReportFile
        {
            get { return _reportFile; }
        }


        /// <summary>
        /// Definition for the current report
        /// </summary>
        public string ReportDefinition
        {
            get { return _reportDefinition; }
        }


        /// <summary>
        /// Whether or not the report requires a host parameter value
        /// </summary>
        public bool DisplayHosts
        {
            get { return _displayHosts; }
        }


        /// <summary>
        /// Whether or not the report requires a filter parameter value
        /// </summary>
        public bool DisplayFilter
        {
            get { return _displayFilter; }
        }


        /// <summary>
        /// List of parameter names whose values are required for the SQL query
        /// </summary>
        public List<string> ReportQueryParameterNames
        {
            get { return _reportQueryParameterNames; }
        }

        #endregion

    }

 }