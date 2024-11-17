/* -----------------------------------------------------------------------
* Product/Project Name: CTS-Enhancement
* Module: CTS Integrator CTS-ITG-DBToFile
* File: Service1.cs
* Purpose: Contains service functions



* * Functions Defined:
1) OnStart()
2) OnStop()

* /// <CHANGE HISTORY>
* ///
* ///-----------------------------------------------------------------------
* /// Date         :           Change By         :   Change Description
    10-03-2023              Garima Barthwal         Added lock object to lock the OnStart and OnStop thread


* /// </CHANGE HISTORY>
-----------------------------------------------------------------------*/
using CTSLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTSIntegratorService
{
    public partial class Service1 : ServiceBase
    {
        private bool StopFlag = false;
        public static Task mainTask = null;
        private static object OnStoplocker = new object();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string sStatus = "";
                string sCabname, sCabUser, sCabPswd;
                OmniFlow.CTSError oRetVal = new OmniFlow.CTSError();
                DBToFileFunc objDB = new DBToFileFunc();

                sStatus = GlobalClass.ReadConfig();
                if (sStatus != GlobalClass.CTS_SUCCESS)
                {
                    throw new Exception("Error at reading Config: " + sStatus);
                }
                
                sCabname = GlobalClass.udtTag_CommonImgDetails.CabinetName;
                sCabUser = GlobalClass.udtTag_CommonImgDetails.sUser;
                sCabPswd = GlobalClass.udtTag_CommonImgDetails.sPassword;

                GlobalClass.objOmniFlow.ImageServerIP = GlobalClass.udtTag_CommonImgDetails.ImageServerIP;
                GlobalClass.objOmniFlow.VolumeName = GlobalClass.udtTag_CommonImgDetails.VolumeName;
                GlobalClass.objOmniFlow.PortId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.PortId);
                GlobalClass.objOmniFlow.VolId = Convert.ToInt16(GlobalClass.udtTag_CommonImgDetails.VolId);
                GlobalClass.objOmniFlow.ProcessId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.ProcessID);

                oRetVal = GlobalClass.objOmniFlow.ConnectToWorkFlow(sCabname, sCabUser, sCabPswd, 1, ref GlobalClass.oudtWMTSessionHandle);
                if (oRetVal.main_code != "SUCCESS")
                {
                    //Connect to workflow fail
                    throw new Exception("Error at ConnectToWorkFlow: " + oRetVal.main_code);
                }
                else
                {
                    GlobalClass.WriteLog("OnStart", GlobalClass.LogLevel.Debug, "Success at ConnectToWorkflow.", false);

                }
                // Log connectToWorkflow success

                lock (OnStoplocker)
                {
                    if (!GlobalClass.isStopCommandFired)
                    {
                        mainTask = new Task(() =>
                        {
                            while (true)
                            {
                                var result = objDB.StartDBToFileTransfer();
                                if (result.ToString() != GlobalClass.CTS_SUCCESS)
                                {
                                    throw new Exception("Could not Transfer from DB to File: " + result);
                                }
                                Thread.Sleep(10000);
                            }
                        }
                );
                        mainTask.Start();
                    }
                }

            }
            catch (Exception ex)
            {
                // Log Ex
                GlobalClass.WriteLog("OnStart", GlobalClass.LogLevel.Error, "Exception at OnStart: " + ex.Message, true);
                Task.WhenAll(mainTask);
                //StopFlag = true;
                this.OnStop();
            }

        }

        // Bug ID: 1132303
        // Bug: CTS-Integrator: Unable to stop CTS-ITG-DBToFile Service.
        // Added By : Garima Barthwal
        // Added on: 21-03-2023
        //Resolution : Added lock object to lock the OnStart and OnStop thread
        protected override void OnStop()
        {
            OmniFlow.CTSError oRetVal = new OmniFlow.CTSError();
            try
            {
                lock (OnStoplocker) {
                    GlobalClass.isStopCommandFired = true;
                    //if (StopFlag == false)

                    if (mainTask != null)
                        Task.WhenAll(mainTask);

                    oRetVal = GlobalClass.objOmniFlow.DisconnectFromWorkflow();
                    if (oRetVal.main_code != "SUCCESS")
                    { 
                        oRetVal = GlobalClass.objOmniFlow.DisconnectFromWorkflow();
                        if (oRetVal.main_code != "SUCCESS")
                        {
                            GlobalClass.WriteLog("OnStop", GlobalClass.LogLevel.Error, "Error at DisconnectFromWorkkflow: " + oRetVal.main_code, true);

                        }
                        else
                        {
                            GlobalClass.WriteLog("OnStop", GlobalClass.LogLevel.Debug, "Disconnected From Workkflow.", false);

                        }
                    }
                    else
                    {
                        GlobalClass.WriteLog("OnStop", GlobalClass.LogLevel.Debug, "Disconnected From Workkflow.", false);

                    }
                }
            }
            catch (Exception e)
            {
                GlobalClass.WriteLog("OnStop", GlobalClass.LogLevel.Error, "Exception at OnStop: " + e.Message, true);

            }
        }
    }
}
