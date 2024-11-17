using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CTSLibrary;

namespace CTSIntegratorService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);

            //DBToFileFunc obj = new DBToFileFunc();
            //DataTable dt = new DataTable();
            //int count = 0;
            //string result = "";

            //string sStatus = GlobalClass.ReadConfig();

            //string sCabname, sCabUser, sCabPswd;
            //OmniFlow.CTSError oRetVal = new OmniFlow.CTSError();

            //sCabname = GlobalClass.udtTag_CommonImgDetails.CabinetName;
            //sCabUser = GlobalClass.udtTag_CommonImgDetails.sUser;
            //sCabPswd = GlobalClass.udtTag_CommonImgDetails.sPassword;

            //GlobalClass.objOmniFlow.ImageServerIP = GlobalClass.udtTag_CommonImgDetails.ImageServerIP;
            //GlobalClass.objOmniFlow.VolumeName = GlobalClass.udtTag_CommonImgDetails.VolumeName;
            //GlobalClass.objOmniFlow.PortId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.PortId);
            //GlobalClass.objOmniFlow.VolId = Convert.ToInt16(GlobalClass.udtTag_CommonImgDetails.VolId);
            //GlobalClass.objOmniFlow.ProcessId = Convert.ToInt32(GlobalClass.udtTag_CommonImgDetails.ProcessID);

            //oRetVal = GlobalClass.objOmniFlow.ConnectToWorkFlow(sCabname, sCabUser, sCabPswd, 1, ref GlobalClass.oudtWMTSessionHandle);
            //if (oRetVal.main_code != "SUCCESS")
            //{
            //    //Connect to workflow fail
            //}
            //else
            //    // connect to workflow success


            //    result = obj.StartDBToFileTransfer();
            //if (result.ToString() != GlobalClass.CTS_SUCCESS)
            //{

            //}

            //sStatus = obj.GetDataFromDB(ref dt);
            //if (sStatus != GlobalClass.CTS_SUCCESS)
            //{
            //    // Error at geeting Data from Table
            //    //return GlobalClass.CTS_FAIL;
            //}
            //else
            //{
            //    if (dt != null )
            //    {
            //        if (dt.DataSet.Tables["Result"] != null && dt.DataSet.Tables["Result"].Rows.Count > 0)
            //        {
            //            if (GlobalClass.udtTag_CTSIntegrator.Output_CSV == "Y")
            //            {
            //                obj.CreateCSV(dt.DataSet.Tables["Result"], GlobalClass.DestFilePath);
            //            }
            //            if (GlobalClass.udtTag_CTSIntegrator.Output_XML == "Y")
            //            {
            //                obj.CreateXML(dt.DataSet.Tables["Result"], GlobalClass.DestFilePath);
            //            }
            //            if (GlobalClass.udtTag_CTSIntegrator.Output_JSON == "Y")
            //            {
            //                obj.CreateJSON(dt.DataSet.Tables["Result"]);
            //            }
            //            if (GlobalClass.udtTag_CTSIntegrator.Output_TXT == "Y")
            //            {
            //                obj.CreateTXT(dt.DataSet.Tables["Result"]);
            //            }

            //            // Get Image for all the records.
            //            if (GlobalClass.udtTag_CTSIntegrator.isImageRequired == "Y") {

            //                if (GlobalClass.udtTag_CTSIntegrator.Output_CSV == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_XML == "Y"
            //               || GlobalClass.udtTag_CTSIntegrator.Output_JSON == "Y" || GlobalClass.udtTag_CTSIntegrator.Output_TXT == "Y")
            //                {
            //                    sStatus = obj.GetImageForRecords(dt);
            //                }
            //            }
            //            else
            //            {
            //                // Image download not required
            //            }

            //            sStatus = obj.UpdateDataIntoDB();
            //        }
            //    }
            //}

        }
    }
}
